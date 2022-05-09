using Palmmedia.ReportGenerator.Core.Parser.Analysis;
using System.Collections.Generic;
using System.Linq;

namespace FineCodeCoverage.Output.JsSerialization
{
	public class MethodMetricJson
	{
		public string shortName { get; }
		public string fullName { get; }
		public int? line { get; }
		public IEnumerable<MetricJson> metrics { get; }

		public MethodMetricJson(MethodMetric methodMetric)
		{
			shortName = methodMetric.ShortName;
			fullName = methodMetric.FullName;
			line = methodMetric.Line;
			metrics = methodMetric.Metrics.Select((metric, index) => new MetricJson(metric, index));
		}
	}

}
