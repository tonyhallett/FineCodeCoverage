using FineCodeCoverage.Core;
using FineCodeCoverage.Impl;
using System.Threading.Tasks;

namespace FineCodeCoverage.Engine.MsTestPlatform.CodeCoverage
{
    interface IMsCodeCoverageRunSettingsService : ICoverageService
    {
        Task<MsCodeCoverageCollectionStatus> IsCollectingAsync(ITestOperation testOperation);
        Task CollectAsync(ITestOperation testOperation);
        Task TestExecutionNotFinishedAsync(ITestOperation testOperation);
    }    
}
