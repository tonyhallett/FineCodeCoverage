using System.IO;

namespace FineCodeCoverage.Core.Utilities
{
    internal interface IFileSystemWatcher
    {
        event FileSystemEventHandler Created;
        bool IncludeSubdirectories { get; set; }
        NotifyFilters NotifyFilter { get; set; }
        bool EnableRaisingEvents { get; set; }
    }
}
