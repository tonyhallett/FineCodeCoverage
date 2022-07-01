using Microsoft.Web.WebView2.Core;
using System;
using System.Windows;

namespace FineCodeCoverage.Output.WebView
{
	internal interface IWebViewControl
	{
		event EventHandler DomContentLoaded;
		void SetWebViewVisibility(Visibility visibility);
		void SetWebViewVerticalAlignment(VerticalAlignment verticalAlignment);
		void SetWebViewHorizontalAlignment(HorizontalAlignment horizontalAlignment);
		void PostWebMessageAsJson(string webMessage);
		void AddHostObjectToScript(string name, object rawObject);
		void Navigate(string htmlPath);
		void Reload();
		void SetVirtualHostNameToFolderMapping(
			string hostName, string folderPath, CoreWebView2HostResourceAccessKind accessKind);

		void Instantiate();

		void AddTextBlock(string text, ITextBlockDynamicResourceNames textBlockDynamicResourceNames);
		void UpdateTextBlock(string text);
		void RemoveTextBlock();
	}
}
