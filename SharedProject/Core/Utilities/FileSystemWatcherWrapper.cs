using System.IO;

namespace FineCodeCoverage.Core.Utilities
{
    class FileSystemWatcherWrapper : IFileSystemWatcher
    {
        private readonly FileSystemWatcher fileSystemWatcher;

        public bool IncludeSubdirectories
        {
            get => fileSystemWatcher.IncludeSubdirectories;
            set => fileSystemWatcher.IncludeSubdirectories = value;
        }
        public NotifyFilters NotifyFilter
        {
            get => fileSystemWatcher.NotifyFilter;
            set => fileSystemWatcher.NotifyFilter = value;
        }
        public bool EnableRaisingEvents
        {
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
