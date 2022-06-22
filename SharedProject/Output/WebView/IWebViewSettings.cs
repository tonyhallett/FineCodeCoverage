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

}
