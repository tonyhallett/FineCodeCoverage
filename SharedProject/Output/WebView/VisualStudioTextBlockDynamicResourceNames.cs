using System;

namespace FineCodeCoverage.Output.WebView
{
    internal class VisualStudioTextBlockDynamicResourceNames : ITextBlockDynamicResourceNames, IEquatable<VisualStudioTextBlockDynamicResourceNames>
    {
        public object FontFamily => "VsFont.EnvironmentFontFamily";

        public object FontSize => "VsFont.EnvironmentFontSize";

        public object Background => "VsBrush.ToolWindowBackground";

        public object Foreground => "VsBrush.ToolWindowText";

        public bool Equals(VisualStudioTextBlockDynamicResourceNames other)
        {
            return FontFamily == other.FontFamily &&
                FontSize == other.FontSize &&
                Background == other.Background &&
                Foreground == other.Foreground;
        }
    }
}
