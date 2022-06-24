namespace FineCodeCoverageTests.WebView_Tests
{
    using System;
    using System.Threading.Tasks;
    using FineCodeCoverage.Core.Utilities;
    using FineCodeCoverage.Output.WebView;
    using NUnit.Framework;


    internal class XXX
    {
        [Test]
        public async Task Uninstall_Async()
        {
            try
            {
                await new WebViewRuntimeUninstaller(
                    new WebViewRuntimeRegistry(),
                    new ProcessUtil()
                ).SilentUninstallAsync();
            }catch (Exception exc)
            {
                var st = "";
            }
        }
    }
}
