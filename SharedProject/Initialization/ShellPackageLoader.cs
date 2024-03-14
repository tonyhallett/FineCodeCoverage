using System;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Task = System.Threading.Tasks.Task;

namespace FineCodeCoverage.Initialization
{
    [Export(typeof(IShellPackageLoader))]
    internal class ShellPackageLoader : IShellPackageLoader
    {
        private readonly IServiceProvider serviceProvider;

        [ImportingConstructor]
        public ShellPackageLoader(
            [Import(typeof(SVsServiceProvider))]
             IServiceProvider serviceProvider
        ) => this.serviceProvider = serviceProvider;
        public async Task LoadPackageAsync()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            if (this.serviceProvider.GetService(typeof(SVsShell)) is IVsShell shell)
            {
                Guid packageToBeLoadedGuid = PackageGuids.guidOutputToolWindowPackage;
                _ = shell.LoadPackage(ref packageToBeLoadedGuid, out _);
            }
        }
    }
}
