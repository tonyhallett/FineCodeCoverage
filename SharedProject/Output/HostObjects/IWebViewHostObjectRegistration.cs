namespace FineCodeCoverage.Output.HostObjects
{
    internal interface IWebViewHostObjectRegistration
    {
        string Name { get; }
        object HostObject { get; }
    }
}
