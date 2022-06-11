using System.Collections.Generic;

namespace FineCodeCoverage.Core.ReportGenerator.Colours
{
    public class CategorizedNamedColours
    {
        public string Name { get; set; }
        public List<INamedColour> NamedColours { get; set; }
    }
}
