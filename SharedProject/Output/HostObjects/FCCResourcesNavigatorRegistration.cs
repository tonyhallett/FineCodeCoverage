using System.ComponentModel.Composition;

namespace FineCodeCoverage.Output.HostObjects
{
    [Export(typeof(IWebViewHostObjectRegistration))]
    public class FCCResourcesNavigatorRegistration : IWebViewHostObjectRegistration
    {
        [ImportingConstructor]
        public FCCResourcesNavigatorRegistration(IProcess process)
        {
            HostObject = new FCCResourcesNavigator(process);
        }
        public const string HostObjectName = "fccResourcesNavigator";
        public string Name => HostObjectName;

        public object HostObject { get; }
    }
}
