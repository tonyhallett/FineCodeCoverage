using System.ComponentModel.Composition;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace FineCodeCoverage.ReportGeneration
{
    [ExcludeFromCodeCoverage]
    [Export(typeof(IHtmlFilesToFolder))]
    internal class HtmlFilesToFolder : IHtmlFilesToFolder
    {
        public void Collate(string reportOutputFolder)
        {
            DeleteHtm(reportOutputFolder);
            Collate(reportOutputFolder, "HtmlReport");
        }

        private static void Collate(string reportOutputFolder, string directoryName)
        {
            var directory = new DirectoryInfo(reportOutputFolder);
            var htmlReportDirectory = directory.CreateSubdirectory(directoryName);

            foreach (var htmlFile in directory.EnumerateFiles("*.html"))
            {
                htmlFile.MoveTo(Path.Combine(htmlReportDirectory.FullName, htmlFile.Name));
            }
        }

        private static void DeleteHtm(string reportOutputFolder)
        {
            var path = Path.Combine(reportOutputFolder, "index.htm");
            File.Delete(path);
        }
    }

}
