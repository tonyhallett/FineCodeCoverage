using FineCodeCoverage.Output.WebView;
using System;
using System.Collections.Generic;

namespace FineCodeCoverageWebViewReport
{
    public class WebViewRuntimeControlledInstalling : IWebViewRuntime
    {
        public const string JsSetInstalledArgumentName = "webViewRuntimInstalledFromJs";
        public WebViewRuntimeControlledInstalling(Dictionary<string,string> namedArguments)
        {
            if(!namedArguments.TryGetValue(JsSetInstalledArgumentName, out var value))
            {
                IsInstalled = true;
            }
        }
        public bool IsInstalled { get; set; }

        public event EventHandler Installed;

        public void SetInstalled()
        {
            IsInstalled = true;
            Installed.Invoke(this, EventArgs.Empty);
        }
    }
}
