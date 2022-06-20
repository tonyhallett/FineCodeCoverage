using FineCodeCoverage.Core;
using FineCodeCoverage.Impl;
using Microsoft.VisualStudio.TestWindow.Extensibility;
using System.Threading.Tasks;

namespace FineCodeCoverage.Engine.MsTestPlatform.CodeCoverage
{
    interface IMsCodeCoverageRunSettingsService : ICoverageService
    {
        Task<MsCodeCoverageCollectionStatus> IsCollectingAsync(ITestOperation testOperation);
        Task CollectAsync(IOperation operation, ITestOperation testOperation);
        Task TestExecutionNotFinishedAsync(ITestOperation testOperation);
    }    
}
