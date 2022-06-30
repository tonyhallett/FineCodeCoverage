namespace FineCodeCoverageWebViewReport_Tests.Tests
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Xml.Linq;
    using FineCodeCoverage.Core.Utilities;
    using FineCodeCoverage.Output.WebView;

    internal class WebViewRuntimeApplicableVersion
    {
        private readonly Func<Version, Version, bool> shouldReinstall;
        public WebViewRuntimeApplicableVersion(Func<Version, Version, bool> shouldReinstall = null) =>
            this.shouldReinstall = shouldReinstall ?? this.DefaultShouldReinstall;

        public async Task EnsureApplicableVersionInstalledAsync()
        {
            var processUtil = new ProcessUtil();
            var webViewRuntimeRegistry = new WebViewRuntimeRegistry();
            var entries = webViewRuntimeRegistry.GetEntries();
            if (entries == null)
            {
                await this.InstallAsync(processUtil);
            }
            else
            {
                if (this.RequiresReinstallation(entries))
                {
                    await this.ReinstallAsync(webViewRuntimeRegistry, processUtil);
                }
            }
        }

        /*
            due to the runtime getting updated

            session not created: This version of MSEdgeDriver only supports MSEdge version 100
            Current browser version is 103.0.1264.37 
        */
        private bool DefaultShouldReinstall(Version installedVersion, Version packageVersion) =>
            installedVersion.Major > packageVersion.Major;
        private bool RequiresReinstallation(IWebViewRuntimeRegistryEntries entries)
        {
            var version = entries.Version;
            var installedVersion = new Version(entries.Version);
            var packageVersion = this.GetPackageVersion();
            return this.shouldReinstall(installedVersion, packageVersion);
        }

        private Version GetPackageVersion()
        {
            var projectFile = ProjectFinder.FromAscendantDirectoryToExecutingAssembly();
            var project = XDocument.Load(projectFile);
            var versionString = project.Descendants("PackageReference")
                .First(packageReference => packageReference.Attribute("Include").Value == "Selenium.WebDriver.MSEdgeDriver")
                .Attribute("Version").Value;
            return new Version(versionString);
        }

        private async Task ReinstallAsync(WebViewRuntimeRegistry webViewRuntimeRegistry, ProcessUtil processUtil)
        {
            await this.UninstallAsync(webViewRuntimeRegistry, processUtil);
            await this.InstallAsync(processUtil);
        }

        private Task InstallAsync(ProcessUtil processUtil)
        {
            var installer = new WebViewRuntimeInstaller(processUtil, new EnvironmentWrapper());
            return installer.InstallAsync(CancellationToken.None, true);
        }
        private Task UninstallAsync(WebViewRuntimeRegistry webViewRuntimeRegistry, ProcessUtil processUtil)
        {
            var uninstaller = new WebViewRuntimeUninstaller(webViewRuntimeRegistry, processUtil);
            return uninstaller.SilentUninstallAsync(true, true);
        }
    }
}

