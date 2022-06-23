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

        [ImportingConstructor]
        public WebViewRuntimeInstaller(
            IProcessUtil processUtil,
            IEnvironment environment
        )
        {
            this.processUtil = processUtil;
            this.environment = environment;
        }
        public async Task InstallAsync(CancellationToken cancellationToken)
        {
            var installerIdentifier = environment.Is64BitOperatingSystem ? "64" : "86";
            var installerFileName = $"MicrosoftEdgeWebView2RuntimeInstallerX{installerIdentifier}.exe";
            var installerPath = Path.Combine(FCCExtension.Directory, "WebViewRuntimeInstaller", installerFileName);
            throw new NotImplementedException();
            // a) Check how to specify the arguments
            /*
                	internal class ExecuteRequest
	                {
		                public string FilePath { get; set; }
		                public string Arguments { get; set; }
		                public string WorkingDirectory { get; set; }
	                }

            */
            /*
                	internal class ExecuteResponse
	                {
		                public int ExitCode { get; set; }
		                public DateTimeOffset ExitTime { get; set; }
		                public TimeSpan RunTime { get; set; }
		                public DateTimeOffset StartTime { get; set; }
		                public string Output { get; set; }
	                }
            */
            var response = await processUtil.ExecuteAsync(new ExecuteRequest { }, cancellationToken);
            // need to know the success exit code 
            var successCode = 0;
            if (response.ExitCode != successCode)
            {
                throw new Exception(response.Output);
            }
        }
    }
}
