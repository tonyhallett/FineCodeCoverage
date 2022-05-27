using Newtonsoft.Json;
using Palmmedia.ReportGenerator.Core.Parser.Analysis;

namespace FineCodeCoverage.Output.JsSerialization.ReportGenerator
{
	public class MetricJson
	{
		[JsonIgnore]
		public Metric Metric { get; }

		public MetricType metricType { get; }
		public MetricMergeOrder mergeOrder { get; }
		public string explanationUrl { get; }
		public string name { get; }
		public decimal? value { get; }

		[JsonIgnore]
		public int index { get; }
		public MetricJson(Metric metric, int index)
		{
			this.index = index;
			Metric = metric;
			metricType = metric.MetricType;
			explanationUrl = metric.ExplanationUrl.ToString();
			mergeOrder = metric.MergeOrder;
			name = metric.Name;
			value = metric.Value;
		}
	}
}
