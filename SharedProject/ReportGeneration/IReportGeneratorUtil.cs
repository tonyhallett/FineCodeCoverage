using System.Collections.Generic;
using System.Threading;

namespace FineCodeCoverage.ReportGeneration
{
    internal interface IReportGeneratorUtil
    {
        ReportGeneratorResult Generate(
            IEnumerable<string> coverOutputFiles,
            string reportOutputFolder,
            CancellationToken cancellationToken
        );
    }
}
