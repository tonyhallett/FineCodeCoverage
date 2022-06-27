using FineCodeCoverage.Options;

namespace FineCodeCoverage.Impl
{
    internal interface IRunCoverageConditions
    {
        bool Met(ITestOperation testOperation, IAppOptions settings);
    }
}
