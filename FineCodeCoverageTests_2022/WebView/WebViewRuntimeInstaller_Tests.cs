namespace FineCodeCoverageTests.WebView_Tests
{
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMoq;
    using FineCodeCoverage.Core.Utilities;
    using FineCodeCoverage.Output.WebView;
    using Moq;
    using NUnit.Framework;

    internal class WebViewRuntimeInstaller_Tests
    {
        [TestCase(true, true)]
        [TestCase(false, false)]
        public async Task Should_Install_From_The_WebViewRuntimeInstaller_Directory_With_Exe_Os_Determined_Async(bool is64BitOs, bool silent)
        {
            var mocker = new AutoMoqer();
            var cancellationToken = CancellationToken.None;
            var installerIdentifier = is64BitOs ? "64" : "86";
            var installerFileName = $"MicrosoftEdgeWebView2RuntimeInstallerX{installerIdentifier}.exe";
            var installerPath = Path.Combine(FCCExtension.Directory, "WebViewRuntimeInstaller", installerFileName);
            var expectedArguments = $"{(silent ? "/silent " : "")}/install";
            var expectedExecuteRequest = new ExecuteRequest
            {
                Arguments = expectedArguments,
                FilePath = installerPath
            };

            var mockProcessUtil = mocker.GetMock<IProcessUtil>();
            _ = mockProcessUtil.Setup(processUtil => processUtil.ExecuteAsync(
                      Parameter.Is<ExecuteRequest>().That(Is.EqualTo(expectedExecuteRequest)
                  ), cancellationToken))
                .ReturnsAsync(new ExecuteResponse { ExitCode = 0 });

            _ = mocker.GetMock<IEnvironment>().SetupGet(environment => environment.Is64BitOperatingSystem)
                .Returns(is64BitOs);

            var webViewRuntimeInstaller = mocker.Create<WebViewRuntimeInstaller>();
            await webViewRuntimeInstaller.InstallAsync(cancellationToken, silent);

            mockProcessUtil.VerifyAll();
        }

        [Test]
        public void Should_Throw_With_Process_Output_If_Does_Not_Succeed()
        {
            var mocker = new AutoMoqer();
            var cancellationToken = CancellationToken.None;

            var mockProcessUtil = mocker.GetMock<IProcessUtil>();
            _ = mockProcessUtil.Setup(processUtil => processUtil.ExecuteAsync(It.IsAny<ExecuteRequest>(), cancellationToken))
                .ReturnsAsync(new ExecuteResponse { ExitCode = -1, Output = "Error output" });

            var webViewRuntimeInstaller = mocker.Create<WebViewRuntimeInstaller>();

            Assert.That(() => webViewRuntimeInstaller.InstallAsync(cancellationToken, true), Throws.Exception.Message.EqualTo("Error output"));
        }
    }
}
