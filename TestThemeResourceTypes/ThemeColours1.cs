using Microsoft.VisualStudio.Shell;
using System;

namespace TestThemeResourceTypes
{
    public static class ThemeColours1
    {
        public static readonly Guid Category = new Guid("1CBAFEDB-E676-4500-A2AD-D32BBF94C460");
        public static ThemeResourceKey AForegroundColourKey { get; } = new ThemeResourceKey(Category, "FgColor", ThemeResourceKeyType.ForegroundColor);
        public static ThemeResourceKey ABackgroundColourKey { get; } = new ThemeResourceKey(Category, "BgColor", ThemeResourceKeyType.BackgroundColor);

        public static ThemeResourceKey IgnoredFgBrushKey { get; } = new ThemeResourceKey(Category, "FgBrush", ThemeResourceKeyType.ForegroundBrush);
        public static ThemeResourceKey IgnoredBgBrushKey { get; } = new ThemeResourceKey(Category, "FgBrush", ThemeResourceKeyType.BackgroundBrush);
    }

    public static class NotThemeColours
    {
        public static readonly string Category = "Not a guid";
    }

    public static class NotThemeColours2
    {
    }
}
