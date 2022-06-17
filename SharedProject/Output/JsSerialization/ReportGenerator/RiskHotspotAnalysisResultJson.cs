using Newtonsoft.Json;
using Palmmedia.ReportGenerator.Core.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;

namespace FineCodeCoverage.Output.JsSerialization.ReportGenerator
{
	public class RiskHotspotAnalysisResultJson
	{
#pragma warning disable IDE1006 // Naming Styles
		public bool codeCodeQualityMetricsAvailable { get; }
		public IEnumerable<RiskHotspotJson> riskHotspots { get; }
#pragma warning restore IDE1006 // Naming Styles

		public RiskHotspotAnalysisResultJson(RiskHotspotAnalysisResult riskHotspotAnalysisResult, List<AssemblyJson> assemblies)
		{
			codeCodeQualityMetricsAvailable = riskHotspotAnalysisResult.CodeCodeQualityMetricsAvailable;
			riskHotspots = riskHotspotAnalysisResult.RiskHotspots.Select(riskHotspot => new RiskHotspotJson(riskHotspot, assemblies));
		}

		[JsonConstructor]
		public RiskHotspotAnalysisResultJson(bool codeCodeQualityMetricsAvailable, IEnumerable<RiskHotspotJson> riskHotspots) { 
			this.codeCodeQualityMetricsAvailable=codeCodeQualityMetricsAvailable;
			this.riskHotspots=riskHotspots;
		}
	}
}
