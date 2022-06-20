namespace FineCodeCoverage.Core.Initialization.ZippedTools
{
    internal interface IToolZipProvider
    {
        ZipDetails ProvideZip(string zipPrefix);
    }
}
