using FineCodeCoverage.Output;
using System.ComponentModel.Composition;
using System.Threading.Tasks;

namespace FineCodeCoverage.Core.Utilities
{
    [Export(typeof(IReportWindowOpener))]
    internal class ReportWindowOpener : IReportWindowOpener
    {
        public async Task OpenAsync() => _ = await OpenReportWindowCommand.Instance.ShowToolWindowAsync();
    }
}
