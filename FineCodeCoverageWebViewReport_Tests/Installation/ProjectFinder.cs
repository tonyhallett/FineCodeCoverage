namespace FineCodeCoverageWebViewReport_Tests.Tests
{
    using System.IO;
    using System.Reflection;

    internal static class ProjectFinder
    {
        public static string FromAscendantDirectory(DirectoryInfo startDirectory)
        {
            var dir = startDirectory.WalkAscendantsUntil(ascendant => ascendant.GetFiles("*.csproj").Length > 0);
            return dir.GetFiles("*.csproj")[0].FullName;
        }

        public static string FromAscendantDirectoryToExecutingAssembly()
        {
            var assemblyDirectory = new DirectoryInfo(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
            return FromAscendantDirectory(assemblyDirectory);
        }
    }
}

