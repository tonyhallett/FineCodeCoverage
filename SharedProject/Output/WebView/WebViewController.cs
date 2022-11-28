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

		private interface IEarlyPost
		{
			string Type { get; }
			string Json { get; }
		}

		/*
			This is necessary when
			vs 2019 does not open FCC Tool Window when not first time ( 2022 FindToolWindow shows it ! )
			or 
			installing the runtime, Test Explorer is the active window and run any tests before activating the FCC Tool Window
			and thus no CoreWebView2InitializationCompleted
		*/
		private class EarlyPosts
		{
			private class PostJsonEarlyPosts
			{
				private readonly NotReadyPostBehaviour notReadyPostBehaviour;
				private readonly string type;
				private readonly List<OrderedEarlyPost> earlyPosts = new List<OrderedEarlyPost>();

				public PostJsonEarlyPosts(NotReadyPostBehaviour notReadyPostBehaviour, string type)
				{
					this.notReadyPostBehaviour = notReadyPostBehaviour;
					this.type = type;
				}

				public void EarlyPost(string json)
				{
					switch (notReadyPostBehaviour)
					{
						case NotReadyPostBehaviour.KeepAll:
							earlyPosts.Add(OrderedEarlyPost.Create(type, json));
							break;
						case NotReadyPostBehaviour.KeepLast:
							earlyPosts.Clear();
							earlyPosts.Add(OrderedEarlyPost.Create(type, json));
							break;

					}
				}
				public IReadOnlyCollection<OrderedEarlyPost> GetEarlyPosts()
				{
					return earlyPosts.AsReadOnly();
				}
			}

			private readonly Dictionary<string, PostJsonEarlyPosts> postJsonEarlyPostsLookup = new Dictionary<string, PostJsonEarlyPosts>();

			private class OrderedEarlyPost : IEarlyPost
			{
				public string Type { get; set; }
				public string Json { get; set; }
				public int Order { get; set; }
				public static int OrderCount = 0;
				public static OrderedEarlyPost Create(string type, string json)
				{
					return new OrderedEarlyPost { Type = type, Json = json, Order = OrderCount++ };
				}
			}

			public void AddJsonPoster(IPostJson postJson)
			{
				postJsonEarlyPostsLookup.Add(postJson.Type, new PostJsonEarlyPosts(postJson.NotReadyPostBehaviour, postJson.Type));
			}

			public void EarlyPost(string type, string json)
			{
				var postJsonEarlyPosts = postJsonEarlyPostsLookup[type];
				postJsonEarlyPosts.EarlyPost(json);
			}

			public IEnumerable<IEarlyPost> GetEarlyPosts()
			{
				var earlyPosts = postJsonEarlyPostsLookup.Values.SelectMany(postJsonEarlyPosts => postJsonEarlyPosts.GetEarlyPosts())
					.OrderBy(orderedEarlyPost => orderedEarlyPost.Order).ToList();

				postJsonEarlyPostsLookup.Clear();
				return earlyPosts;
			}
		}


		private readonly IEnumerable<IWebViewHostObjectRegistration> webViewHostObjectRegistrations;
        private readonly IPayloadSerializer payloadSerializer;
        private readonly ILogger logger;
        private readonly IAppDataFolder appDataFolder;
        private readonly IFileUtil fileUtil;
        private readonly IWebViewRuntime webViewRuntime;
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
		private readonly EarlyPosts earlyPosts = new EarlyPosts();

		public ITextBlockDynamicResourceNames TextBlockDynamicResourceNames { get; set; } = new VisualStudioTextBlockDynamicResourceNames();

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
			IReportPathsProvider reportPathsProvider,
			IWebViewRuntime webViewRuntime
		)
		{
			this.webViewHostObjectRegistrations = webViewHostObjectRegistrations;
            this.payloadSerializer = payloadSerializer;
            this.logger = logger;
            this.appDataFolder = appDataFolder;
            this.fileUtil = fileUtil;
            this.webViewRuntime = webViewRuntime;
            this.jsonPosters = jsonPosters.ToList();
			this.jsonPosters.ForEach(jsonPoster =>
			{
				earlyPosts.AddJsonPoster(jsonPoster);
				jsonPoster.Initialize(this);
			});

			var reportPaths = reportPathsProvider.Provide();
            if (reportPaths.ShouldWatch)
            {
				Watch(reportPaths.NavigationPath);
            }
			htmlDirectory = Path.GetDirectoryName(reportPaths.NavigationPath);
			htmlPath = $"https://{fccDomain}/{Path.GetFileName(reportPaths.NavigationPath)}";

		}

		private void HtmlWatcher_CreatedOrChanged(object sender, FileSystemEventArgs e)
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
            
            htmlWatcher.IncludeSubdirectories = false;
			htmlWatcher.EnableRaisingEvents = true;
			htmlWatcher.Created += HtmlWatcher_CreatedOrChanged;
			htmlWatcher.Changed += HtmlWatcher_CreatedOrChanged;
		}
		
		public void Initialize(IWebView webView)
		{
			EnsureUserDataDirectory();
			this.webView = webView;
			var initialText = webViewRuntime.IsInstalled ? "Loading." : "Installing Web View Runtime.";

			this.webView.AddTextBlock(initialText, TextBlockDynamicResourceNames);

			if (webViewRuntime.IsInstalled)
            {
				InstantiateAndSetUpWebView(true);
            }
            else
            {
				webViewRuntime.Installed += (sender, args) => InstantiateAndSetUpWebView(false);
			}
		}

		private void InstantiateAndSetUpWebView(bool isInstalled)
        {
			_ = ExecuteOnMainThreadAsync(() =>
			{
                if (!isInstalled)
                {
					webView.UpdateTextBlock("Loading. This takes some time.");
                }
				webView.Instantiate();
				
				webView.SetWebViewVerticalAlignment(VerticalAlignment.Stretch);
				webView.SetWebViewHorizontalAlignment(HorizontalAlignment.Stretch);
				webView.SetWebViewVisibility(Visibility.Hidden);

				webView.DomContentLoaded += WebView_DomContentLoaded;
				
			});
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
				webView.RemoveTextBlock();
				webView.SetWebViewVisibility(Visibility.Visible);
				PostEarlyJsons();
				ReadyJsonPosters();
			}
		}

		internal Func<Action, Task> ExecuteOnMainThreadAsync = async (action) =>
		{
			await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
			action();
		};
        #region json posting
        private void PostEarlyJsons()
        {
			postJsonTask = Task.WhenAll(
				earlyPosts.GetEarlyPosts().Select(earlyPost => PostJsonAsync(earlyPost.Type, earlyPost.Json))
			);
		}

        private async Task PostJsonAsync(string type,string json)
		{
            try
            {
				await ExecuteOnMainThreadAsync(() =>
				{
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
			var json = payloadSerializer.Serialize(type, data);
            if (navigated)
            {
				postJsonTask = PostJsonAsync(type, json);
            }
            else
            {
				earlyPosts.EarlyPost(type, json);
            }
        } 

		private void ReadyJsonPosters()
        {
			jsonPosters.ForEach(jsonPoster => jsonPoster.Ready(webView));
        }

		private void RefreshJson()
		{
			jsonPosters.ForEach(jsonPoster => jsonPoster.Refresh());
		}
		#endregion
		public void CoreWebView2InitializationCompleted()
		{
			webViewHostObjectRegistrations.ToList().ForEach(webViewHostObjectRegistration =>
			{
				webViewHostObjectRegistration.InitializationCompleted(webView);

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
