using FineCodeCoverage.Output;
using System.Windows;

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

            this.Content = outputToolWindowControl;
        }
    }
}
