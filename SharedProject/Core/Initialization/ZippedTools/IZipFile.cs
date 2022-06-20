namespace FineCodeCoverage.Core.Initialization.ZippedTools
{
    internal interface IZipFile
    {
        void ExtractToDirectory(string sourceArchiveFileName, string destinationDirectoryName);
    }
}
