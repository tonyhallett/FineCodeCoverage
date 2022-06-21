using FineCodeCoverage.Core.Utilities;
using System.ComponentModel.Composition;
using System.IO;
using System.Threading;

namespace FineCodeCoverage.Core.Initialization.ZippedTools
{
    [Export(typeof(IToolFolder))]
    internal class ToolFolder : IToolFolder
    {
        private readonly IZipFile zipFile;
        private readonly IFileUtil fileUtil;

        [ImportingConstructor]
        public ToolFolder(IZipFile zipFile, IFileUtil fileUtil)
        {
            this.zipFile = zipFile;
            this.fileUtil = fileUtil;
        }

        public string EnsureUnzipped(
            string appDataFolder, 
            string toolFolderName, 
            ZipDetails zipDetails, 
            CancellationToken cancellationToken
        )
        {
            cancellationToken.ThrowIfCancellationRequested();

            var toolFolderPath = Path.Combine(appDataFolder, toolFolderName);
            var zipDestination = Path.Combine(toolFolderPath, zipDetails.Version);

            if (!fileUtil.DirectoryExists(zipDestination))
            {
                DeleteOldVersions(toolFolderPath);
                Unzip(cancellationToken, zipDestination, zipDetails.Path);
            }

            return zipDestination;
        }

        private void DeleteOldVersions(string toolDirectory)
        {
            if (fileUtil.DirectoryExists(toolDirectory))
            {
                fileUtil.TryDeleteDirectory(toolDirectory);
            }
        }

        private void Unzip(CancellationToken cancellationToken,string zipDestination, string zipPath)
        {
            cancellationToken.ThrowIfCancellationRequested();
            
            fileUtil.CreateDirectory(zipDestination);
            zipFile.ExtractToDirectory(zipPath, zipDestination);
        }
    }
}
