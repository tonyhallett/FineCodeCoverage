using Microsoft.VisualStudio.Shell;
using System;
using System.Drawing;

namespace FineCodeCoverage.Core.ReportGenerator.Colours
{
    public interface IVsColourTheme
    {
        event EventHandler ThemeChanged;
        Color GetThemedColour(ThemeResourceKey themeResourceKey);
    }
}
