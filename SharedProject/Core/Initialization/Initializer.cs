using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Threading;
using System.Threading.Tasks;
using FineCodeCoverage.Core.Utilities;
using FineCodeCoverage.Engine;
using FineCodeCoverage.Impl;
using FineCodeCoverage.Output;
using Microsoft.VisualStudio.TestWindow.Extensibility;
using ILogger = FineCodeCoverage.Logging.ILogger;

namespace FineCodeCoverage.Core.Initialization
{
    [Export(typeof(ITestInstantiationPathAware))]
    [Export(typeof(IInitializeStatusProvider))]
    [Export(typeof(IPackageInitializeAware))]
    internal class Initializer : 
        IInitializeStatusProvider, ITestInstantiationPathAware, IPackageInitializeAware
    {
        private readonly ILogger logger;
        private readonly IDisposeAwareTaskRunner disposeAwareTaskRunner;
        private readonly IEnumerable<IRequireInitialization> requiresInitialization;

        internal const string initializationFailedMessagePrefix = "Initialization failed.  Please check the following error which may be resolved by reopening visual studio which will start the initialization process again.";
        internal int initializeWait = 5000;
        internal Task initializeTask;

        public InitializeStatus InitializeStatus { get; set; } = InitializeStatus.Initializing;
        private bool Initialized => InitializeStatus == InitializeStatus.Initialized;
        public string InitializeExceptionMessage { get; set; }
        private bool startedInitialization = false;

        [ImportingConstructor]
        public Initializer(
            [ImportMany]
            IEnumerable<IRequireInitialization> requiresInitialization,
            ILogger logger,
            IDisposeAwareTaskRunner disposeAwareTaskRunner
        )
        {
            this.logger = logger;
            this.disposeAwareTaskRunner = disposeAwareTaskRunner;
            this.requiresInitialization = requiresInitialization;
        }

        
        #region IInitializeStatusProvider
        private void ThrowIfInitializationFailed()
        {
            if (InitializeStatus == InitializeStatus.Error)
            {
                throw new Exception($"{initializationFailedMessagePrefix} {InitializeExceptionMessage}");
            }
        }

        private Task WaitForInitializationAsync()
        {
            logger.Log(CoverageStatus.Initializing.Message());
            return Task.Delay(initializeWait);
        }

        public async Task WaitForInitializedAsync(CancellationToken cancellationToken)
        {
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();
                ThrowIfInitializationFailed();
                if (Initialized)
                {
                    return;
                }
                await WaitForInitializationAsync();
            }
        }

        #endregion
        
        void ITestInstantiationPathAware.Notify(IOperationState operationState)
        {
            InitializeIfNotInitializing();
        }

        void IPackageInitializeAware.Notify()
        {
            InitializeIfNotInitializing();
        }

        private void InitializeIfNotInitializing()
        {
            if (!startedInitialization)
            {
                startedInitialization = true;
                disposeAwareTaskRunner.RunAsync(RunInitializeTaskAsync);
            }
        }

        private Task RunInitializeTaskAsync()
        {
            initializeTask = Task.Run(InitializeAsync);
            return initializeTask;
        }

        private Task InitializeAsync()
        {
            return InitializeAsync(disposeAwareTaskRunner.DisposalToken);
        }

        private async Task InitializeAsync(CancellationToken cancellationToken)
        {
            try
            {
                await DoInitializeAsync(cancellationToken);
            }
            catch (Exception exception)
            {
                InitializeStatus = InitializeStatus.Error;
                InitializeExceptionMessage = exception.Message;
                if (!cancellationToken.IsCancellationRequested)
                {
                    logger.Log($"Failed Initialization", exception);
                }
            }
        }

        private async Task DoInitializeAsync(CancellationToken cancellationToken)
        {
            logger.Log($"Initializing");

            foreach (var requireInitialization in requiresInitialization)
            {
                cancellationToken.ThrowIfCancellationRequested();
                await requireInitialization.InitializeAsync(cancellationToken);
            }

            cancellationToken.ThrowIfCancellationRequested();

            InitializeStatus = InitializeStatus.Initialized;

            logger.Log($"Initialized");
        }

    }

}

