using System.Windows;
using System.Windows.Controls;
using System;
using Microsoft.Web.WebView2.Wpf;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Web.WebView2.Core;
using Task = System.Threading.Tasks.Task;
using FineCodeCoverage.Output.WebView;

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
        private readonly IWebViewController webViewController;
        
		private const int remoteDebuggingPort = 9222;
		
        /// <summary>
        /// Initializes a new instance of the <see cref="OutputToolWindowControl"/> class.
        /// </summary>
        public OutputToolWindowControl(
			IWebViewController webViewController
		)
		{
            this.webViewController = webViewController;
            InitializeComponent();
            _ = InitializeWebViewAsync();
		}

		private async Task InitializeWebViewAsync()
        {
			_webView2 = new WebView2();
			webViewController.Initialize(this);
			this.AddChild(_webView2); // needs to be in the tree before 

			_webView2.CoreWebView2InitializationCompleted += Webview2_CoreWebView2InitializationCompleted;
			await InitializeWebViewEnvironmentAsync();
		}


        //todo - user data folder / move into controller
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
		
		//todo - context menu - turn off / move into controller
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
				_webView2.CoreWebView2.DOMContentLoaded += CoreWebView2_DOMContentLoaded;
				coreWebView2.ProcessFailed += CoreWebView2_ProcessFailed;
				coreWebView2.ContextMenuRequested += CoreWebView2_ContextMenuRequested;
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

		#region IWebView
		public FrameworkElement FrameworkElement => _webView2;
		public event EventHandler DomContentLoaded;
		public void PostWebMessageAsJson(string webMessage)
        {
			_webView2.CoreWebView2.PostWebMessageAsJson(webMessage);
        }

        public void SetVisibility(Visibility visibility)
        {
			_webView2.Visibility = visibility;
        }

        public void SetVerticalAlignment(VerticalAlignment verticalAlignment)
        {
			_webView2.VerticalAlignment = verticalAlignment;
        }

        public void SetHorizontalAlignment(HorizontalAlignment horizontalAlignment)
        {
			_webView2.HorizontalAlignment = horizontalAlignment;
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
		#endregion
	}
}