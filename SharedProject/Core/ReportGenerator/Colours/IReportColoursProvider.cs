using System;
using System.Collections.Generic;

namespace FineCodeCoverage.Core.ReportGenerator.Colours
{
    public class CategorizedNamedColoursChangedArgs
    {
        public List<CategorizedNamedColours> CategorizedNamedColours { get; set; }
        public bool ThemeIsHighContrast { get; set; }
    }
    internal interface IReportColoursProvider
    {
        event EventHandler<CategorizedNamedColoursChangedArgs> CategorizedNamedColoursChanged;
        List<CategorizedNamedColours> GetCategorizedNamedColoursList();
    }
}
