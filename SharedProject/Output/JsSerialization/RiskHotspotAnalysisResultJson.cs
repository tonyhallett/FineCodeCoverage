using Palmmedia.ReportGenerator.Core.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;

namespace FineCodeCoverage.Output.JsSerialization
{
	public class RiskHotspotAnalysisResultJson
	{
		public bool codeCodeQualityMetricsAvailable { get; }
		public IEnumerable<RiskHotspotJson> riskHotspots { get; }
		public RiskHotspotAnalysisResultJson(RiskHotspotAnalysisResult riskHotspotAnalysisResult, List<AssemblyJson> assemblies)
		{
			codeCodeQualityMetricsAvailable = riskHotspotAnalysisResult.CodeCodeQualityMetricsAvailable;
			riskHotspots = riskHotspotAnalysisResult.RiskHotspots.Select(riskHotspot => new RiskHotspotJson(riskHotspot, assemblies));
		}
	}
}
