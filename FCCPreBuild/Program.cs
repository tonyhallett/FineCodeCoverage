namespace HelloWorld
{
    class Program
    {
        static void Main(string[] args)
        {
            GenerateReportPaths(args[0]);
        }

        // todo find dist folder so not dependent upon on the project folder name
        static string GetDebugPath(string solutionDirectory)
        {
            return Path.Combine(solutionDirectory, "FCCReport", "dist", "debug", "index.html");
        }

        static string GetReportPathsPath(string solutionDirectory)
        {
            var shprojFile = Directory.GetFiles(solutionDirectory, "*.shproj", SearchOption.AllDirectories)[0];
            var sharedProjectDirectory = new FileInfo(shprojFile).Directory;
            return sharedProjectDirectory!.GetFiles(
                "ReportPaths.cs", SearchOption.AllDirectories
            )[0].FullName;
        }

        static void GenerateReportPaths(string solutionDirectory)
        {
            Write(GetReportPathsPath(solutionDirectory),GetDebugPath(solutionDirectory));
        }

        static void Write(string reportPathsPath, string debugPath)
        {
            var reportPaths = $@"namespace {GetNamespace(reportPathsPath)}
{{
    internal class ReportPaths
    {{
        // this will be populated in a prebuild step
        public const string DebugPath = @""{debugPath}"";
    }}
}}";
            File.WriteAllText(reportPathsPath, reportPaths);
        }

        static string GetNamespace(string reportPathsPath)
        {
            return File.ReadAllLines(reportPathsPath)[0].Replace("namespace", "").Trim();
        }
    }
}