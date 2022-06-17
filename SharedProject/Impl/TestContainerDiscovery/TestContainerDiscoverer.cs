using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using FineCodeCoverage.Core.Utilities;
using FineCodeCoverage.Options;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.TestWindow.Extensibility;
using Task = System.Threading.Tasks.Task;
using Microsoft.VisualStudio.Utilities;
using FineCodeCoverage.Engine.MsTestPlatform.CodeCoverage;
using ILogger = FineCodeCoverage.Logging.ILogger;
using FineCodeCoverage.Output.JsMessages.Logging;
using FineCodeCoverage.Output.HostObjects;
using FineCodeCoverage.Output.JsMessages;
using FineCodeCoverage.Core;

namespace FineCodeCoverage.Impl
{
    [Name(Vsix.TestContainerDiscovererName)]
    // Both exports necessary !
    [Export(typeof(TestContainerDiscoverer))]
    [Export(typeof(ITestContainerDiscoverer))]
    internal class TestContainerDiscoverer : ITestContainerDiscoverer
    {
#pragma warning disable 67
        public event EventHandler TestContainersUpdated;
#pragma warning restore 67
        private readonly ITestOperationFactory testOperationFactory;
        private readonly IRunCoverageConditions runCoverageConditions;
        private readonly IInitializer initializer;
        private readonly IAppOptionsProvider appOptionsProvider;
        private readonly IDisposeAwareTaskRunner disposeAwareTaskRunner;
        private readonly ILogger logger;
        private readonly IMsCodeCoverageRunSettingsService msCodeCoverageRunSettingsService;
        private readonly IOldStyleCoverage oldStyleCoverage;
        private readonly IEventAggregator eventAggregator;

        private readonly Dictionary<TestOperationStates, Func<IOperation, Task>> testOperationStateChangeHandlers;

        internal ICoverageService coverageService;
        internal bool cancelling;
        internal bool runningInParallel;
        internal MsCodeCoverageCollectionStatus msCodeCoverageCollectionStatus;
        
        private IAppOptions settings;
        internal IAppOptions Settings
        {
            get
            {
                if (settings == null)
                {
                    settings = appOptionsProvider.Provide();
                }
                return settings;
            }
        }
        
        internal Task initializeTask;

        [ExcludeFromCodeCoverage]
        public Uri ExecutorUri => new Uri($"executor://{Vsix.Code}.Executor/v1");
        [ExcludeFromCodeCoverage]
        public IEnumerable<ITestContainer> TestContainers => Enumerable.Empty<ITestContainer>();
        public bool MsCodeCoverageErrored => msCodeCoverageCollectionStatus == MsCodeCoverageCollectionStatus.Error;

        [ImportingConstructor]
        public TestContainerDiscoverer
        (
            [Import(typeof(IOperationState))]
            IOperationState operationState,
            ITestOperationFactory testOperationFactory,
            IRunCoverageConditions runCoverageConditions,
            IInitializer initializer,
            IAppOptionsProvider appOptionsProvider,
            IDisposeAwareTaskRunner disposeAwareTaskRunner,
            IMsCodeCoverageRunSettingsService msCodeCoverageRunSettingsService,
            IOldStyleCoverage oldStyleCoverage,
            IEventAggregator eventAggregator,
            ILogger logger
        )
        {
            this.msCodeCoverageRunSettingsService = msCodeCoverageRunSettingsService;
            this.oldStyleCoverage = oldStyleCoverage;

            this.eventAggregator = eventAggregator;
            this.logger = logger;
            this.testOperationFactory = testOperationFactory;
            this.runCoverageConditions = runCoverageConditions;
            this.initializer = initializer;
            this.appOptionsProvider = appOptionsProvider;
            this.disposeAwareTaskRunner = disposeAwareTaskRunner;
            appOptionsProvider.OptionsChanged += AppOptionsProvider_OptionsChanged;
            testOperationStateChangeHandlers = new Dictionary<TestOperationStates, Func<IOperation, Task>>
            {
                { TestOperationStates.TestExecutionCanceling, TestExecutionCancellingAsync},
                { TestOperationStates.TestExecutionStarting, TestExecutionStartingAsync},
                { TestOperationStates.TestExecutionFinished, TestExecutionFinishedAsync},
                { TestOperationStates.TestExecutionCancelAndFinished, TestExecutionCancelAndFinishedAsync},
                { TestOperationStates.OperationSetFinished, OperationSetFinishedAsync }
            };

            disposeAwareTaskRunner.RunAsync(RunInitializeTaskAsync);

            operationState.StateChanged += OperationState_StateChanged;
        }

        private Task RunInitializeTaskAsync()
        {
            initializeTask = Task.Run(InitializeAsync);
            return initializeTask;
        }

        private Task InitializeAsync()
        {
            return initializer.InitializeAsync(disposeAwareTaskRunner.DisposalToken);
        }

        private void AppOptionsProvider_OptionsChanged(IAppOptions newSettings)
        {
            settings = newSettings;
        }

        
        #region cancelling
        private Task TestExecutionCancellingAsync(IOperation operation)
        {
            cancelling = true;
            return CoverageCancelledAsync(
                "Test execution cancelling - running coverage will be cancelled.", 
                MessageContext.CoverageCancelled, 
                operation
            );
        }

        private Task TestExecutionCancelAndFinishedAsync(IOperation operation)
        {
            if (!cancelling)
            {
                return CoverageCancelledAsync(
                    "There has been an issue running tests. See the Tests output window pane.", 
                    MessageContext.Error, 
                    operation
                );
            }

            return Task.CompletedTask;
        }

        private Task CoverageCancelledAsync(string logMessage, MessageContext messageContext, IOperation operation)
        {
            CombinedLog(logMessage, messageContext);
            StopCoverage();
            return NotifyMsCodeCoverageTestExecutionNotFinishedIfCollectingAsync(operation);
        }

        private Task NotifyMsCodeCoverageTestExecutionNotFinishedIfCollectingAsync(IOperation operation)
        {
            if (msCodeCoverageCollectionStatus == MsCodeCoverageCollectionStatus.Collecting)
            {
                var testOperation = testOperationFactory.Create(operation);
                return msCodeCoverageRunSettingsService.TestExecutionNotFinishedAsync(testOperation);
            }

            return Task.CompletedTask;
        }

        #endregion

        #region TestExecutionStarting
        private Task TestExecutionStartingAsync(IOperation operation)
        {
            // bizarelly it is possible to get TestExecutionCanceling / TestExecutionStarting
            if (!cancelling)
            {
                return TestExecutionStartingAndNotCancellingAsync(operation);
            }

            return Task.CompletedTask;
        }

        private Task TestExecutionStartingAndNotCancellingAsync(IOperation operation)
        {
            StopCoverage();

            if (!Settings.Enabled)
            {
                CombinedLog("Coverage not collected as FCC disabled.", MessageContext.Warning);
                return Task.CompletedTask;
            }

            return TestExecutionStartingAndEnabledAsync(operation);
        }

        private async Task TestExecutionStartingAndEnabledAsync(IOperation operation)
        {
            var testOperation = testOperationFactory.Create(operation);
            msCodeCoverageCollectionStatus = await msCodeCoverageRunSettingsService.IsCollectingAsync(testOperation);
            if (msCodeCoverageCollectionStatus == MsCodeCoverageCollectionStatus.NotCollecting)
            {
                TestExecutionStartingMsCodeCoverageNotCollecting(testOperation);
            }
        }

        private void TestExecutionStartingMsCodeCoverageNotCollecting(ITestOperation testOperation)
        {
            if (Settings.RunInParallel)
            {
                RunInParallel(testOperation);
            }
            else
            {
                LogRunInParallelOptionExistence();
            }
        }

        private void RunInParallel(ITestOperation testOperation)
        {
            runningInParallel = true;
            CollectOldStyleCoverage(testOperation);
        }

        private void LogRunInParallelOptionExistence()
        {
            eventAggregator.SendMessage(
                    new LogMessage
                    {
                        context = MessageContext.Info,
                        message = new LogMessagePart[] {
                                new Emphasized("Coverage collected when tests finish. "),
                                new Emphasized("RunInParallel", Emphasis.Italic),
                                new Emphasized(" option true for immediate"),
                        }
                    }
                );
            logger.Log("Coverage collected when tests finish. RunInParallel option true for immediate");
        }

        #endregion

        #region TestExecutionFinished
        private Task TestExecutionFinishedAsync(IOperation operation)
        {
            var (should, testOperation) = ShouldConditionallyCollectWhenTestExecutionFinished(operation);
            if (should)
            {
                return TestExecutionFinishedCollectAsync(operation, testOperation);
            }
            return Task.CompletedTask;
        }

        private (bool should, ITestOperation testOperation) ShouldConditionallyCollectWhenTestExecutionFinished(IOperation operation)
        {
            if (ShouldNotCollectWhenTestExecutionFinished())
            {
                return (false, null);
            }
            
            var testOperation = testOperationFactory.Create(operation);
            
            var shouldCollect = runCoverageConditions.Met(testOperation, Settings);
            return (shouldCollect, testOperation);
        }

        private bool ShouldNotCollectWhenTestExecutionFinished()
        {
            if (cancelling)
            {
                return true;
            }
            return !Settings.Enabled || runningInParallel || MsCodeCoverageErrored;
            
        }

        private Task TestExecutionFinishedCollectAsync(IOperation operation, ITestOperation testOperation)
        {
            if (msCodeCoverageCollectionStatus == MsCodeCoverageCollectionStatus.Collecting)
            {
                coverageService = msCodeCoverageRunSettingsService;
                return msCodeCoverageRunSettingsService.CollectAsync(operation, testOperation);
            }
            
            
            CollectOldStyleCoverage(testOperation);
            return Task.CompletedTask;
        }
        #endregion

        private void OperationState_StateChanged(object sender, OperationStateChangedEventArgs e)
        {
            RunAsync(() => TryAndLogExceptionAsync(() => OperationState_StateChangedAsync(e)));
        }

        internal Action<Func<Task>> RunAsync = (asyncMethod) => ThreadHelper.JoinableTaskFactory.Run(asyncMethod);

        private Task OperationState_StateChangedAsync(OperationStateChangedEventArgs e)
        {
            if (testOperationStateChangeHandlers.TryGetValue(e.State, out var handler))
            {
                return handler(e.Operation);
            }

            return Task.CompletedTask;
        }

        private async Task TryAndLogExceptionAsync(Func<Task> action)
        {
            try
            {
                await action();
            }
            catch (Exception exception)
            {
                LogException(exception);
            }
        }

        private void LogException(Exception exception)
        {
            logger.Log("Error processing unit test events", exception);
            var logMessage = new LogMessage
            {
                context = MessageContext.Error,
                message = new LogMessagePart[] {
                    new Emphasized("Error processing unit test events, see "),
                    new FCCLink{
                        hostObject = FCCOutputPaneHostObjectRegistration.HostObjectName,
                        methodName = nameof(FCCOutputPaneHostObject.show),
                        title = "FCC Output Pane",
                        ariaLabel = "Open FCC Output Pane"
                    }
                }
            };
            eventAggregator.SendMessage(logMessage);
            eventAggregator.SendMessage(new CoverageStoppedMessage());
        }

        private Task OperationSetFinishedAsync(IOperation operation)
        {
            runningInParallel = false;
            cancelling = false;
            msCodeCoverageCollectionStatus = MsCodeCoverageCollectionStatus.NotCollecting;
            coverageService = null;
            return Task.CompletedTask;
        }
        
        private void StopCoverage()
        {
            if (coverageService != null)
            {
                coverageService.StopCoverage();
            }
        }

        private void CollectOldStyleCoverage(ITestOperation testOperation)
        {
            coverageService = oldStyleCoverage;
            oldStyleCoverage.CollectCoverage(testOperation.GetCoverageProjectsAsync);
        }

        private void CombinedLog(string message, MessageContext messageContext)
        {
            eventAggregator.SendMessage(LogMessage.Simple(messageContext, message));
            logger.Log(message);
        }
    }
}
