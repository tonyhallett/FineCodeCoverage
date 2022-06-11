using System.Collections.Generic;

namespace FineCodeCoverage.Core.ReportGenerator.Colours
{
    public class CategorizedNamedColoursSpecification
    {
        public List<string> FullCategoryTypes { get; set; }
        public List<CategorizedNamedColours> CategorizedNamedColoursList { get; set; }
    }

}
