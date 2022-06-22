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
}
