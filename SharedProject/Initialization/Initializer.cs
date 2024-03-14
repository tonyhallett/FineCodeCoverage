using System;
using System.ComponentModel.Composition;
using System.Threading;
using System.Threading.Tasks;
using FineCodeCoverage.Engine;
using FineCodeCoverage.Engine.Model;

namespace FineCodeCoverage.Initialization
{
    [Export(typeof(IInitializer))]
    [Export(typeof(IInitializeStatusProvider))]
    internal class Initializer : IInitializer
    {
        private readonly IFCCEngine fccEngine;
        private readonly ILogger logger;
        private readonly ICoverageProjectFactory coverageProjectFactory;
        private readonly IFirstTimeToolWindowOpener firstTimeToolWindowOpener;

        public InitializeStatus InitializeStatus { get; set; } = InitializeStatus.Initializing;
        public string InitializeExceptionMessage { get; set; }

        [ImportingConstructor]
        public Initializer(
            IFCCEngine fccEngine,
            ILogger logger,
            ICoverageProjectFactory coverageProjectFactory,
            IFirstTimeToolWindowOpener firstTimeToolWindowOpener,
            [ImportMany]
            IInitializable[] initializables
        )
        {
            this.fccEngine = fccEngine;
            this.logger = logger;
            this.coverageProjectFactory = coverageProjectFactory;
            this.firstTimeToolWindowOpener = firstTimeToolWindowOpener;
        }
        public async Task InitializeAsync(CancellationToken cancellationToken)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                this.logger.Log($"Initializing");

                cancellationToken.ThrowIfCancellationRequested();
                this.coverageProjectFactory.Initialize();

                this.fccEngine.Initialize(cancellationToken);

                cancellationToken.ThrowIfCancellationRequested();
                this.logger.Log($"Initialized");

                await this.firstTimeToolWindowOpener.OpenIfFirstTimeAsync(cancellationToken);
            }
            catch (Exception exception)
            {
                this.InitializeStatus = InitializeStatus.Error;
                this.InitializeExceptionMessage = exception.Message;
                if (!cancellationToken.IsCancellationRequested)
                {
                    this.logger.Log($"Failed Initialization", exception);
                }
            }

            if (this.InitializeStatus != InitializeStatus.Error)
            {
                this.InitializeStatus = InitializeStatus.Initialized;
            }
        }
    }
}

