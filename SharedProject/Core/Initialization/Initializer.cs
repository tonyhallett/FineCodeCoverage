using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FineCodeCoverage.Core.Utilities;
using FineCodeCoverage.Impl;
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
        private readonly List<IRequireInitialization> requiresInitialization;

        internal const string initializationFailedMessagePrefix = "Initialization failed.  Please check the following error which may be resolved by reopening visual studio which will start the initialization process again.";
        internal int initializeWait = 5000;
        internal Task initializeTask;
        private bool initializing;

        public InitializeStatus InitializeStatus { get; set; } = InitializeStatus.Initializing;

        [ImportingConstructor]
        public Initializer(
            [ImportMany]
            IEnumerable<Lazy<IRequireInitialization,IOrderMetadata>> lazyRequiresInitialization,
            ILogger logger,
            IDisposeAwareTaskRunner disposeAwareTaskRunner
        )
        {
            this.logger = logger;
            this.disposeAwareTaskRunner = disposeAwareTaskRunner;
            this.requiresInitialization = lazyRequiresInitialization.OrderBy(lazyRequires => lazyRequires.Metadata.Order)
                .Select(lazyRequires => lazyRequires.Value).ToList();
        }

        public void PackageInitializing()
        {
            InitializeIfNotInitializing();
        }

        public void TestPathInstantion()
        {
            InitializeIfNotInitializing();
        }

        private void InitializeIfNotInitializing()
        {
            if (!initializing)
            {
                initializing = true;
                initializeTask = disposeAwareTaskRunner.RunAsync(InitializeAsync);
            }
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

