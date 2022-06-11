using System;
using System.Collections.Generic;

namespace FineCodeCoverage.Core.ReportGenerator.Colours
{
    public interface IVsColourThemeResourceType
    {
        Guid Category { get; }
        List<IThemeResourceKeyProperty> ColourThemeResourceKeyProperties { get; }
        string Name { get; }
    }
}
