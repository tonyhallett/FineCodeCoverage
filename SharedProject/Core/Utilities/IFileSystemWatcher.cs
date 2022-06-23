using System.IO;

namespace FineCodeCoverage.Core.Utilities
{
    internal interface IFileSystemWatcher
    {
        event FileSystemEventHandler Created;
        event FileSystemEventHandler Changed;
        bool IncludeSubdirectories { get; set; }
        bool EnableRaisingEvents { get; set; }
    }
}
