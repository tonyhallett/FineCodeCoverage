namespace FineCodeCoverageWebViewReport_Tests.Tests
{
    using System.Diagnostics;
    using System.Threading.Tasks;

    internal static class CmdLineWriter
    {
        public static string ErrorOutput { get; private set; }
        public static string Output { get; private set; }
        public static int ExitCode { get; private set; }
        public static async Task<Process> WriteAsync(string command)
        {
            var processStartInfo = new ProcessStartInfo
            {
                UseShellExecute = false,// necessary for redirecting
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                FileName = "cmd",
            };
            var cmdProcess = Process.Start(processStartInfo);

            await cmdProcess.StandardInput.WriteLineAsync($"{command} & exit");

            return cmdProcess;
        }

    }
}

