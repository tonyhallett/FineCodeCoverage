using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using FineCodeCoverage.Engine;
using FineCodeCoverage.Options;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.TestWindow.Extensibility;
using Task = System.Threading.Tasks.Task;
using Microsoft.VisualStudio.Utilities;
using FineCodeCoverage.Engine.MsTestPlatform.CodeCoverage;
using System.Threading;
using FineCodeCoverage.Initialization;
using FineCodeCoverage.Core.Utilities;
using FineCodeCoverage.Engine.Messages;

namespace FineCodeCoverage.Impl
{
    [Name(Vsix.TestContainerDiscovererName)]
    [Export(typeof(ITestContainerDiscoverer))]
    internal class TestContainerDiscoverer : ITestContainerDiscoverer
    {
#pragma warning disable 67
        public event EventHandler TestContainersUpdated;

        private readonly IEventAggregator eventAggregator;
#pragma warning restore 67
        private readonly IFCCEngine fccEngine;
        private readonly ITestOperationStateInvocationManager testOperationStateInvocationManager;
        private readonly ITestOperationFactory testOperationFactory;
        private readonly ILogger logger;
        private readonly IAppOptionsProvider appOptionsProvider;
        private readonly IMsCodeCoverageRunSettingsService msCodeCoverageRunSettingsService;
        internal Dictionary<TestOperationStates, Func<IOperation, Task>> testOperationStateChangeHandlers;
        private bool cancelling;
        private MsCodeCoverageCollectionStatus msCodeCoverageCollectionStatus;
        private bool runningInParallel;
        private bool coverageDisabledWhenStarting;
        private IAppOptions settings;
        
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
            IEventAggregator eventAggregator,
            IFCCEngine fccEngine,
            ITestOperationStateInvocationManager testOperationStateInvocationManager,
            IPackageLoader packageLoader,
            ITestOperationFactory testOperationFactory,
            ILogger logger,
            IAppOptionsProvider appOptionsProvider,
            IMsCodeCoverageRunSettingsService msCodeCoverageRunSettingsService
        )
        {
            this.appOptionsProvider = appOptionsProvider;
            this.msCodeCoverageRunSettingsService = msCodeCoverageRunSettingsService;
            this.eventAggregator = eventAggregator;
            this.fccEngine = fccEngine;
            this.testOperationStateInvocationManager = testOperationStateInvocationManager;
            this.testOperationFactory = testOperationFactory;
            this.logger = logger;
            testOperationStateChangeHandlers = new Dictionary<TestOperationStates, Func<IOperation, Task>>
            {
                { TestOperationStates.TestExecutionCanceling, TestExecutionCancellingAsync},
                { TestOperationStates.TestExecutionStarting, TestExecutionStartingAsync},
                { TestOperationStates.TestExecutionFinished, TestExecutionFinishedAsync},
                { TestOperationStates.TestExecutionCancelAndFinished, TestExecutionCancelAndFinishedAsync},
            };
            _ = packageLoader.LoadPackageAsync(CancellationToken.None);
            operationState.StateChanged += OperationState_StateChanged;
        }

        private enum CollectWhenTestExecutionFinished
        { 
            Yes, Disabled, CoverageConditionsNotMet, RunningInParallel, MsCodeCoverageErrored
        }

        internal Action<Func<Task>> RunAsync = (taskProvider) =>
        {
            ThreadHelper.JoinableTaskFactory.Run(taskProvider);
        };

        private bool CoverageDisabled(IAppOptions settings)
        {
            return !settings.Enabled  && settings.DisabledNoCoverage;
        }

        private async Task TestExecutionStartingAsync(IOperation operation)
        {
            cancelling = false;
            runningInParallel = false;
            StopCoverage();
            msCodeCoverageCollectionStatus = MsCodeCoverageCollectionStatus.NotCollecting;
            var settings = appOptionsProvider.Get();
            coverageDisabledWhenStarting = CoverageDisabled(settings);
            if (coverageDisabledWhenStarting)
            {
                logger.Log("Coverage not collected as FCC disabled.");
                return;
            }

            msCodeCoverageCollectionStatus = await msCodeCoverageRunSettingsService.IsCollectingAsync(testOperationFactory.Create(operation));
            if (msCodeCoverageCollectionStatus == MsCodeCoverageCollectionStatus.NotCollecting)
            {
                if (settings.RunInParallel)
                {
                    RaiseCoverageStarted(true);
                    runningInParallel = true;
                    fccEngine.ReloadCoverage(() =>
                    {
                        return testOperationFactory.Create(operation).GetCoverageProjectsAsync();
                    });
                }
                else
                {
                    RaiseCoverageStarted(false);
                    logger.Log("Coverage collected when tests finish. RunInParallel option true for immediate");
                }
            }

            if(msCodeCoverageCollectionStatus == MsCodeCoverageCollectionStatus.Collecting)
            {
                RaiseCoverageStarted();
            }
        }

        private void RaiseCoverageStarted(bool pending = false)
        {
            eventAggregator.SendMessage(new CoverageStartingMessage(pending));
        }

        private void RaiseCoverageEnded(CollectWhenTestExecutionFinished endReason)
        {
            CoverageEndedStatus coverageEndedStatus = CoverageEndedStatus.Faulted;
            switch (endReason)
            {
                case CollectWhenTestExecutionFinished.Disabled:
                    coverageEndedStatus = CoverageEndedStatus.Disabled;
                    break;
                case CollectWhenTestExecutionFinished.CoverageConditionsNotMet:
                    coverageEndedStatus = CoverageEndedStatus.ConditionsNotMet;
                    break;
            }

            eventAggregator.SendMessage(new CoverageEndedMessage(coverageEndedStatus,null));
        }

        private async Task TestExecutionFinishedAsync(IOperation operation)
        {
            var (should, testOperation) = ShouldConditionallyCollectWhenTestExecutionFinished(operation);
            if (should == CollectWhenTestExecutionFinished.Yes)
            {
                await TestExecutionFinishedCollectionAsync(operation, testOperation);
            }
            else
            {
                if (!coverageDisabledWhenStarting && should != CollectWhenTestExecutionFinished.RunningInParallel)
                {
                    RaiseCoverageEnded(should);
                }
            }
        }

        private (CollectWhenTestExecutionFinished should, ITestOperation testOperation) ShouldConditionallyCollectWhenTestExecutionFinished(IOperation operation)
        {
            CollectWhenTestExecutionFinished collectWhenTestExecutionFinished = ShouldNotCollectWhenTestExecutionFinished();
            if (collectWhenTestExecutionFinished != CollectWhenTestExecutionFinished.Yes)
            {
                return (collectWhenTestExecutionFinished, null);
            }
            
            var testOperation = testOperationFactory.Create(operation);
            
            var coverageConditionsMet = CoverageConditionsMet(testOperation);
            return (coverageConditionsMet ? CollectWhenTestExecutionFinished.Yes : CollectWhenTestExecutionFinished.CoverageConditionsNotMet, testOperation);
        }

        private CollectWhenTestExecutionFinished ShouldNotCollectWhenTestExecutionFinished()
        {
            if (runningInParallel)
            {
                return CollectWhenTestExecutionFinished.RunningInParallel;
            }

            if (MsCodeCoverageErrored)
            {
                return CollectWhenTestExecutionFinished.MsCodeCoverageErrored;
            }

            settings = appOptionsProvider.Get();
            var disabled = CoverageDisabled(settings);
            return disabled ? CollectWhenTestExecutionFinished.Disabled : CollectWhenTestExecutionFinished.Yes;
            
        }

        private async Task TestExecutionFinishedCollectionAsync(IOperation operation, ITestOperation testOperation)
        {
            if (msCodeCoverageCollectionStatus == MsCodeCoverageCollectionStatus.Collecting)
            {
                await msCodeCoverageRunSettingsService.CollectAsync(operation, testOperation);
            }
            else
            {
                fccEngine.ReloadCoverage(testOperation.GetCoverageProjectsAsync);
            }
        }

        private bool CoverageConditionsMet(ITestOperation testOperation)
        {
            if (!settings.RunWhenTestsFail && testOperation.FailedTests > 0)
            {
                logger.Log($"Skipping coverage due to failed tests.  Option {nameof(AppOptions.RunWhenTestsFail)} is false");
                return false;
            }

            var totalTests = testOperation.TotalTests;
            var runWhenTestsExceed = settings.RunWhenTestsExceed;
            if (totalTests > 0) // in case this changes to not reporting total tests
            {
                if (totalTests <= runWhenTestsExceed)
                {
                    logger.Log($"Skipping coverage as total tests ({totalTests}) <= {nameof(AppOptions.RunWhenTestsExceed)} ({runWhenTestsExceed})");
                    return false;
                }
            }
            return true;
        }
        
        private void StopCoverage()
        {
            switch (msCodeCoverageCollectionStatus)
            {
                case MsCodeCoverageCollectionStatus.Collecting:
                    msCodeCoverageRunSettingsService.StopCoverage();
                    break;
                case MsCodeCoverageCollectionStatus.NotCollecting:
                    fccEngine.StopCoverage();
                    break;
            }
        }

        private Task CoverageCancelledAsync(string logMessage, IOperation operation)
        {
            logger.Log(logMessage);
            fccEngine.StopCoverage();
            return NotifyMsCodeCoverageTestExecutionNotFinishedIfCollectingAsync(operation);
        }

        private async Task NotifyMsCodeCoverageTestExecutionNotFinishedIfCollectingAsync(IOperation operation)
        {
            if (msCodeCoverageCollectionStatus == MsCodeCoverageCollectionStatus.Collecting)
            {
                var testOperation = testOperationFactory.Create(operation);
                await msCodeCoverageRunSettingsService.TestExecutionNotFinishedAsync(testOperation);
            }
        }

        private void OperationState_StateChanged(object sender, OperationStateChangedEventArgs e)
        {
            RunAsync(async () =>
            {
                await TryAndLogExceptionAsync(() => OperationState_StateChangedAsync(e));
            });
        }

        private async Task TestExecutionCancellingAsync(IOperation operation)
        {
            cancelling = true;
            await CoverageCancelledAsync("Test execution cancelling - running coverage will be cancelled.", operation);
        }

        private async Task TestExecutionCancelAndFinishedAsync(IOperation operation)
        {
            if (!cancelling)
            {
                await CoverageCancelledAsync("There has been an issue running tests. See the Tests output window pane.", operation);
            }
        }

        private async Task OperationState_StateChangedAsync(OperationStateChangedEventArgs e)
        {
            if (testOperationStateChangeHandlers.TryGetValue(e.State, out var handler)) {
                if (testOperationStateInvocationManager.CanInvoke(e.State))
                {
                    await handler(e.Operation);
                }
            }
        }

        private async Task TryAndLogExceptionAsync(Func<Task> action)
        {
            try
            {
                await action();
                    
            }
            catch (Exception exception)
            {
                logger.Log("Error processing unit test events", exception);
            }
        }
    }
}
