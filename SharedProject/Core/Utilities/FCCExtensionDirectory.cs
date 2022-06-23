using System.IO;

namespace FineCodeCoverage.Core.Utilities
{
    internal static class FCCExtension
    {
        private static string directory;
        public static string Directory
        {
            get {
                if (directory == null)
                {
                    directory = Path.GetDirectoryName(typeof(FCCExtension).Assembly.Location);
                }
                return directory;
            }
        }
    }
}
