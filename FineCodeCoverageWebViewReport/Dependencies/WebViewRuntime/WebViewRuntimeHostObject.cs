using System.Runtime.InteropServices;

namespace FineCodeCoverageWebViewReport
{
    [ClassInterface(ClassInterfaceType.AutoDual)]
    [ComVisible(true)]
    public class WebViewRuntimeHostObject
    {
        private readonly WebViewRuntimeControlledInstalling webviewRuntime;

        public WebViewRuntimeHostObject(WebViewRuntimeControlledInstalling webviewRuntime)
        {
            this.webviewRuntime = webviewRuntime;
        }
        public void setInstalled()
        {
            this.webviewRuntime.SetInstalled();
        }
    }
}
