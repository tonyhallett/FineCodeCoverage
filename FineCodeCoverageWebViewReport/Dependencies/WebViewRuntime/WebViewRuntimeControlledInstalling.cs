using FineCodeCoverage.Output.WebView;
using System;
using System.Collections.Generic;
using System.IO;

namespace FineCodeCoverageWebViewReport
{
    public class WebViewRuntimeControlledInstalling : IWebViewRuntime
    {
        public const string InstalledWatcherFile = "webviewruntimeinstalledwatcherfile";
        public WebViewRuntimeControlledInstalling(Dictionary<string,string> namedArguments)
        {
            if(namedArguments.TryGetValue(InstalledWatcherFile, out var watcherFile))
            {
                var fileSystemWatcher = new FileSystemWatcher(Path.GetTempPath());
                fileSystemWatcher.Filter = watcherFile;
                fileSystemWatcher.Created += FileSystemWatcher_Created;
                fileSystemWatcher.EnableRaisingEvents = true;
            }
            else
            {
                IsInstalled = true;
            }
        }

        private void FileSystemWatcher_Created(object sender, FileSystemEventArgs e)
        {
            SetInstalled();
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
