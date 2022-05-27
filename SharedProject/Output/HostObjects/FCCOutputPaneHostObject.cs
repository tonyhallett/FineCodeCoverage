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

        public void show()
        {
            eventAggregator.SendMessage(new ShowFCCOutputPaneMessage());
        }
    }


}
