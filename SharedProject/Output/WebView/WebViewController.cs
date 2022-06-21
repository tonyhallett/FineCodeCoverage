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
	internal interface IWebViewSettings
    {
		bool IsGeneralAutofillEnabled { get; }
		bool IsPasswordAutosaveEnabled { get; }
		bool IsStatusBarEnabled { get; }
		bool AreDefaultContextMenusEnabled { get; }
		bool AreDevToolsEnabled { get; }
	}

	internal interface IStandaloneReportPathProvider
    {
		string Path { get; }
    }

	[Export(typeof(IWebViewController))]
	[Export(typeof(IStandaloneReportPathProvider))]
	internal class WebViewController : IWebViewController, IJsonPoster, IStandaloneReportPathProvider
	{
		private class WebView2Settings : IWebViewSettings
        {
            public bool IsGeneralAutofillEnabled => false;

            public bool IsPasswordAutosaveEnabled => false;

            public bool IsStatusBarEnabled => true;

            public bool AreDefaultContextMenusEnabled => false;
			public bool AreDevToolsEnabled => true;
		}
		private class HtmlPathInfo
        {
			public string StandalonePath { get; set; }
			public string NavigationPath { get; set; }
			public bool ShouldWatch { get; set; }
        }

		private readonly IEnumerable<IWebViewHostObjectRegistration> webViewHostObjectRegistrations;
        private readonly IPayloadSerializer payloadSerializer;
        private readonly ILogger logger;
        private readonly IAppDataFolder appDataFolder;
        private readonly IFileUtil fileUtil;
        private readonly List<IPostJson> jsonPosters;

        private IWebView webView;
		private bool debugRefreshed;
		//private FileSystemWatcher debugHtmlWatcher;

		// todo - this will be a project asset
		private readonly string htmlPath;
		private const string fccDomain = "fcc";
		private readonly string htmlDirectory;
		private const int remoteDebuggingPort = 9222;
		internal Task postJsonTask;
		
#if DEBUG
		internal bool debug = true;
#else
		internal bool debug = false;
#endif

		public IWebViewSettings WebViewSettings => new WebView2Settings();

		public string UserDataFolder { get; private set; }

		public string AdditionalBrowserArguments => $"--remote-debugging-port={remoteDebuggingPort}";

		// todo - this will be a project asset
		private readonly string standaloneReportpath;
		string IStandaloneReportPathProvider.Path => standaloneReportpath;

        [ImportingConstructor]
		public WebViewController(
			[ImportMany]
			IEnumerable<IWebViewHostObjectRegistration> webViewHostObjectRegistrations,
			[ImportMany]
			IEnumerable<IPostJson> jsonPosters,
			IPayloadSerializer payloadSerializer,
			ILogger logger,
			IAppDataFolder appDataFolder,
			IFileUtil fileUtil
		)
		{
			this.webViewHostObjectRegistrations = webViewHostObjectRegistrations;
            this.payloadSerializer = payloadSerializer;
            this.logger = logger;
            this.appDataFolder = appDataFolder;
            this.fileUtil = fileUtil;
            this.jsonPosters = jsonPosters.ToList();

			var htmlPathInfo = InitializeReport();
			htmlDirectory = Path.GetDirectoryName(htmlPathInfo.NavigationPath);
			htmlPath = $"https://{fccDomain}/{Path.GetFileName(htmlPathInfo.NavigationPath)}";
			standaloneReportpath = htmlPathInfo.StandalonePath;
		}

		private void HtmlWatcher_Created(object sender, FileSystemEventArgs e)
		{
			_ = ExecuteOnMainThreadAsync(() =>
			{
				debugRefreshed = true;
				webView.Reload();
			});
		}
		
		private HtmlPathInfo InitializeReport()
        {
			var htmlPathInfo = GetHtmlPathInfo();
			if (htmlPathInfo.ShouldWatch)
			{
                var watchFile = "watch.txt";
                var htmlWatcher = fileUtil.CreateFileSystemWatcher(
					Path.GetDirectoryName(htmlPathInfo.NavigationPath), 
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

			return htmlPathInfo;

		}
		private HtmlPathInfo GetHtmlPathInfo()
        {
			// todo checking AppOptions / Getting the path as an asset
			return new HtmlPathInfo
			{
				NavigationPath = debug ? ReportPaths.DebugPath : "",
				StandalonePath = @"C:\Users\tonyh\source\repos\FineCodeCoverage\FCCReport\dist\build\index.html",
				ShouldWatch = debug
			};
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
			if (debugRefreshed)
			{
				RefreshJson();
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
			debugRefreshed = false;
		}

		public void CoreWebView2InitializationCompleted()
		{
			webViewHostObjectRegistrations.ToList().ForEach(webViewHostObjectRegistration =>
			{
				webView.AddHostObjectToScript(webViewHostObjectRegistration.Name, webViewHostObjectRegistration.HostObject);
			});

			//if (debug)
			//{
				webView.SetVirtualHostNameToFolderMapping(
					fccDomain, htmlDirectory, CoreWebView2HostResourceAccessKind.Deny
				);
			//}

			webView.Navigate(htmlPath);
		}

        public void ProcessFailed(object coreWebView2ProcessFailedEventArgs)
        {
			logger.Log("WebView2 Process failed :", coreWebView2ProcessFailedEventArgs);
        }
    }

}
