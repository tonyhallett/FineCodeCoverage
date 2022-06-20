using System;
using System.ComponentModel.Composition;
using System.IO;
using FineCodeCoverage.Core.Utilities;
using FineCodeCoverage.Engine;
using FineCodeCoverage.Logging;
using FineCodeCoverage.Options;

namespace FineCodeCoverage.Core.Initialization
{
    [Export(typeof(IAppDataFolder))]
    internal class AppDataFolder : IAppDataFolder
    {
        private readonly ILogger logger;
        private readonly IEnvironmentVariable environmentVariable;
        private readonly IAppOptionsProvider appOptionsProvider;
        private readonly IFileUtil fileUtil;
        internal const string fccDebugCleanInstallEnvironmentVariable = "FCCDebugCleanInstall";
        private string directoryPath;

        [ImportingConstructor]
        public AppDataFolder(
            ILogger logger,
            IEnvironmentVariable environmentVariable, 
            IAppOptionsProvider appOptionsProvider,
            IFileUtil fileUtil
        )
        {
            this.logger = logger;
            this.environmentVariable = environmentVariable;
            this.appOptionsProvider = appOptionsProvider;
            this.fileUtil = fileUtil;
        }

        public string GetDirectoryPath() { 
            if (directoryPath == null)
            {
                directoryPath = CreateAppDataFolder();
            }

            return directoryPath;
        }

        private string CreateAppDataFolder()
        {
            var directoryPath = Path.Combine(GetAppDataFolderPath(), Vsix.Code);
            if (environmentVariable.Get(fccDebugCleanInstallEnvironmentVariable) != null)
            {
                CleanInstall(directoryPath);
            }
            fileUtil.CreateDirectory(directoryPath);
            return directoryPath;
        }

        private void CleanInstall(string fccAppDataPath)
        {
            logger.Log("FCC Clean Install");
            if (fileUtil.DirectoryExists(fccAppDataPath))
            {
                var success = fileUtil.TryDeleteDirectory(fccAppDataPath);
                if (success)
                {
                    logger.Log("Deleted FCC app data folder");
                }
                else
                {
                    logger.Log("Error deleting FCC app data folder");
                }
            }
            else
            {
                logger.Log("FCC App data folder does not exist");
            }
        }

        private string GetAppDataFolderPath()
        {
            var dir = appOptionsProvider.Provide().ToolsDirectory;

            return fileUtil.DirectoryExists(dir) ? dir : GetLocalAppDataPath();
        }

        private static string GetLocalAppDataPath()
        {
            return Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        }

    }

}