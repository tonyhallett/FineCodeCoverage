using FineCodeCoverage.Core.Utilities;
using FineCodeCoverage.Output.WebView;
using System.ComponentModel.Composition;

namespace FineCodeCoverage.Output.HostObjects
{
    [Export(typeof(IWebViewHostObjectRegistration))]
    public class FCCOutputPaneHostObjectRegistration : IWebViewHostObjectRegistration
    {
        [ImportingConstructor]
        public FCCOutputPaneHostObjectRegistration(IEventAggregator eventAggregator)
        {
            HostObject = new FCCOutputPaneHostObject(eventAggregator);
        }
        public const string HostObjectName = "fccOutputPane";
        public string Name => HostObjectName;

        public object HostObject { get; }
        public void InitializationCompleted(IWebViewInterface webViewInterface) { }
    }

}
