namespace FineCodeCoverage.Engine
{
    internal static class CoverageStatusMessageExtensions
    {
        internal static string Message(this CoverageStatus reloadCoverageStatus)
        {
            return $"================================== {reloadCoverageStatus.ToString().ToUpper()} ==================================";
        }
    }
}
