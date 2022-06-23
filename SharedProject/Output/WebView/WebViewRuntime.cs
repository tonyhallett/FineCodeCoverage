using FineCodeCoverage.Core.Initialization;
using System;
using System.ComponentModel.Composition;
using System.Threading;
using System.Threading.Tasks;

namespace FineCodeCoverage.Output.WebView
{
    [Export(typeof(IWebViewRuntime))]
    [Export(typeof(IRequireInitialization))]
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

        public bool IsInstalled { get; private set; }

        public event EventHandler Installed;

        public async Task InitializeAsync(CancellationToken cancellationToken)
        {
            if (installationChecker.IsInstalled())
            {
                RaiseIsInstalled();
            }
            else
            {
                await InstallAsync(cancellationToken);
            }
        }

        private async Task InstallAsync(CancellationToken cancellationToken)
        {
            await webViewRuntimeInstaller.InstallAsync(cancellationToken);
            RaiseIsInstalled();
        }

        private void RaiseIsInstalled()
        {
            IsInstalled = true;
            Installed?.Invoke(this, new EventArgs());
        }
    }
}
