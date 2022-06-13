using System;
using System.IO;
using FineCodeCoverage.Logging;

namespace FineCodeCoverage.Core.Utilities
{
	internal static class FileSystemInfoDeleteExtensions
	{
		private static void LogDeletionError(FileSystemInfo fileSystemInfo, Exception exc, string header, ILogger logger)
		{
			logger.Log($"{header} Error deleting {fileSystemInfo.FullName} : " + exc.Message);
		}

		public static void TryDeleteWithLogging(this FileInfo fileInfo,ILogger logger, string header = "")
		{
			fileInfo.TryDelete(exc => LogDeletionError(fileInfo, exc, header, logger));
		}

		public static void TryDeleteWithLogging(this DirectoryInfo directoryInfo, ILogger logger, string header = "", bool recursive = true)
		{
			directoryInfo.TryDelete(recursive, exc => LogDeletionError(directoryInfo, exc, header, logger));
		}

		public static void TryDelete(string path)
        {
            if (File.Exists(path))
            {
				new FileInfo(path).TryDelete();
            }
        }

		public static void TryDelete(this FileInfo fileInfo, Action<Exception> exceptionCallback = null)
		{
			try
			{
				fileInfo.Delete();
			}
			catch (Exception exc)
			{
                exceptionCallback?.Invoke(exc);
            }
		}

		public static void TryDelete(this DirectoryInfo directoryInfo, bool recursive = true, Action<Exception> exceptionCallback = null)
		{
			try
			{
				directoryInfo.Delete(recursive);
			}
			catch (Exception exc)
			{
                exceptionCallback?.Invoke(exc);
            }
		}
	}
}
