using System.Collections.Generic;

namespace FineCodeCoverage.Core.ReportGenerator.Colours
{
    internal interface ICategorizedNamedColoursProvider
    {
        List<CategorizedNamedColours> Provide();
    }
}
