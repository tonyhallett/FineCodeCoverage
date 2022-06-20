namespace FineCodeCoverage.Core.Initialization.ZippedTools
{
    internal interface IRequireToolUnzipping
    {
        string ZipDirectoryName { get; }
        string ZipPrefix { get; }
        void SetZipDestination(string zipDestination);
    }
}
