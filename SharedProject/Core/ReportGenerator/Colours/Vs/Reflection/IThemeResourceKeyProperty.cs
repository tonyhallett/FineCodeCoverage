using Microsoft.VisualStudio.Shell;

namespace FineCodeCoverage.Core.ReportGenerator.Colours
{
    public interface IThemeResourceKeyProperty
    {
        bool IsColour { get; set; }
        string PropertyName { get; set; }
        ThemeResourceKey ThemeResourceKey { get; set; }
    }
}
