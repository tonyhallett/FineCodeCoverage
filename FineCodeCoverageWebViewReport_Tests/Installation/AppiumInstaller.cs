namespace FineCodeCoverageWebViewReport_Tests.Installation
{
    using System;
    using System.Diagnostics;
    using System.Threading.Tasks;

    internal static class AppiumInstaller
    {
        public static async Task EnsureGloballyInstalledWithNpmAsync()
        {
            var installed = await IsNpmGloballyInstalledAsync();
            if (!installed)
            {
                var installProcess = Process.Start("npm", "install -g appium");
                await WaitAndThrowIfNotSuccessfulAsync(installProcess, () =>
                    Task.FromResult($"Error installing appium globally with npm - exit code {installProcess.ExitCode}"));
            }
        }

        private static Task<bool> IsNpmGloballyInstalledAsync() =>
            IsNpmGloballyInstalledAsync(output => output.Contains("+-- appium@"));

        private static async Task<bool> IsNpmGloballyInstalledAsync(Func<string, bool> isInstalledFromOutput)
        {
            var installProcess = await CmdLineWriter.WriteAsync("npm list -g --depth 0 & exit");
            await WaitAndThrowWithStandardErrorIfNotSuccessfulAsync(installProcess, "Error listing npm global installs");
            var output = await installProcess.StandardOutput.ReadToEndAsync();
            return isInstalledFromOutput(output);
        }

        private static async Task WaitAndThrowIfNotSuccessfulAsync(Process process, Func<Task<string>> exceptionMessageProvider, int success = 0)
        {
            process.WaitForExit();
            var exitCode = process.ExitCode;
            if (exitCode != success)
            {
                var errorMessage = await exceptionMessageProvider();
                throw new Exception(errorMessage);
            }
        }
        private static Task WaitAndThrowWithStandardErrorIfNotSuccessfulAsync(
            Process process,
            string errorPrefix,
            int success = 0
        ) => WaitAndThrowIfNotSuccessfulAsync(
                process,
                async () =>
                {
                    var errorOutput = await process.StandardError.ReadToEndAsync();
                    return $"{errorPrefix} - exit code {process.ExitCode}. {errorOutput}";
                },
                success
            );
    }
}

