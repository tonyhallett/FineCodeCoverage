using FineCodeCoverage.Output.HostObjects;

namespace FineCodeCoverageWebViewReport
{
    internal class WebViewRuntimeHostObjectRegistration : IWebViewHostObjectRegistration
    {
        public WebViewRuntimeHostObjectRegistration(WebViewRuntimeControlledInstalling webViewRuntime)
        {
            HostObject = new WebViewRuntimeHostObject(webViewRuntime);
        }
        public const string JsName = "webViewRuntime";
        public string Name => JsName;

        public object HostObject { get; }
    }
}
