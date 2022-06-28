using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Threading;
using FineCodeCoverage.Core.Utilities;
using FineCodeCoverage.Output;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Task = System.Threading.Tasks.Task;

namespace FineCodeCoverage.Core.Initialization
{
    [Order(0, typeof(IRequireInitialization))]
    internal class PackageInitializer : IRequireInitialization
    {
        private readonly IServiceProvider serviceProvider;
        private readonly IAppDataFolder appDataFolder;

        [ImportingConstructor]
        public PackageInitializer(
            [Import(typeof(SVsServiceProvider))]
            IServiceProvider serviceProvider,
            IAppDataFolder appDataFolder
        )
        {
            this.serviceProvider = serviceProvider;
            this.appDataFolder = appDataFolder;
        }

        public async Task InitializeAsync(bool testExplorerInstantiation, CancellationToken cancellationToken)
        {
            if (testExplorerInstantiation)
            {
                cancellationToken.ThrowIfCancellationRequested();
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

                if (serviceProvider.GetService(typeof(SVsShell)) is IVsShell shell)
                {
                    var packageToBeLoadedGuid = new Guid(OutputToolWindowPackage.PackageGuidString);
                    shell.LoadPackage(ref packageToBeLoadedGuid, out var _);

                    var outputWindowInitializedFile = Path.Combine(appDataFolder.GetDirectoryPath(), "outputWindowInitialized");

                    if (File.Exists(outputWindowInitializedFile))
                    {
                        // bizarelly vs 2022 will show whereas 2019 will not
                        await OutputToolWindowCommand.Instance.FindToolWindowAsync();
                    }
                    else
                    {
                        // for first time users, the window is automatically docked 
                        await OutputToolWindowCommand.Instance.ShowToolWindowAsync();
                        File.WriteAllText(outputWindowInitializedFile, string.Empty);
                    }
                }
            }

        }
    }

}

