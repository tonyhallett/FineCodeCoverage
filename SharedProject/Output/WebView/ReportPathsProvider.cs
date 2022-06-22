using System.ComponentModel.Composition;
using System.IO;

namespace FineCodeCoverage.Output.WebView
{
	[Export(typeof(IReportPathsProvider))]
	internal class ReportPathsProvider : IReportPathsProvider
	{
#if DEBUG
		internal bool debug = true;
#else
		internal bool debug = false;
#endif
		private class ReportPaths : IReportPaths
		{
			public string StandalonePath { get; set; }
			public string NavigationPath { get; set; }
			public bool ShouldWatch { get; set; }
		}

		private IReportPaths reportPaths;
		public IReportPaths Provide() // cache
		{
			if (reportPaths == null)
            {
				// todo checking AppOptions
				var standalonePath = GetStandalonePath();
				reportPaths = new ReportPaths
				{
					NavigationPath = debug ? DebugReportPath.Path : standalonePath,
					StandalonePath = standalonePath,
					ShouldWatch = debug
				};
			}
			return reportPaths;
		}

		private string GetStandalonePath()
		{
			var fccExtensionDirectory = Path.GetDirectoryName(this.GetType().Assembly.Location);
			return Path.Combine(fccExtensionDirectory, "Resources", "index.html");
		}
	}

}
