using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell;
using System;
using System.ComponentModel.Composition;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;

namespace FineCodeCoverage.Core.ReportGenerator.Colours
{
    [ExcludeFromCodeCoverage]
    [Export(typeof(IVsColourTheme))]
    internal class VsColorThemeWrapper : IVsColourTheme
    {
        public VsColorThemeWrapper()
        {
            VSColorTheme.ThemeChanged += VSColorTheme_ThemeChanged;
        }

        public event EventHandler ThemeChanged;

        private void VSColorTheme_ThemeChanged(ThemeChangedEventArgs e)
        {
            ThemeChanged?.Invoke(this, EventArgs.Empty);
        }

        public Color GetThemedColour(ThemeResourceKey themeResourceKey)
        {
            return  VSColorTheme.GetThemedColor(themeResourceKey);
        }
    }
}
