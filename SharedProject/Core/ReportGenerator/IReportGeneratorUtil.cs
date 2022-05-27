using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace FineCodeCoverage.Core.ReportGenerator
{
    interface IReportGeneratorUtil
    {
        void Initialize(string appDataFolder, CancellationToken cancellationToken);
        Task<string> GenerateAsync(IEnumerable<string> coverOutputFiles, string reportOutputFolder, CancellationToken cancellationToken);
    }
}
