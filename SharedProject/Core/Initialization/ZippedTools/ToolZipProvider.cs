using FineCodeCoverage.Core.Utilities;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Reflection;

namespace FineCodeCoverage.Core.Initialization.ZippedTools
{
    [Export(typeof(IToolZipProvider))]
    internal class ToolZipProvider : IToolZipProvider
    {
        private const string ZippedToolsDirectoryName = "ZippedTools";
        private readonly string zipFolder;
        private readonly IFileUtil fileUtil;

        [ImportingConstructor]
        public ToolZipProvider(IFileUtil fileUtil)
        {
            var extensionDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            zipFolder = Path.Combine(extensionDirectory, ZippedToolsDirectoryName);
            this.fileUtil = fileUtil;
        }

        public ZipDetails ProvideZip(string zipPrefix)
        {
            var zipPath = GetZipPath(zipPrefix);
            var version = GetVersion(zipPath, zipPrefix);
            
            return new ZipDetails { Path = zipPath, Version = version };
        }

        private string GetZipPath(string zipPrefix)
        {
            var matchingZipFiles = fileUtil.DirectoryGetFiles(zipFolder, $"{zipPrefix}.*.zip");
            return matchingZipFiles.Single();
        }

        private string GetVersion(string zipPath, string zipPrefix)
        {
            var zipFileName = Path.GetFileName(zipPath);
            return zipFileName.Replace($"{zipPrefix}.", "").Replace(".zip", "");
        }
    }
}
