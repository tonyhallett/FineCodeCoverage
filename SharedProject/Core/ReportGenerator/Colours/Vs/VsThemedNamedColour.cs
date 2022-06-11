using Microsoft.VisualStudio.Shell;
using System.Drawing;

namespace FineCodeCoverage.Core.ReportGenerator.Colours
{
    public class VsThemedNamedColour : INamedColour
    {
        private readonly ThemeResourceKey themeResourceKey;
        public VsThemedNamedColour(IThemeResourceKeyProperty themeResourceKeyProperty)
        {
            themeResourceKey = themeResourceKeyProperty.ThemeResourceKey;
            var propertyName = themeResourceKeyProperty.PropertyName;
            JsName = propertyName.Replace("ColorKey", "");
        }
        public Color Colour { get; set; }
        public string JsName { get; set; }

        public bool UpdateColour(IVsColourTheme vsColourTheme)
        {
            var themedColour = vsColourTheme.GetThemedColour(themeResourceKey);
            var colorChanged = ColourChanged(Colour, themedColour);
            Colour = themedColour;
            return colorChanged;
        }

        private bool ColourChanged(Color oldColour, Color newColour)
        {
            return !oldColour.Equals(newColour);
        }
    }
}
