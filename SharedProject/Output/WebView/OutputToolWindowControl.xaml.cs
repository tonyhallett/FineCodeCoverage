using System.Windows;
using System.Windows.Controls;
using System;
using Microsoft.Web.WebView2.Wpf;
using Microsoft.Web.WebView2.Core;
using Task = System.Threading.Tasks.Task;
using FineCodeCoverage.Output.WebView;
using FineCodeCoverage.Automation.Controls;

namespace FineCodeCoverage.Output
{
	/// <summary>
	/// Interaction logic for OutputToolWindowControl.
	/// </summary>
	internal partial class OutputToolWindowControl : 
		UserControl, 
		IWebView
	{
		private WebView2 _webView2;
        private TextBlock textBlock;
        private readonly IWebViewController webViewController;

		private Grid Grid=> this.Content as Grid;
		public const string WebViewAutomationId = "WebView";
		public const string InitializeTextBlockAutomationId = "InitializeTextBlock";

		/// <summary>
		/// Initializes a new instance of the <see cref="OutputToolWindowControl"/> class.
		/// </summary>
		public OutputToolWindowControl(
			IWebViewController webViewController
		)
		{
            this.webViewController = webViewController;
            InitializeComponent();
			webViewController.Initialize(this);
		}

		public void Instantiate()
        {
			_webView2 = new WebView2();
			_webView2.SetValue(System.Windows.Automation.AutomationProperties.AutomationIdProperty, WebViewAutomationId);
			Grid.Children.Add(_webView2);
			_webView2.CoreWebView2InitializationCompleted += Webview2_CoreWebView2InitializationCompleted;
			 _ = InitializeWebViewEnvironmentAsync();
		}

		public void AddTextBlock(string text, ITextBlockDynamicResourceNames textBlockDynamicResourceNames)
        {
#if DEBUG
			textBlock = new DependencyPropertiesItemStatusTextBlock();
#else
			textBlock = new TextBlock();
#endif
			textBlock.Text = text;
			textBlock.Margin = new Thickness(5);
			textBlock.SetValue(System.Windows.Automation.AutomationProperties.AutomationIdProperty, InitializeTextBlockAutomationId);
			if (textBlockDynamicResourceNames != null)
			{
				textBlock.SetResourceReference(TextBlock.FontFamilyProperty, textBlockDynamicResourceNames.FontFamily);
				textBlock.SetResourceReference(TextBlock.FontSizeProperty, textBlockDynamicResourceNames.FontSize);
				textBlock.SetResourceReference(TextBlock.BackgroundProperty, textBlockDynamicResourceNames.Background);
				textBlock.SetResourceReference(TextBlock.ForegroundProperty, textBlockDynamicResourceNames.Foreground);
			}

			Grid.Children.Add(textBlock);
        }

		public void UpdateTextBlock(string text)
        {
			textBlock.Text = text;
        }

		public void RemoveTextBlock()
        {
			textBlock.Visibility = Visibility.Hidden;
        }

		private async Task InitializeWebViewEnvironmentAsync()
		{
			CoreWebView2EnvironmentOptions options = new CoreWebView2EnvironmentOptions(
				webViewController.AdditionalBrowserArguments
			);
			var environment = await CoreWebView2Environment.CreateAsync(
				userDataFolder: webViewController.UserDataFolder, 
				options: options
			);
			await _webView2.EnsureCoreWebView2Async(environment);
		}
		
		private void InitializeWebViewSettings()
        {
			var settings = _webView2.CoreWebView2.Settings;

			var controllerSettings = webViewController.WebViewSettings;

			settings.IsGeneralAutofillEnabled = controllerSettings.IsGeneralAutofillEnabled;
			settings.IsPasswordAutosaveEnabled = controllerSettings.IsPasswordAutosaveEnabled;
			settings.IsStatusBarEnabled = controllerSettings.IsStatusBarEnabled;
			settings.AreDefaultContextMenusEnabled = controllerSettings.AreDefaultContextMenusEnabled;
			settings.AreDevToolsEnabled = controllerSettings.AreDevToolsEnabled;
		}
		
		// note that can only access the CoreWebView2 here
		private void Webview2_CoreWebView2InitializationCompleted(object sender, CoreWebView2InitializationCompletedEventArgs e)
		{
			if (e.IsSuccess)
			{
				InitializeWebViewSettings();
				var coreWebView2 = _webView2.CoreWebView2;
				_webView2.CoreWebView2.DOMContentLoaded += CoreWebView2_DOMContentLoaded;
				coreWebView2.ProcessFailed += CoreWebView2_ProcessFailed;
				webViewController.CoreWebView2InitializationCompleted();
			}
			else
			{
				//todo
				var exc = e.InitializationException;
			}
		}

		private void CoreWebView2_DOMContentLoaded(object sender, CoreWebView2DOMContentLoadedEventArgs e)
		{
			DomContentLoaded?.Invoke(this, e);
		}

		//todo
		private void CoreWebView2_ProcessFailed(object sender, CoreWebView2ProcessFailedEventArgs e)
		{
			webViewController.ProcessFailed(e);
		}

#region IWebView
		public FrameworkElement FrameworkElement => _webView2;
		public event EventHandler DomContentLoaded;

#region control properties
        public void SetWebViewVisibility(Visibility visibility)
        {
			_webView2.Visibility = visibility;
        }

        public void SetWebViewVerticalAlignment(VerticalAlignment verticalAlignment)
        {
			_webView2.VerticalAlignment = verticalAlignment;
        }

        public void SetWebViewHorizontalAlignment(HorizontalAlignment horizontalAlignment)
        {
			_webView2.HorizontalAlignment = horizontalAlignment;
        }
#endregion
#region CoreWebView2
		public void PostWebMessageAsJson(string webMessage)
		{
			_webView2.CoreWebView2.PostWebMessageAsJson(webMessage);
		}

		public void AddHostObjectToScript(string name, object rawObject)
        {
            _webView2.CoreWebView2.AddHostObjectToScript(name, rawObject);
        }

        public void Navigate(string htmlPath)
        {
			_webView2.CoreWebView2.Navigate(htmlPath);
        }

		public void Reload()
        {
			_webView2.CoreWebView2.Reload();
        }

        public void SetVirtualHostNameToFolderMapping(string hostName, string folderPath, CoreWebView2HostResourceAccessKind accessKind)
        {
			_webView2.CoreWebView2.SetVirtualHostNameToFolderMapping(hostName, folderPath, accessKind);
        }

		public void SetZoomFactor(double zoomFactor)
		{
			_webView2.ZoomFactor = zoomFactor;

        }
		#endregion
		#endregion
	}
}