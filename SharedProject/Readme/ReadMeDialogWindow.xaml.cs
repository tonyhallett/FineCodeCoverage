using System;
using Markdig;
using Microsoft.VisualStudio.PlatformUI;

namespace FineCodeCoverage.Readme
{
    public partial class ReadMeDialogWindow : DialogWindow
    {
        private readonly Action<string> openHyperlink;
        private readonly Action<string> imageClicked;

        public ReadMeDialogWindow(
            string readMe, 
            Action<string> openHyperlink,
            Action<string> imageClicked
            )
        {
            this.InitializeComponent();
            this.Viewer.Pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();
            this.Viewer.Markdown = readMe;
            this.openHyperlink = openHyperlink;
            this.imageClicked = imageClicked;
        }

        private void OpenHyperlink(object sender, System.Windows.Input.ExecutedRoutedEventArgs e) 
            => this.openHyperlink(e.Parameter.ToString());

        private void ClickOnImage(object sender, System.Windows.Input.ExecutedRoutedEventArgs e) 
            => this.imageClicked(e.Parameter.ToString());
    }
}
