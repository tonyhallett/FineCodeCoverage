using System.Diagnostics;

namespace HelloWorld
{
    class Program
    {
        static void Main(string[] args)
        {
            var solutionDirectory = args[0];
            BuildFCCReport(solutionDirectory);
            GenerateDebugReportPath(solutionDirectory);
        }

        private static void BuildFCCReport(string solutionDirectory)
        {
            var process = new Process();
            var startInfo = new ProcessStartInfo();
            startInfo.WorkingDirectory = GetFCCReportPath(solutionDirectory);
            startInfo.FileName = "cmd.exe";
            startInfo.Arguments = "/C npm run build";
            process.StartInfo = startInfo;
            process.Start();
            process.WaitForExit();
            
            if(process.ExitCode != 0)
            {
                throw new Exception();
            }
            process.Close();
        }

        private static string GetFCCReportPath(string solutionDirectory)
        {
            return Path.Combine(solutionDirectory, "FCCReport");
        }

        static string GetDebugPath(string solutionDirectory)
        {
            return Path.Combine(GetFCCReportPath(solutionDirectory), "dist", "debug", "index.html");
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