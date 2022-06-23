using System;

namespace FineCodeCoverage.Output.WebView
{
    internal interface IWebViewRuntime
    {
        event EventHandler Installed; 
        bool IsInstalled { get; }
    }
}
