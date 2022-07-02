namespace FineCodeCoverageWebViewReport_Tests.Tests
{
    using System.IO;
    using System.Reflection;

    public static class WebViewReportExeFinder
    {
        public static string Find()
        {
            var assemblyLocation = Assembly.GetExecutingAssembly().Location;
            var fccSolutionDirectory = new DirectoryInfo(Path.GetDirectoryName(assemblyLocation));
            while (true)
            {
                fccSolutionDirectory = fccSolutionDirectory.Parent;
                if (fccSolutionDirectory.Name == "FineCodeCoverage")
                {
                    break;
                }
            }
            return Path.Combine(fccSolutionDirectory.FullName, "FineCodeCoverageWebViewReport", "bin", "Debug", "FineCodeCoverageWebViewReport.exe");
        }
    }
}
