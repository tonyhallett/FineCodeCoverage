using System;
using System.IO;
using Markdig.Renderers.Normalize;
using Markdig.Syntax.Inlines;
using Markdig;
using System.Reflection;
using Markdig.Syntax;
using System.Linq;
using System.ComponentModel.Composition;
using System.Collections.Generic;
using FineCodeCoverage.Output;

namespace FineCodeCoverage.Readme
{
    [Export(typeof(IReadMeService))]
    internal class ReadMeService : IReadMeService
    {
        private readonly IProcess process;

        [ImportingConstructor]
        public ReadMeService(
            IProcess process
        ) => this.process = process;

        public void ShowReadMe()
        {
            FileInfo readmeFile = GetReadMeFile();
            MarkdownDocument markdownDocument = GetMarkdownDocument(readmeFile);
            FixPaths(markdownDocument, readmeFile.Directory);

            string markdown = MardownDocumentToString(markdownDocument);
            _ = new ReadMeDialogWindow(markdown,this.LinkClicked, this.ImageClicked).ShowModal();

            // alternative is html
            //Markdown.ToHtml(markdownDocument);
        }

        private void LinkClicked(string url)
        {
            if (IsRelativePath(url))
            {
                url = "https://github.com/FortuneN/FineCodeCoverage/blob/master/" + url;
            }
            this.process.Start(url);
        }

        private static bool IsRelativePath(string url) => Uri.IsWellFormedUriString(url, UriKind.Relative);

        private void ImageClicked(string url)
        {
            if (url.StartsWith("https://img.youtube.com"))
            {
                string[] segments = url.Split('/');
                string videoId = segments[segments.Length - 2];
                url = $"https://youtu.be/{videoId}";
                this.process.Start(url);
            }
        }

        private static string MardownDocumentToString(MarkdownDocument markdownDocument)
        {
            var stringWriter = new StringWriter();
            var normalizeRenderer = new NormalizeRenderer(stringWriter);
            _ = normalizeRenderer.Render(markdownDocument);
            return stringWriter.ToString();
        }

        private static void FixPaths(MarkdownDocument markdownDocument, DirectoryInfo readMeDirectory)
        {
            IEnumerable<LinkInline> assets = markdownDocument.Descendants<LinkInline>().Where(linkInline => linkInline.IsImage && linkInline.Url != null && Uri.IsWellFormedUriString(linkInline.Url, UriKind.Relative));
            foreach (LinkInline asset in assets)
            {
                string combinedPath = Path.Combine(readMeDirectory.FullName, asset.Url);
                asset.Url = new Uri(combinedPath).AbsoluteUri;
            }
        }

        private static FileInfo GetReadMeFile()
        {
            DirectoryInfo dir = new FileInfo(Assembly.GetExecutingAssembly().Location).Directory;
            return dir.EnumerateFiles("README.md", SearchOption.AllDirectories).FirstOrDefault();
        }

        private static MarkdownDocument GetMarkdownDocument(FileInfo readmeFile)
        {
            string readMe = File.ReadAllText(readmeFile.FullName);
            return Markdown.Parse(readMe);
        }
    }
}
