using System.ComponentModel.Composition;
using System.Threading;
using System.Threading.Tasks;
using FineCodeCoverage.Core.Utilities;

namespace FineCodeCoverage.Initialization
{
    [Export(typeof(IFirstTimeReportWindowOpener))]
    internal class FirstTimeReportWindowOpener : IFirstTimeReportWindowOpener
    {
        private readonly IInitializedFromTestContainerDiscoverer initializedFromTestContainerDiscoverer;
        private readonly IShownReportWindowHistory shownReportWindowHistory;
        private readonly IReportWindowOpener reportWindowOpener;

        [ImportingConstructor]
        public FirstTimeReportWindowOpener(
            IInitializedFromTestContainerDiscoverer initializedFromTestContainerDiscoverer,
            IShownReportWindowHistory shownReportWindowHistory,
            IReportWindowOpener reportWindowOpener
        )
        {
            this.initializedFromTestContainerDiscoverer = initializedFromTestContainerDiscoverer;
            this.shownReportWindowHistory = shownReportWindowHistory;
            this.reportWindowOpener = reportWindowOpener;
        }

        public async Task OpenIfFirstTimeAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (
                this.initializedFromTestContainerDiscoverer.InitializedFromTestContainerDiscoverer &&
                !this.shownReportWindowHistory.HasShown
            )
            {
                await this.reportWindowOpener.OpenAsync();
            }
        }
    }
}
