using System.Collections.Generic;
using System.IO;

namespace FineCodeCoverage.Core.Utilities
{
    internal interface IFileUtil
    {
        string ReadAllText(string path);
        void WriteAllText(string path, string contents);
        string CreateTempDirectory();
        bool DirectoryExists(string directory);
        void TryEmptyDirectory(string directory);
        string EnsureAbsolute(string directory, string possiblyRelativeTo);
        string FileDirectoryPath(string filePath);
        bool TryDeleteDirectory(string directory);
        bool Exists(string filePath);
        void Copy(string source, string destination);
        string DirectoryParentPath(string directoryPath);
        DirectoryInfo CreateDirectory(string directoryPath);
        IEnumerable<string> DirectoryGetFiles(string folder, string searchPattern);
        IFileSystemWatcher CreateFileSystemWatcher(string path, string filter);
    }

    internal interface IFileSystemWatcher
    {
        event FileSystemEventHandler Created;
        bool IncludeSubdirectories { get; set; }
        NotifyFilters NotifyFilter { get; set; }
        bool EnableRaisingEvents { get; set; }
    }

    //todo IDisposable
    class FileSystemWatcherWrapper : IFileSystemWatcher
    {
        private readonly FileSystemWatcher fileSystemWatcher;

        public bool IncludeSubdirectories { 
            get => fileSystemWatcher.IncludeSubdirectories; 
            set => fileSystemWatcher.IncludeSubdirectories = value; 
        }
        public NotifyFilters NotifyFilter { 
            get => fileSystemWatcher.NotifyFilter;
            set => fileSystemWatcher.NotifyFilter = value; 
        }
        public bool EnableRaisingEvents { 
            get => fileSystemWatcher.EnableRaisingEvents; 
            set => fileSystemWatcher.EnableRaisingEvents = value; 
        }

        public FileSystemWatcherWrapper(string path, string filter)
        {
            fileSystemWatcher = new FileSystemWatcher(path, filter);
        }

        event FileSystemEventHandler IFileSystemWatcher.Created
        {
            add
            {
                fileSystemWatcher.Created += value;
            }

            remove
            {
                fileSystemWatcher.Created -= value;
            }
        }
    }

}
