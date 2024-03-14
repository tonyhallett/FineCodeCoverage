using System.ComponentModel.Composition;
using System.Threading;
using System.Threading.Tasks;
using FineCodeCoverage.Core.Utilities;

namespace FineCodeCoverage.Initialization
{
    [Export(typeof(IFirstTimeToolWindowOpener))]
    internal class FirstTimeToolWindowOpener : IFirstTimeToolWindowOpener
    {
        private readonly IInitializedFromTestContainerDiscoverer initializedFromTestContainerDiscoverer;
        private readonly IShownToolWindowHistory shownToolWindowHistory;
        private readonly IToolWindowOpener toolWindowOpener;

        [ImportingConstructor]
        public FirstTimeToolWindowOpener(
            IInitializedFromTestContainerDiscoverer initializedFromTestContainerDiscoverer,
            IShownToolWindowHistory shownToolWindowHistory,
            IToolWindowOpener toolWindowOpener
        )
        {
            this.initializedFromTestContainerDiscoverer = initializedFromTestContainerDiscoverer;
            this.shownToolWindowHistory = shownToolWindowHistory;
            this.toolWindowOpener = toolWindowOpener;
        }

        public async Task OpenIfFirstTimeAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (
                this.initializedFromTestContainerDiscoverer.InitializedFromTestContainerDiscoverer &&
                !this.shownToolWindowHistory.HasShownToolWindow
            )
            {
                await this.toolWindowOpener.OpenToolWindowAsync();
            }
        }
    }
}
