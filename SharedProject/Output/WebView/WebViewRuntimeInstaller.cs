using FineCodeCoverage.Core.Utilities;
using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace FineCodeCoverage.Output.WebView
{
    [Export(typeof(IWebViewRuntimeInstaller))]
    internal class WebViewRuntimeInstaller : IWebViewRuntimeInstaller
    {
        private readonly IProcessUtil processUtil;
        private readonly IEnvironment environment;
        private ProcessUtil processUtil1;

        public WebViewRuntimeInstaller(ProcessUtil processUtil1) => this.processUtil1 = processUtil1;

        [ImportingConstructor]
        public WebViewRuntimeInstaller(
            IProcessUtil processUtil,
            IEnvironment environment
        )
        {
            this.processUtil = processUtil;
            this.environment = environment;
        }

        public async Task InstallAsync(CancellationToken cancellationToken, bool silent)
        {
            var installerIdentifier = environment.Is64BitOperatingSystem ? "64" : "86";
            var installerFileName = $"MicrosoftEdgeWebView2RuntimeInstallerX{installerIdentifier}.exe";
            var installerPath = Path.Combine(FCCExtension.Directory, "WebViewRuntimeInstaller", installerFileName);

            var arguments = $"{(silent ? "/silent " : "")}/install";
            var response = await processUtil.ExecuteAsync(
                new ExecuteRequest { FilePath = installerPath, Arguments = arguments }, 
                cancellationToken
            );

            if (response.ExitCode != 0)
            {
                throw new Exception(response.Output);
            }
        }
    }
}
