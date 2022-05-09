using Newtonsoft.Json;
using Palmmedia.ReportGenerator.Core.Parser.Analysis;

namespace FineCodeCoverage.Output.JsSerialization
{
	public class MetricJson
	{
		[JsonIgnore]
		public Metric Metric { get; }

		public string metricType { get; }
		public string mergeOrder { get; }
		public string explanationUrl { get; }
		public string name { get; }
		public decimal? value { get; }

		[JsonIgnore]
		public int index { get; }
		public MetricJson(Metric metric, int index)
		{
			this.index = index;
			Metric = metric;
			metricType = metric.MetricType.ToString();
			explanationUrl = metric.ExplanationUrl.ToString();
			mergeOrder = metric.MergeOrder.ToString();
			name = metric.Name;
			value = metric.Value;
		}
	}
}
