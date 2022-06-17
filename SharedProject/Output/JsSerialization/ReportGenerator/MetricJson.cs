using Newtonsoft.Json;
using Palmmedia.ReportGenerator.Core.Parser.Analysis;

namespace FineCodeCoverage.Output.JsSerialization.ReportGenerator
{
	public class MetricJson
	{
		[JsonIgnore]
		public Metric Metric { get; }

#pragma warning disable IDE1006 // Naming Styles
		[JsonIgnore]
		public int index { get; }
		public MetricType metricType { get; }
		public MetricMergeOrder mergeOrder { get; }
		public string explanationUrl { get; }
		public string name { get; }
		public decimal? value { get; }
#pragma warning restore IDE1006 // Naming Styles


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

		[JsonConstructor]
		public MetricJson(
			MetricType metricType,
			MetricMergeOrder mergeOrder,
			string explanationUrl,
			string name,
			decimal? value
		)
        {
			this.metricType = metricType;
			this.mergeOrder = mergeOrder;
			this.explanationUrl = explanationUrl;
			this.name = name;
			this.value = value;
        }
	}
}
