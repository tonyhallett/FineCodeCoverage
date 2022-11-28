using Microsoft.Internal.VisualStudio.Shell.Interop;
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
        private readonly IVsColorThemeService colorThemeService;

        public VsColorThemeWrapper()
        {
            colorThemeService = Package.GetGlobalService(typeof(SVsColorThemeService)) as IVsColorThemeService;
            VSColorTheme.ThemeChanged += VSColorTheme_ThemeChanged;
        }

        public event EventHandler<ColourThemeChangedArgs> ThemeChanged;

        private void VSColorTheme_ThemeChanged(ThemeChangedEventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            ThemeChanged?.Invoke(this, new ColourThemeChangedArgs { ThemeId = colorThemeService.CurrentTheme.ThemeId });
        }

        public Color GetThemedColour(ThemeResourceKey themeResourceKey)
        {
            return  VSColorTheme.GetThemedColor(themeResourceKey);
        }
    }
}
