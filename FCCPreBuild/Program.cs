namespace HelloWorld
{
    class Program
    {
        static void Main(string[] args)
        {
            GenerateDebugReportPath(args[0]);
        }

        // todo find dist folder so not dependent upon on the project folder name
        static string GetDebugPath(string solutionDirectory)
        {
            return Path.Combine(solutionDirectory, "FCCReport", "dist", "debug", "index.html");
        }

        static string GetDebugReportPathPath(string solutionDirectory)
        {
            var shprojFile = Directory.GetFiles(solutionDirectory, "*.shproj", SearchOption.AllDirectories)[0];
            var sharedProjectDirectory = new FileInfo(shprojFile).Directory;
            return sharedProjectDirectory!.GetFiles(
                "DebugReportPath.cs", SearchOption.AllDirectories
            )[0].FullName;
        }

        static void GenerateDebugReportPath(string solutionDirectory)
        {
            Write(GetDebugReportPathPath(solutionDirectory),GetDebugPath(solutionDirectory));
        }

        static void Write(string debugReportPathPath, string debugPath)
        {
            var reportPaths = $@"namespace {GetNamespace(debugReportPathPath)}
{{
    internal class DebugReportPath
    {{
        // this will be populated in a prebuild step
        public const string Path = @""{debugPath}"";
    }}
}}";
            File.WriteAllText(debugReportPathPath, reportPaths);
        }

        static string GetNamespace(string reportPathsPath)
        {
            return File.ReadAllLines(reportPathsPath)[0].Replace("namespace", "").Trim();
        }
    }
}