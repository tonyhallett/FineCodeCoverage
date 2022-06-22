namespace FineCodeCoverage.Output.WebView
{
	internal interface IReportPaths
	{
		string NavigationPath { get; }
		bool ShouldWatch { get; }
		string StandalonePath { get; }
	}

}
