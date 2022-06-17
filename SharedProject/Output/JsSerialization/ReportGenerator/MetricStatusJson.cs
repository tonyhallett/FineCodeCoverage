namespace FineCodeCoverage.Output.JsSerialization.ReportGenerator
{
	public class MetricStatusJson
	{
#pragma warning disable IDE1006 // Naming Styles
		public bool exceeded { get; }
		public int metricIndex { get; }
#pragma warning restore IDE1006 // Naming Styles
		public MetricStatusJson(bool exceeded, int metricIndex)
		{
			this.exceeded = exceeded;
			this.metricIndex = metricIndex;
		}
	}

}
