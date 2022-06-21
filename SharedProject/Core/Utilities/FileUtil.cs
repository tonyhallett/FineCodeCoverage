using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace FineCodeCoverage.Core.Utilities
{
    [Export(typeof(IFileUtil))]
    internal class FileUtil : IFileUtil
    {
        [ExcludeFromCodeCoverage]
        public string CreateTempDirectory()
        {
            string tempDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(tempDirectory);
            return tempDirectory;
        }

        [ExcludeFromCodeCoverage]
        public bool TryDeleteDirectory(string directory)
        {
            var success = true;
            new DirectoryInfo(directory).TryDelete(true,(_) => success = false);
            return success;
        }

        [ExcludeFromCodeCoverage]
        public bool DirectoryExists(string directory)
        {
            return Directory.Exists(directory);
        }

        public string EnsureAbsolute(string directory, string possiblyRelativeTo)
        {
            if (!Path.IsPathRooted(directory))
            {
                directory =  Path.GetFullPath(Path.Combine(possiblyRelativeTo, directory));
            }
            return directory;
        }

        [ExcludeFromCodeCoverage]
        public string FileDirectoryPath(string filePath)
        {
            return new FileInfo(filePath).Directory.FullName;
        }

        [ExcludeFromCodeCoverage]
        public string ReadAllText(string path)
        {
            return File.ReadAllText(path);
        }

        public void TryEmptyDirectory(string directory)
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(directory);
            if (directoryInfo.Exists)
            {
                foreach (FileInfo file in directoryInfo.GetFiles())
                {
                    file.TryDelete();
                }
                foreach (DirectoryInfo subDir in directoryInfo.GetDirectories())
                {
                    subDir.TryDelete(true);
                }
            }
        }

        [ExcludeFromCodeCoverage]
        public void WriteAllText(string path, string contents)
        {
            File.WriteAllText(path, contents);
        }

        [ExcludeFromCodeCoverage]
        public bool Exists(string filePath)
        {
            return File.Exists(filePath);
        }

        [ExcludeFromCodeCoverage]
        public void Copy(string source, string destination)
        {
            File.Copy(source, destination);
        }

        [ExcludeFromCodeCoverage]
        public string DirectoryParentPath(string directoryPath)
        {
            var parentDirectory = new DirectoryInfo(directoryPath).Parent;
            if (parentDirectory == null)
            {
                return null;
            }
            return parentDirectory.FullName;
        }

        [ExcludeFromCodeCoverage]
        public DirectoryInfo CreateDirectory(string directoryPath)
        {
            return Directory.CreateDirectory(directoryPath);
        }

        [ExcludeFromCodeCoverage]
        public IEnumerable<string> DirectoryGetFiles(string folder, string searchPattern)
        {
            return Directory.GetFiles(folder, searchPattern);
        }

        public IFileSystemWatcher CreateFileSystemWatcher(string path, string filter)
        {
            return new FileSystemWatcherWrapper(path, filter);
        }
    }
}
