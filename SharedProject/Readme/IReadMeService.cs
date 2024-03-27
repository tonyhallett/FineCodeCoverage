using Markdig;

namespace FineCodeCoverage.Readme
{
    public interface IReadMeService
    {
        void ShowReadMe(Microsoft.VisualStudio.Shell.AsyncPackage package);

        void ImageClicked(string url);
        
        void LinkClicked(string url);

        string MarkdownString { get; }

        MarkdownPipeline MarkdownPipeline { get; }
    }
}
