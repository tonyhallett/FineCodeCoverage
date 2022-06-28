using FineCodeCoverage.Engine;

namespace FineCodeCoverageWebViewReport
{
    public class NoEnvironmentVariable : IEnvironmentVariable
    {
        public string Get(string variable) => null;
    }
}
