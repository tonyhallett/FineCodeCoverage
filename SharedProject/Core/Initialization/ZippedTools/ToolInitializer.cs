using FineCodeCoverage.Core.Utilities;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Threading;
using System.Threading.Tasks;

namespace FineCodeCoverage.Core.Initialization.ZippedTools
{
    [Order(2, typeof(IRequireInitialization))]
    internal class ToolInitializer : IRequireInitialization
    {
        private readonly IAppDataFolder appDataFolder;
        private readonly IEnumerable<IRequireToolUnzipping> requiresToolUnzipping;
        private readonly IToolFolder toolFolder;
        private readonly IToolZipProvider toolZipProvider;

        [ImportingConstructor]
        public ToolInitializer(
            IAppDataFolder appDataFolder,
            [ImportMany]
            IEnumerable<IRequireToolUnzipping> requiresToolUnzipping,
            IToolFolder toolFolder,
            IToolZipProvider toolZipProvider
        )
        {
            this.appDataFolder = appDataFolder;
            this.requiresToolUnzipping = requiresToolUnzipping;
            this.toolFolder = toolFolder;
            this.toolZipProvider = toolZipProvider;
        }

        public Task InitializeAsync(CancellationToken cancellationToken)
        {
            var appDataDirectoryPath = appDataFolder.GetDirectoryPath(); 
            foreach (var requireToolUnzipping in requiresToolUnzipping)
            {
                var zipDestination = toolFolder.EnsureUnzipped(
                    appDataDirectoryPath,
                    requireToolUnzipping.ZipDirectoryName,
                    toolZipProvider.ProvideZip(requireToolUnzipping.ZipPrefix),
                    cancellationToken);

                requireToolUnzipping.SetZipDestination(zipDestination);
                cancellationToken.ThrowIfCancellationRequested();
            }
            return Task.CompletedTask;
        }
    }
}
