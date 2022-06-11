using System.Collections.Generic;
using System.Threading;

namespace FineCodeCoverage.Core.ReportGenerator
{
    interface IReportGeneratorUtil
    {
        string Generate(IEnumerable<string> coverOutputFiles, string reportOutputFolder, CancellationToken cancellationToken);
    }
}
