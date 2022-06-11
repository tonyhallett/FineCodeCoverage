using System.Collections.Generic;

namespace FineCodeCoverage.Core.ReportGenerator.Colours
{
    internal interface IVsCategorizedNamedColoursPopulator
    {
        List<CategorizedNamedColours> Populate(CategorizedNamedColoursSpecification categoryColoursSpecification);
    }
}
