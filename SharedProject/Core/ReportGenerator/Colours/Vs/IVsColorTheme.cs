using Microsoft.VisualStudio.Shell;
using System;
using System.Drawing;

namespace FineCodeCoverage.Core.ReportGenerator.Colours
{
    public class ColourThemeChangedArgs { 
        public Guid ThemeId { get; set; }
    }

    public interface IVsColourTheme
    {
        event EventHandler<ColourThemeChangedArgs> ThemeChanged;
        Color GetThemedColour(ThemeResourceKey themeResourceKey);
    }
}
