using System.ComponentModel.Composition;
using System.IO.Compression;

namespace FineCodeCoverage.Core.Initialization.ZippedTools
{
    [Export(typeof(IZipFile))]
    internal class ZipFileWrapper : IZipFile
    {
        public void ExtractToDirectory(string sourceArchiveFileName, string destinationDirectoryName)
        {
            ZipFile.ExtractToDirectory(sourceArchiveFileName, destinationDirectoryName);
        }
    }
}
