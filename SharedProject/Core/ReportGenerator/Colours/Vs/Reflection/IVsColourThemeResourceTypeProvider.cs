using System.Collections.Generic;
using System.Reflection;

namespace FineCodeCoverage.Core.ReportGenerator.Colours
{
    internal interface IVsColourThemeResourceTypeProvider
    {
        List<IVsColourThemeResourceType> Provide(Assembly assembly);
    }
}
