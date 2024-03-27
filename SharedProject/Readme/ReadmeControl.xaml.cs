using System.Windows.Controls;

namespace FineCodeCoverage.Readme
{
    public partial class ReadmeControl : UserControl
    {
        private readonly IReadMeService readMeService;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReadmeControl"/> class.
        /// </summary>
        public ReadmeControl(IReadMeService readMeService)
        {
            this.InitializeComponent();
            this.Viewer.Pipeline = readMeService.MarkdownPipeline;
            this.Viewer.Markdown = readMeService.MarkdownString;
            this.readMeService = readMeService;
        }
        private void OpenHyperlink(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
            => this.readMeService.LinkClicked(e.Parameter.ToString());

        private void ClickOnImage(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
            => this.readMeService.ImageClicked(e.Parameter.ToString());
    }
}