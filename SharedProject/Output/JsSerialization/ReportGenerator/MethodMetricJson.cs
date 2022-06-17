using Newtonsoft.Json;
using Palmmedia.ReportGenerator.Core.Parser.Analysis;
using System.Collections.Generic;
using System.Linq;

namespace FineCodeCoverage.Output.JsSerialization.ReportGenerator
{
	public class MethodMetricJson
	{
#pragma warning disable IDE1006 // Naming Styles
		public string shortName { get; }
		public string fullName { get; }
		public int? line { get; }
		public IEnumerable<MetricJson> metrics { get; }
#pragma warning restore IDE1006 // Naming Styles

		public MethodMetricJson(MethodMetric methodMetric)
		{
			shortName = methodMetric.ShortName;
			fullName = methodMetric.FullName;
			line = methodMetric.Line;
			metrics = methodMetric.Metrics.Select((metric, index) => new MetricJson(metric, index));
		}

		[JsonConstructor]
		public MethodMetricJson(
			string shortName,
			string fullName,
			int? line,
			IEnumerable<MetricJson> metrics
        )
        {
			this.shortName = shortName;
			this.fullName = fullName;
			this.line = line;
			this.metrics = metrics;
        }
	}

}
