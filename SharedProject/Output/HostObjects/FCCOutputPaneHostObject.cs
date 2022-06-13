using FineCodeCoverage.Core.Utilities;
using FineCodeCoverage.Impl;
using System.Runtime.InteropServices;

namespace FineCodeCoverage.Output.HostObjects
{
    [ClassInterface(ClassInterfaceType.AutoDual)]
    [ComVisible(true)]
    public class FCCOutputPaneHostObject
    {
        private readonly IEventAggregator eventAggregator;

        public FCCOutputPaneHostObject(IEventAggregator eventAggregator)
        {
            this.eventAggregator = eventAggregator;
        }

#pragma warning disable IDE1006 // Naming Styles
        public void show()
#pragma warning restore IDE1006 // Naming Styles
        {
            eventAggregator.SendMessage(new ShowFCCOutputPaneMessage());
        }
    }


}
