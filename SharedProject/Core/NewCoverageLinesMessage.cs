using FineCodeCoverage.Engine.Model;
using System.Collections.Generic;

namespace FineCodeCoverage.Engine
{
    internal class NewCoverageLinesMessage
    {
        public List<CoverageLine> CoverageLines { get; set; }
    }
}
