using FineCodeCoverage.Engine.Model;

namespace FineCodeCoverage.Engine.Messages
{
    internal sealed class NewCoverageLinesMessage
    {
        public IFileLineCoverage CoverageLines { get; set; }
    }
}
