namespace FineCodeCoverage.Output.WebView
{
    internal interface IWebViewRuntimeRegistryEntries
    {
        string Location { get; }
        string Name { get; }
        string SilentUninstallCommand { get; }
        string Version { get; }
    }
}
