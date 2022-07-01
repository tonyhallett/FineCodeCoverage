namespace FineCodeCoverage.Output.WebView
{
    internal class VisualStudioTextBlockDynamicResourceNames : ITextBlockDynamicResourceNames
    {
        public object FontFamily => "VsFont.EnvironmentFontFamily";


        public object FontSize => "VsFont.EnvironmentFontSize";

        public object Background => "VsBrush.ToolWindowBackground";

        public object Foreground => "VsBrush.ToolWindowText";
    }
}
