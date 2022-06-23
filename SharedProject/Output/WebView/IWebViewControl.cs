using Microsoft.Web.WebView2.Core;
using System;
using System.Threading.Tasks;
using System.Windows;

namespace FineCodeCoverage.Output.WebView
{
	internal interface IWebViewControl
	{
		event EventHandler DomContentLoaded;
		void SetVisibility(Visibility visibility);
		void SetVerticalAlignment(VerticalAlignment verticalAlignment);
		void SetHorizontalAlignment(HorizontalAlignment horizontalAlignment);
		void PostWebMessageAsJson(string webMessage);
		void AddHostObjectToScript(string name, object rawObject);
		void Navigate(string htmlPath);
		void Reload();
		void SetVirtualHostNameToFolderMapping(
			string hostName, string folderPath, CoreWebView2HostResourceAccessKind accessKind);

		void Instantiate();
	}
}
