using Palmmedia.ReportGenerator.Core.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;

namespace FineCodeCoverage.Output.JsSerialization
{
	public class RiskHotspotJson
	{
		public int assemblyIndex { get; }
		public int classIndex { get; }
		public MethodMetricJson methodMetric { get; }

		public int fileIndex { get; }

		public IEnumerable<MetricStatusJson> statusMetrics { get; }

		public RiskHotspotJson(RiskHotspot riskHotspot, List<AssemblyJson> assemblies)
		{
			var rgAssembly = assemblies.First(assembly => assembly.Assembly == riskHotspot.Assembly);
			assemblyIndex = rgAssembly.index;
			var rgClass = rgAssembly.classes.First(@class => @class.Class == riskHotspot.Class);
			classIndex = rgClass.index;
			methodMetric = new MethodMetricJson(riskHotspot.MethodMetric);
			var metrics = methodMetric.metrics;
			statusMetrics = riskHotspot.StatusMetrics.Select(statusMetric =>
			{
				var metricIndex = metrics.First(metric => metric.Metric == statusMetric.Metric).index;
				return new MetricStatusJson(statusMetric.Exceeded, metricIndex);
			});

			fileIndex = riskHotspot.FileIndex;
		}
	}
}
