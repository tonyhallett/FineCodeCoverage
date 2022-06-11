using System.Windows;
using System.Windows.Controls;
using Microsoft.VisualStudio.Shell;
using FineCodeCoverage.Core.Utilities;
using System;
using Microsoft.Web.WebView2.Wpf;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Web.WebView2.Core;
using Newtonsoft.Json;
using Task = System.Threading.Tasks.Task;
using FineCodeCoverage.Output.HostObjects;
using FineCodeCoverage.Output.JsSerialization;
using FineCodeCoverage.Options;
using System.IO;
using FineCodeCoverage.Output.JsMessages.Logging;
using FineCodeCoverage.Output.JsMessages;
using FineCodeCoverage.Output.JsSerialization.ReportGenerator;
using FineCodeCoverage.Core.ReportGenerator.Colours;

namespace FineCodeCoverage.Output
{
	/// <summary>
	/// Interaction logic for OutputToolWindowControl.
	/// </summary>
	internal partial class OutputToolWindowControl : 
		UserControl, IListener<NewReportMessage>, IListener<LogMessage>, IListener<CoverageStoppedMessage>, IListener<ClearReportMessage>
	{
		private WebView2 _webView2;
        private readonly IReportColoursProvider reportColoursProvider;
        private readonly List<IWebViewHostObjectRegistration> webViewHostObjectRegistrations;
        private readonly IAppOptionsProvider appOptionsProvider;
        private Styling styling;
		private Report report;
		private string htmlPath;
		private string standaloneReportPath;
		private const string debugDomain = "debug";
		private const int remoteDebuggingPort = 9222;
		private string debugDirectory;
		private FileSystemWatcher debugHtmlWatcher;
		private bool debugRefreshed;
		private ReportOptions lastReportOptions;
		private bool domContentLoaded;
		private readonly List<LogMessage> earlyLogMessages = new List<LogMessage>();

#if DEBUG
		private readonly bool debug = true;
#else
		private readonly bool debug = false;
#endif

		/// <summary>
		/// Initializes a new instance of the <see cref="OutputToolWindowControl"/> class.
		/// </summary>
		public OutputToolWindowControl(
			IEventAggregator eventAggregator, 
			IReportColoursProvider reportColoursProvider, 
			List<IWebViewHostObjectRegistration> webViewHostObjectRegistrations,
			IAppOptionsProvider appOptionsProvider)
		{
			eventAggregator.AddListener(this);
			this.reportColoursProvider = reportColoursProvider;
            this.webViewHostObjectRegistrations = webViewHostObjectRegistrations;
            this.appOptionsProvider = appOptionsProvider;
            InitializeComponent();
            _ = InitializeAsync();
            appOptionsProvider.OptionsChanged += AppOptionsProvider_OptionsChanged;

		}

        private void AppOptionsProvider_OptionsChanged(IAppOptions appOptions)
        {
			var reportOptions = ReportOptions.Create(appOptions);
            if (lastReportOptions == null || !(lastReportOptions.namespacedClasses == reportOptions.namespacedClasses && lastReportOptions.hideFullyCovered == reportOptions.hideFullyCovered))
            {
				lastReportOptions = reportOptions;
				_ = PostReportOptionsAsync();
            }
        }

        private async Task InitializeAsync()
		{
			await InitializeWebViewAsync();
			reportColoursProvider.CategorizedNamedColoursChanged += ReportColoursProvider_ColoursChanged;
		}

		//todo - html path
		private async Task InitializeWebViewAsync()
        {
			//todo
			standaloneReportPath = @"C:/Users/tonyh/source/repos/WebView2Demo/my-app/dist/build/index.html";
			htmlPath = $"file:///{standaloneReportPath}";
			if (debug)
            {
				debugDirectory = @"C:/Users/tonyh/source/repos/WebView2Demo/my-app/dist/debug/";	
				var watchFile = "watch.txt";
				// todo refactor to interface
				debugHtmlWatcher = new FileSystemWatcher(debugDirectory, watchFile);
				// todo - just the filters necessary
				debugHtmlWatcher.IncludeSubdirectories = false;
				debugHtmlWatcher.NotifyFilter = NotifyFilters.Attributes
								 | NotifyFilters.CreationTime
								 | NotifyFilters.DirectoryName
								 | NotifyFilters.FileName
								 | NotifyFilters.LastAccess
								 | NotifyFilters.LastWrite
								 | NotifyFilters.Security
								 | NotifyFilters.Size;
				debugHtmlWatcher.EnableRaisingEvents = true;
                debugHtmlWatcher.Created += DebugHtmlWatcher_Created;
				// disposal
				htmlPath = $"https://{debugDomain}/index.html";
			}

			_webView2 = new WebView2();
			_webView2.VerticalAlignment = VerticalAlignment.Stretch;
			_webView2.HorizontalAlignment = HorizontalAlignment.Stretch;
			_webView2.Visibility = Visibility.Hidden;
			this.AddChild(_webView2); // needs to be in the tree before 

			_webView2.CoreWebView2InitializationCompleted += Webview2_CoreWebView2InitializationCompleted;
			await InitializeWebViewEnvironmentAsync();
		}


        private void DebugHtmlWatcher_Created(object sender, FileSystemEventArgs e)
        {
			ThreadHelper.JoinableTaskFactory.Run(async () =>
			{
				await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
				debugRefreshed = true;
				_webView2.CoreWebView2.Reload();
			});
		}

        //todo - user data folder
        private async Task InitializeWebViewEnvironmentAsync()
		{
			// todo
			var userDataFolder = @"C:\Users\tonyh\AppData\Local\FineCodeCoverage\webview2";

			//todo conditional
			//should --user-data-dir be above ?
			//https://stackoverflow.com/questions/69378977/how-do-i-enable-editable-debugging-of-files-within-vs-code-w-chrome-debugging
			//https://chromium.googlesource.com/chromium/src/+/HEAD/docs/user_data_dir.md

			// https://code.visualstudio.com/docs/nodejs/browser-debugging#_attaching-to-browsers
			// or from an open edge - edge://inspect

			string additionalBrowserArguments = $"--remote-debugging-port={remoteDebuggingPort} --user-data-dir=remote-debug-profile";
			CoreWebView2EnvironmentOptions options = new CoreWebView2EnvironmentOptions(additionalBrowserArguments);
			var environment = await CoreWebView2Environment.CreateAsync(userDataFolder: userDataFolder, options: options);
			await _webView2.EnsureCoreWebView2Async(environment);
		}
		
		//todo - context menu - turn off
		private void InitializeWebViewSettings()
        {
			var settings = _webView2.CoreWebView2.Settings;

			//settings.AreDefaultContextMenusEnabled = false;// do not get the ContextMenuRequested event

			// Removes the menuitem
			//settings.AreDevToolsEnabled = false;// need to set this as can do with keyboard even if remove the context menu item or the context menu
			//coreWebView2.OpenDevToolsWindow

			settings.IsGeneralAutofillEnabled = false;
			settings.IsPasswordAutosaveEnabled = false;
			//settings.AreBrowserAcceleratorKeysEnabled ???
			//settings.AreDefaultScriptDialogsEnabled
			settings.HiddenPdfToolbarItems = Microsoft.Web.WebView2.Core.CoreWebView2PdfToolbarItems.None;
			//settings.IsBuiltInErrorPageEnabled

			//settings.IsPinchZoomEnabled
			//settings.IsSwipeNavigationEnabled - should not be necessary as no forward or backward

			//settings.IsStatusBarEnabled = false;//?
			//coreWebView2.StatusBarText = "FCC Yeah !"; readonly currently

			// settings.IsZoomControlEnabled - Inspect the zoom ?

			//coreWebView2.BasicAuthenticationRequested - perhaps necessary for share gmail
			//coreWebView2.ClientCertificateRequested

			//coreWebView2.ContextMenuRequested += CoreWebView2_ContextMenuRequested;

			//coreWebView2.WindowCloseRequested
		}
		
		// note that can only access the CoreWebView2 here
		private void Webview2_CoreWebView2InitializationCompleted(object sender, CoreWebView2InitializationCompletedEventArgs e)
		{
			if (e.IsSuccess)
			{
				InitializeWebViewSettings();
				var coreWebView2 = _webView2.CoreWebView2;
				webViewHostObjectRegistrations.ForEach(webViewHostObjectRegistration =>
				{
					coreWebView2.AddHostObjectToScript(webViewHostObjectRegistration.Name, webViewHostObjectRegistration.HostObject);
				});
                coreWebView2.ProcessFailed += CoreWebView2_ProcessFailed;
				coreWebView2.ContextMenuRequested += CoreWebView2_ContextMenuRequested;
                if (debug)
                {
					coreWebView2.SetVirtualHostNameToFolderMapping(debugDomain, debugDirectory, CoreWebView2HostResourceAccessKind.Deny);
				}
				LoadHtml();
			}
			else
			{
				//todo
				var exc = e.InitializationException;
			}
		}

		private void LoadHtml()
		{
			_webView2.CoreWebView2.DOMContentLoaded += CoreWebView2_DOMContentLoaded;
			_webView2.CoreWebView2.Navigate(htmlPath);
		}

        private void CoreWebView2_DOMContentLoaded(object sender, CoreWebView2DOMContentLoadedEventArgs e)
        {
			domContentLoaded = true;
			if (debugRefreshed)
            {
				_ = RefreshJsonAsync();
            }
            else
            {
				InitializeStyling();
				InitializeReportOptions();
				_webView2.Visibility = Visibility.Visible;
				_ = PostEarlyLogMessagesAsync();
			}
		}

		private async Task RefreshJsonAsync()
        {
			await PostStylingAsync();
			await PostReportOptionsAsync();
			await PostReportAsync();
        }

		private void InitializeReportOptions()
        {
			lastReportOptions = ReportOptions.Create(appOptionsProvider.Get());
			var _ = PostReportOptionsAsync();
		}

        private void InitializeStyling()
		{
			styling = new Styling
			{
				categoryColours = reportColoursProvider.GetCategorizedNamedColoursList().SerializeAsDictionary()
			};
			var environmentFont = new EnvironmentFont();
			environmentFont.Changed += (sender, fontDetails) =>
			{
				styling.fontName = fontDetails.Family.Source;
				styling.fontSize = $"{fontDetails.Size}px";
				_ = PostStylingAsync();
			};
			environmentFont.Initialize(this);
		}

		private async Task PostJsonAsync<T>(string type, T data, JsonSerializerSettings settings = null)
		{
            try
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                var json = Payload<T>.AsJson(type, data, settings);
                _webView2.CoreWebView2.PostWebMessageAsJson(json);
            }
            catch (ObjectDisposedException) { }
        }

		private Task PostStylingAsync()
		{
			return PostJsonAsync("styling", styling);
		}
		
		private Task PostReportOptionsAsync()
        {
			return PostJsonAsync("reportOptions", lastReportOptions);
        }

		private async Task PostReportAsync()
        {
			await PostJsonAsync("report", report);
		}

		private Task PostLogMessageAsync(LogMessage message)
        {
			return PostJsonAsync("message", message);
		}

		private Task PostEarlyLogMessagesAsync()
        {
			var whenAll = Task.WhenAll(earlyLogMessages.Select(logMessage => PostLogMessageAsync(logMessage)));
			earlyLogMessages.Clear();
			return whenAll;
        }

        public void Handle(NewReportMessage message)
        {
			report = new Report(message.RiskHotspotAnalysisResult, message.RiskHotspotsAnalysisThresholds, message.SummaryResult);
			
			GenerateStandaloneReport(message.ReportFilePath,report);
			_ = PostReportAsync();
		}

		private void GenerateStandaloneReport(string reportPath, Report report)
        {
			var reportJson = JsonConvert.SerializeObject(report);
			HtmlAgilityPack.HtmlDocument document = new HtmlAgilityPack.HtmlDocument();
			document.Load(standaloneReportPath);
			var script = document.CreateElement("script");
			script.InnerHtml = $"var report={reportJson}";
			document.DocumentNode.AppendChild(script); // todo ok ?
			document.Save(reportPath);
			
		}

		private void ReportColoursProvider_ColoursChanged(object sender, List<CategorizedNamedColours> reportColours)
		{
			if (styling != null)
            {
				styling.categoryColours = reportColours.SerializeAsDictionary();
				_ = PostStylingAsync();
			}
		}

		//todo
		private void CoreWebView2_ProcessFailed(object sender, CoreWebView2ProcessFailedEventArgs e)
		{
			throw new NotImplementedException();
		}

		//todo
		private void CoreWebView2_ContextMenuRequested(object sender, Microsoft.Web.WebView2.Core.CoreWebView2ContextMenuRequestedEventArgs e)
		{
			var cmTarget = e.ContextMenuTarget;
			//
			//e.GetDeferral()//Complete() why would you want a deferral ?
			//e.Location
			var selectedCommandId = e.SelectedCommandId;// this is a getter / setter ?
			//
			// Summary:
			//     Gets or sets whether the Microsoft.Web.WebView2.Core.CoreWebView2.ContextMenuRequested
			//     event is handled by host after the event handler completes or after the deferral
			//     is completed if there is a taken Microsoft.Web.WebView2.Core.CoreWebView2Deferral.
			//
			// Remarks:
			//     If Handled is set to true then WebView2 will not display a context menu and will
			//     instead use the Microsoft.Web.WebView2.Core.CoreWebView2ContextMenuRequestedEventArgs.SelectedCommandId
			//     property to indicate which, if any, context menu item to invoke.
			//
			//     If after the
			//     event handler or deferral completes, Handled is set to false then WebView will
			//     display a context menu based on the contents of the Microsoft.Web.WebView2.Core.CoreWebView2ContextMenuRequestedEventArgs.MenuItems
			//     property. The default value is false.
			var handled = e.Handled;


			var menuItems = e.MenuItems;
			var menuItemsType = menuItems.GetType().FullName;

			// 35004 SaveAs
			var bannedCommandIds = new List<int> { 33000, 33001, 33002, 35003, 50460 };
			var commandMenuItems = menuItems.Where(menuItem => menuItem.Kind == Microsoft.Web.WebView2.Core.CoreWebView2ContextMenuItemKind.Command);
			// is there a circumstance where the WebView creates Kind Submenu ?
			var bannedCommandMenuItems = commandMenuItems.Where(menuItem => bannedCommandIds.Contains(menuItem.CommandId)).ToList();

			// this works - but if you want to provide own logic for current will need to provide own WPF ContextMenu
			foreach (var bannedCommandMenuItem in bannedCommandMenuItems)
			{
				menuItems.Remove(bannedCommandMenuItem);
			}
			// would check no getting separators in the wrong place
			foreach (var menuItem in menuItems.Where(menuItem => menuItem.Kind == CoreWebView2ContextMenuItemKind.Command))
			{
				var name = menuItem.Name;
				// does this then override default functionality
				menuItem.CustomItemSelected += (s, o) =>
				{
					// what is o ?
				};
			}

		}

        public void Handle(LogMessage message)
        {
            if (domContentLoaded)
            {
				_ = PostLogMessageAsync(message);
            }
            else
            {
				earlyLogMessages.Add(message);
            }
			
        }

        public void Handle(CoverageStoppedMessage message)
        {
			_ = PostJsonAsync<object>("coverageStopped", null);
        }

        public void Handle(ClearReportMessage message)
        {
			report = null;
			_ = PostReportAsync();
		}
    }
}