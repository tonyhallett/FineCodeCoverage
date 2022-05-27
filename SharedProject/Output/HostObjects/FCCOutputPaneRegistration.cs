using FineCodeCoverage.Core.Utilities;
using System.ComponentModel.Composition;

namespace FineCodeCoverage.Output.HostObjects
{
    [Export(typeof(IWebViewHostObjectRegistration))]
    public class FCCOutputPaneRegistration : IWebViewHostObjectRegistration
    {
        [ImportingConstructor]
        public FCCOutputPaneRegistration(IEventAggregator eventAggregator)
        {
            HostObject = new FCCOutputPaneHostObject(eventAggregator);
        }
        public const string HostObjectName = "fccOutputPane";
        public string Name => HostObjectName;

        public object HostObject { get; }
    }

}
