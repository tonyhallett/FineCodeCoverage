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
        public MainWindow(string[] arguments)
        {
            var resources = new ResourceDictionary();
            resources.Add("VsFont.EnvironmentFontFamily", new FontFamily("Arial"));
            resources.Add("VsFont.EnvironmentFontSize", (double)30);
            resources.Add("VsBrush.ToolWindowBackground", new SolidColorBrush(Colors.Red));
            resources.Add("VsBrush.ToolWindowText", new SolidColorBrush(Colors.Yellow));
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
