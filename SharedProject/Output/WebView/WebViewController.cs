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
		private readonly IEnumerable<IWebViewHostObjectRegistration> webViewHostObjectRegistrations;
        private readonly IPayloadSerializer payloadSerializer;
        private readonly ILogger logger;
        private readonly List<IPostJson> jsonPosters;

        private IWebView webView;
		private bool debugRefreshed;
		private readonly string standaloneReportPath = @"C:/Users/tonyh/source/repos/WebView2Demo/my-app/dist/build/index.html";
		private readonly string htmlPath;
		private const string debugDomain = "debug";
		//private const int remoteDebuggingPort = 9222;
		private readonly string debugDirectory;
		internal Task postJsonTask;
		//private FileSystemWatcher debugHtmlWatcher;
#if DEBUG
		internal bool debug = true;
#else
		internal bool debug = false;
#endif

		[ImportingConstructor]
		public WebViewController(
			[ImportMany]
			IEnumerable<IWebViewHostObjectRegistration> webViewHostObjectRegistrations,
			[ImportMany]
			IEnumerable<IPostJson> jsonPosters,
			IPayloadSerializer payloadSerializer,
			ILogger logger
		)
		{
			this.webViewHostObjectRegistrations = webViewHostObjectRegistrations;
            this.payloadSerializer = payloadSerializer;
            this.logger = logger;
            this.jsonPosters = jsonPosters.ToList();
            htmlPath = $"file:///{standaloneReportPath}";
			if (debug)
			{
				//if (false)
				//{
				//	var watchFile = "watch.txt";
				//	// todo refactor to interface
				//	debugHtmlWatcher = new FileSystemWatcher(debugDirectory, watchFile);
				//	// todo - just the filters necessary
				//	debugHtmlWatcher.IncludeSubdirectories = false;
				//	debugHtmlWatcher.NotifyFilter = NotifyFilters.Attributes
				//					 | NotifyFilters.CreationTime
				//					 | NotifyFilters.DirectoryName
				//					 | NotifyFilters.FileName
				//					 | NotifyFilters.LastAccess
				//					 | NotifyFilters.LastWrite
				//					 | NotifyFilters.Security
				//					 | NotifyFilters.Size;
				//	debugHtmlWatcher.EnableRaisingEvents = true;
				//	debugHtmlWatcher.Created += DebugHtmlWatcher_Created;
				//	// disposal
				//}

				debugDirectory = @"C:/Users/tonyh/source/repos/WebView2Demo/my-app/dist/debug/";
				htmlPath = $"https://{debugDomain}/index.html";
			}
		}

		private void DebugHtmlWatcher_Created(object sender, FileSystemEventArgs e)
		{
			_ = ExecuteOnMainThreadAsync(() =>
			{
				debugRefreshed = true;
				webView.Reload();
			});
		}

		public void Initialize(IWebView webView)
		{
			this.webView = webView;
			webView.SetVerticalAlignment(VerticalAlignment.Stretch);
			webView.SetHorizontalAlignment(HorizontalAlignment.Stretch);
			webView.SetVisibility(Visibility.Hidden);
			webView.DomContentLoaded += WebView_DomContentLoaded;
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

			if (debug)
			{
				webView.SetVirtualHostNameToFolderMapping(
					debugDomain, debugDirectory, CoreWebView2HostResourceAccessKind.Deny
				);
			}

			webView.Navigate(htmlPath);
		}

        public void ProcessFailed(object coreWebView2ProcessFailedEventArgs)
        {
			logger.Log("WebView2 Process failed :", coreWebView2ProcessFailedEventArgs);
        }
    }

}
