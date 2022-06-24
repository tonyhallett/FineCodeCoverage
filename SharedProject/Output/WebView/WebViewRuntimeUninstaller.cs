using FineCodeCoverage.Core.Utilities;
using System;
using System.ComponentModel.Composition;
using System.Threading;
using System.Threading.Tasks;

namespace FineCodeCoverage.Output.WebView
{
    internal class WebViewRuntimeUninstaller : IWebViewRuntimeUninstaller
    {
        private readonly IWebViewRuntimeRegistry webViewRuntimeRegistry;
        private readonly IProcessUtil processUtil;

        [ImportingConstructor]
        public WebViewRuntimeUninstaller(
            IWebViewRuntimeRegistry webViewRuntimeRegistry,
            IProcessUtil processUtil
        )
        {
            this.webViewRuntimeRegistry = webViewRuntimeRegistry;
            this.processUtil = processUtil;
        }

        public async Task SilentUninstallAsync(bool throwIfNotInstalled = false, CancellationToken cancellationToken = default)
        {
            var webViewRuntimeRegistryEntries = webViewRuntimeRegistry.GetEntries();
            if (webViewRuntimeRegistryEntries == null)
            {
                if (throwIfNotInstalled)
                {
                    throw new Exception("WebView2 Runtime is not installed");
                }
            }
            else
            {
                var (exe, arguments) = ExtractArguments(webViewRuntimeRegistryEntries.SilentUninstallCommand);
                var response = await processUtil.ExecuteAsync(new ExecuteRequest { FilePath = exe, Arguments = arguments }, cancellationToken);
                if (response.ExitCode != 0)
                {
                    throw new Exception(response.Output);
                }
            }
        }

        private (string exe, string arguments) ExtractArguments(string commandWithArguments)
        {
            var lastQuote = commandWithArguments.LastIndexOf("\"");
            if (lastQuote == -1)
            {
                var dotExeIndex = commandWithArguments.IndexOf(".exe");
                var exe = commandWithArguments.Substring(0, dotExeIndex + 4);
                var arguments = commandWithArguments.Substring(dotExeIndex + 4).Trim();
                return (exe, arguments);
            }
            else
            {
                var exe = commandWithArguments.Substring(0, lastQuote + 1);
                var arguments = commandWithArguments.Substring(lastQuote + 1).Trim();
                return (exe, arguments);
            }
        }
    }
}
