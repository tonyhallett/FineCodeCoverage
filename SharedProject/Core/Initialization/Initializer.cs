using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Threading;
using System.Threading.Tasks;
using FineCodeCoverage.Engine;
using FineCodeCoverage.Logging;

namespace FineCodeCoverage.Core.Initialization
{
    [Export(typeof(IInitializer))]
    [Export(typeof(IInitializeStatusProvider))]
    internal class Initializer : IInitializer
    {
        private readonly ILogger logger;
        private readonly IEnumerable<IRequireInitialization> requiresInitialization;

        internal const string initializationFailedMessagePrefix = "Initialization failed.  Please check the following error which may be resolved by reopening visual studio which will start the initialization process again.";
        internal int initializeWait = 5000;

        public InitializeStatus InitializeStatus { get; set; } = InitializeStatus.Initializing;
        private bool Initialized => InitializeStatus == InitializeStatus.Initialized;
        public string InitializeExceptionMessage { get; set; }

        [ImportingConstructor]
        public Initializer(
            [ImportMany]
            IEnumerable<IRequireInitialization> requiresInitialization,
            ILogger logger
        )
        {
            this.logger = logger;
            this.requiresInitialization = requiresInitialization;
        }

        private async Task DoInitializeAsync(CancellationToken cancellationToken)
        {
            logger.Log($"Initializing");

            foreach(var requireInitialization in requiresInitialization)
            {
                cancellationToken.ThrowIfCancellationRequested();
                await requireInitialization.InitializeAsync(cancellationToken);
            }

            cancellationToken.ThrowIfCancellationRequested();
            
            InitializeStatus = InitializeStatus.Initialized;

            logger.Log($"Initialized");
        }

        public async Task InitializeAsync(CancellationToken cancellationToken)
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
    }

}

