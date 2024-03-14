using System.ComponentModel.Composition;
using FineCodeCoverage.Core.Utilities;
using FineCodeCoverage.Engine.Messages;

namespace FineCodeCoverage.Output
{
    [Export(typeof(ReportViewModel))]
    class ReportViewModel : IListener<NewReportMessage>
    {
        [ImportingConstructor]
        public ReportViewModel(
            IEventAggregator eventAggregator
        )
        {
            eventAggregator.AddListener(this);
        }

        public void Handle(NewReportMessage message)
        {
            // use the SummaryResult
            // message.SummaryResult.Assemblies
        }
    }
}
