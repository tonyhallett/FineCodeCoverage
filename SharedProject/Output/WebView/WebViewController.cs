using FineCodeCoverage.Core.Initialization;
using FineCodeCoverage.Core.Utilities;
using FineCodeCoverage.Logging;
using FineCodeCoverage.Output.HostObjects;
using FineCodeCoverage.Output.JsPosting;
using FineCodeCoverage.Output.JsSerialization;
using Microsoft.VisualStudio.Shell;
using Microsoft.Web.WebView2.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Windows;
using Task = System.Threading.Tasks.Task;

namespace FineCodeCoverage.Output.WebView
{
    [Export(typeof(IWebViewController))]
	internal class WebViewController : IWebViewController, IJsonPoster
	{
		private class WebView2Settings : IWebViewSettings
        {
            public bool IsGeneralAutofillEnabled => false;

            public bool IsPasswordAutosaveEnabled => false;

            public bool IsStatusBarEnabled => true;

            public bool AreDefaultContextMenusEnabled => false;
			public bool AreDevToolsEnabled => true;
		}

		private readonly IEnumerable<IWebViewHostObjectRegistration> webViewHostObjectRegistrations;
        private readonly IPayloadSerializer payloadSerializer;
        private readonly ILogger logger;
        private readonly IAppDataFolder appDataFolder;
        private readonly IFileUtil fileUtil;
        private readonly List<IPostJson> jsonPosters;

        private IWebView webView;
		private bool navigated;
		private bool watcherRefreshed;
		private IFileSystemWatcher htmlWatcher;

		private const int remoteDebuggingPort = 9222;
		private const string fccDomain = "fcc";
		private readonly string htmlPath;
		private readonly string htmlDirectory;
		
		internal Task postJsonTask;

		public IWebViewSettings WebViewSettings => new WebView2Settings();

		public string UserDataFolder { get; private set; }

		public string AdditionalBrowserArguments => $"--remote-debugging-port={remoteDebuggingPort}";

        [ImportingConstructor]
		public WebViewController(
			[ImportMany]
			IEnumerable<IWebViewHostObjectRegistration> webViewHostObjectRegistrations,
			[ImportMany]
			IEnumerable<IPostJson> jsonPosters,
			IPayloadSerializer payloadSerializer,
			ILogger logger,
			IAppDataFolder appDataFolder,
			IFileUtil fileUtil,
			IReportPathsProvider reportPathsProvider
		)
		{
			this.webViewHostObjectRegistrations = webViewHostObjectRegistrations;
            this.payloadSerializer = payloadSerializer;
            this.logger = logger;
            this.appDataFolder = appDataFolder;
            this.fileUtil = fileUtil;
            this.jsonPosters = jsonPosters.ToList();

			var reportPaths = reportPathsProvider.Provide();
            if (reportPaths.ShouldWatch)
            {
				Watch(reportPaths.NavigationPath);
            }
			htmlDirectory = Path.GetDirectoryName(reportPaths.NavigationPath);
			htmlPath = $"https://{fccDomain}/{Path.GetFileName(reportPaths.NavigationPath)}";
		}

		private void HtmlWatcher_Created(object sender, FileSystemEventArgs e)
		{
            if (navigated)
            {
				_ = ExecuteOnMainThreadAsync(() =>
				{
					watcherRefreshed = true;
					webView.Reload();
				});
			}
		}
		
		private void Watch(string htmlPath)
        {
            var watchFile = "watch.txt";
            htmlWatcher = fileUtil.CreateFileSystemWatcher(
				Path.GetDirectoryName(htmlPath), 
				watchFile
			);
            // todo - just the filters necessary
            htmlWatcher.IncludeSubdirectories = false;
			htmlWatcher.NotifyFilter = NotifyFilters.Attributes
                                | NotifyFilters.CreationTime
                                | NotifyFilters.DirectoryName
                                | NotifyFilters.FileName
                                | NotifyFilters.LastAccess
                                | NotifyFilters.LastWrite
                                | NotifyFilters.Security
                                | NotifyFilters.Size;
			htmlWatcher.EnableRaisingEvents = true;
			htmlWatcher.Created += HtmlWatcher_Created;
		}
		
		public void Initialize(IWebView webView)
		{
			this.webView = webView;
			webView.SetVerticalAlignment(VerticalAlignment.Stretch);
			webView.SetHorizontalAlignment(HorizontalAlignment.Stretch);
			webView.SetVisibility(Visibility.Hidden);
			webView.DomContentLoaded += WebView_DomContentLoaded;

			EnsureUserDataDirectory();
		}

		private void EnsureUserDataDirectory()
        {
			UserDataFolder = Path.Combine(appDataFolder.GetDirectoryPath(), "webview2");
			fileUtil.CreateDirectory(UserDataFolder);
        }

		private void WebView_DomContentLoaded(object sender, EventArgs e)
		{
			if (watcherRefreshed)
			{
				RefreshJson();
				watcherRefreshed = false;
			}
			else
			{
				InitializeJson();
				webView.SetVisibility(Visibility.Visible);
			}
		}

		internal Func<Action, Task> ExecuteOnMainThreadAsync = async (action) =>
		{
			await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
			action();
		};
        
        private async Task PostJsonAsync<T>(string type, T data)
		{
            try
            {
				await ExecuteOnMainThreadAsync(() =>
				{
					var json = payloadSerializer.Serialize(type, data);
					PostWebMessageAsJson(json);
				});
			}
			catch (Exception exc)
            {
				logger.LogWithoutTitle($"Exception posting web message of type - {type}", exc);
			}
		}

		private void PostWebMessageAsJson(string json)
        {
			try
			{
				webView.PostWebMessageAsJson(json);
			}
			catch (ObjectDisposedException) { }
		}

		public void PostJson<T>(string type, T data)
        {
			postJsonTask = PostJsonAsync(type , data);
        } 

		private void InitializeJson()
        {
			jsonPosters.ForEach(jsonPoster => jsonPoster.Ready(this, webView));
        }

		private void RefreshJson()
		{
			jsonPosters.ForEach(jsonPoster => jsonPoster.Refresh());
		}

		public void CoreWebView2InitializationCompleted()
		{
			webViewHostObjectRegistrations.ToList().ForEach(webViewHostObjectRegistration =>
			{
				webView.AddHostObjectToScript(webViewHostObjectRegistration.Name, webViewHostObjectRegistration.HostObject);
			});

			webView.SetVirtualHostNameToFolderMapping(
				fccDomain, htmlDirectory, CoreWebView2HostResourceAccessKind.Deny
			);

			webView.Navigate(htmlPath);
			navigated = true;
		}

        public void ProcessFailed(object coreWebView2ProcessFailedEventArgs)
        {
			logger.Log("WebView2 Process failed :", coreWebView2ProcessFailedEventArgs);
        }
    }

}
