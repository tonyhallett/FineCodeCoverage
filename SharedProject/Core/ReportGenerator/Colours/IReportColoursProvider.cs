using System;
using System.Collections.Generic;

namespace FineCodeCoverage.Core.ReportGenerator.Colours
{
    internal interface IReportColoursProvider
    {
        event EventHandler<List<CategorizedNamedColours>> CategorizedNamedColoursChanged;
        List<CategorizedNamedColours> GetCategorizedNamedColoursList();
    }
}
