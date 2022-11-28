using FineCodeCoverage.Output.WebView;

namespace FineCodeCoverage.Output.HostObjects
{
    internal interface IWebViewHostObjectRegistration
    {
        string Name { get; }
        object HostObject { get; }

        void InitializationCompleted(IWebViewInterface webViewInterface);
    }
}
