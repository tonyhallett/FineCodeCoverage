using System.Threading;

namespace FineCodeCoverage.Core.Initialization.ZippedTools
{
    internal interface IToolFolder
    {
        string EnsureUnzipped(
            string appDataFolder,
            string toolFolderName, 
            ZipDetails zipDetails, 
            CancellationToken cancellationToken
        );
    }
}
