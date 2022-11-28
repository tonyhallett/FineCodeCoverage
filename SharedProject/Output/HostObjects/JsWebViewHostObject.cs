using FineCodeCoverage.Output.WebView;
using System.ComponentModel.Composition;
using System.Runtime.InteropServices;

namespace FineCodeCoverage.Output.HostObjects
{
    [ClassInterface(ClassInterfaceType.AutoDual)]
    [ComVisible(true)]
    public class JsWebViewHostObject
    {
        public void setZoomFactor(double zoomLevel)
        {
            this.WebViewInterface.SetZoomFactor(zoomLevel);
        }

        public IWebViewInterface WebViewInterface { set; private get; }
    }
}
