using FineCodeCoverage.Output.WebView;
using System.ComponentModel.Composition;

namespace FineCodeCoverage.Output.HostObjects
{
    [Export(typeof(IWebViewHostObjectRegistration))]
    public class JsWebViewHostObjectRegistration : IWebViewHostObjectRegistration
    {
        public const string HostObjectName = "webView";
        public string Name => HostObjectName;
        private JsWebViewHostObject hostObject = new JsWebViewHostObject();

        public object HostObject => hostObject;

        public void InitializationCompleted(IWebViewInterface webViewInterface)
        {
            hostObject.WebViewInterface = webViewInterface;
        }
    }
}
