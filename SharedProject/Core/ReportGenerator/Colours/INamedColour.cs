using System.Drawing;

namespace FineCodeCoverage.Core.ReportGenerator.Colours
{
    public interface INamedColour
    {
        Color Colour { get; set; }
        string JsName { get; set; }
        bool UpdateColour(IVsColourTheme vsColourTheme);
    }
}
