using FineCodeCoverage.Core.ReportGenerator.Colours;
using FineCodeCoverage.Core.Utilities;
using FineCodeCoverage.Options;
using FineCodeCoverage.Output;
using FineCodeCoverage.Output.EnvironmentFont;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;

namespace FineCodeCoverageWebViewReport
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            var outputToolWindowControl = new OutputToolWindowControl(
                WebViewControllerProvider.Provide()
            );

            this.Content = outputToolWindowControl;
        }
    }
}
