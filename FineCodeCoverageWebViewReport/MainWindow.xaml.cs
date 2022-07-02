using FineCodeCoverage.Output;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Media;

namespace FineCodeCoverageWebViewReport
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private SolidColorBrush SolidColorBrushFromHex(string hex) => 
            new SolidColorBrush((Color)ColorConverter.ConvertFromString(hex));

        public MainWindow(string[] arguments)
        {
            var resources = new ResourceDictionary();
            resources.Add("VsFont.EnvironmentFontFamily", new FontFamily(VsResourceValues.VsFontEnvironmentFontFamily));
            resources.Add("VsFont.EnvironmentFontSize", VsResourceValues.VsFontEnvironmentFontSize);
            resources.Add(
                "VsBrush.ToolWindowBackground", 
                SolidColorBrushFromHex(VsResourceValues.VsBrushToolWindowBackgroundColorHex)
            );
            resources.Add(
                "VsBrush.ToolWindowText", 
                SolidColorBrushFromHex(VsResourceValues.VsBrushToolWindowTextColorHex)
            );

            this.Resources = resources;

            InitializeComponent();

            var outputToolWindowControl = new OutputToolWindowControl(
                WebViewControllerProvider.Provide(arguments)
            );
            AutomationProperties.SetAutomationId(outputToolWindowControl, "FCCToolWindow");

            this.Content = outputToolWindowControl;
        }
    }
}
