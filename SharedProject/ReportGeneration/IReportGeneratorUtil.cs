using System.Collections.Generic;
using System.Threading;

namespace FineCodeCoverage.ReportGeneration
{
    interface IReportGeneratorUtil
    {
        ReportGeneratorResult Generate(
            IEnumerable<string> coverOutputFiles, 
            string reportOutputFolder, 
            CancellationToken cancellationToken
        );
    }
}
