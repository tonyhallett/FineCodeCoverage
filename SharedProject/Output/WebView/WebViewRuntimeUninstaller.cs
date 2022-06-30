using FineCodeCoverage.Core.Utilities;
using System;
using System.ComponentModel.Composition;
using System.Threading;
using System.Threading.Tasks;

namespace FineCodeCoverage.Output.WebView
{
    internal class UnsuccessfulExitCodeException : Exception { 
        public int ExitCode { get; private set; }
        public UnsuccessfulExitCodeException(int exitCode, string message) : base(message) { 
            ExitCode = exitCode;
        }
    }

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

        public async Task SilentUninstallAsync(bool throwIfNotInstalled = false,bool exitCode19Success = true, CancellationToken cancellationToken = default)
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
                if (!IsSuccessfulExitCode(response.ExitCode, exitCode19Success))
                {
                    throw new UnsuccessfulExitCodeException(response.ExitCode, $"Unsuccessful exit code : {response.ExitCode} - {response.Output}");
                }
            }

            await Task.CompletedTask;
        }

        /*
            Getting exit code 19 when run from .NET Console app -  UninstallWebView
            with no response.Output
            and it does uninstall....
            https://github.com/MicrosoftEdge/WebView2Feedback/issues/2317#issuecomment-1165536731
        */
        private bool IsSuccessfulExitCode(int exitCode, bool exitCode19Success)
        {
            return exitCode == 0 || exitCode19Success && exitCode == 19;
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
