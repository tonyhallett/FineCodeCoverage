namespace FineCodeCoverageWebViewReport_Tests.EdgeHelpers
{
    using OpenQA.Selenium.Edge;

    public static class WebViewEdgeOptions
    {
        public static EdgeOptions CreateStart(string binaryLocation, string[] arguments = null)
        {
            var edgeOptions = new EdgeOptions
            {
                UseWebView = true,
                BinaryLocation = binaryLocation
            };

            if (arguments != null)
            {
                edgeOptions.AddArguments(arguments);
            }

            return edgeOptions;
        }

        public static EdgeOptions CreateAttach()
        {
            var edgeOptions = new EdgeOptions
            {
                UseWebView = true,
                DebuggerAddress = "localhost:9222"
            };

            return edgeOptions;
        }
    }
}
