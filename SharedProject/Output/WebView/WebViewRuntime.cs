using FineCodeCoverage.Core.Initialization;
using FineCodeCoverage.Core.Utilities;
using System;
using System.ComponentModel.Composition;
using System.Threading;
using System.Threading.Tasks;

namespace FineCodeCoverage.Output.WebView
{
    [Export(typeof(IWebViewRuntime))]
    [Order(1, typeof(IRequireInitialization))]
    internal class WebViewRuntime : IWebViewRuntime, IRequireInitialization
    {
        private readonly IWebViewRuntimeInstallationChecker installationChecker;
        private readonly IWebViewRuntimeInstaller webViewRuntimeInstaller;

        [ImportingConstructor]
        public WebViewRuntime(
            IWebViewRuntimeInstallationChecker installationChecker,
            IWebViewRuntimeInstaller webViewRuntimeInstaller
        )
        {
            this.installationChecker = installationChecker;
            this.webViewRuntimeInstaller = webViewRuntimeInstaller;
        }


        public bool IsInstalled => installationChecker.IsInstalled();

        public event EventHandler Installed;

        public async Task InitializeAsync(bool testExplorerInstantiation, CancellationToken cancellationToken)
        {
            if (!IsInstalled)
            {
                await InstallWithoutUIAsync(cancellationToken);
            }
        }

        private async Task InstallWithoutUIAsync(CancellationToken cancellationToken)
        {
            await webViewRuntimeInstaller.InstallAsync(cancellationToken, true);
            Installed?.Invoke(this, new EventArgs());
        }
    }
}
