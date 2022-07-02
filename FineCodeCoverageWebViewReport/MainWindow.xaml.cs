using FineCodeCoverage.Output;
using System.Windows;
using System.Windows.Automation;

namespace FineCodeCoverageWebViewReport
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow(string[] arguments)
        {
            InitializeComponent();
            var outputToolWindowControl = new OutputToolWindowControl(
                WebViewControllerProvider.Provide(arguments)
            );
            AutomationProperties.SetAutomationId(outputToolWindowControl, "FCCToolWindow");
            this.Content = outputToolWindowControl;
        }
    }
}
