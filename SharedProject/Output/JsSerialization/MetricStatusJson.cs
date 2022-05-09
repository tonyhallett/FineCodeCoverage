﻿namespace FineCodeCoverage.Output.JsSerialization
{
	public class MetricStatusJson
	{
		public bool exceeded { get; }
		public int metricIndex { get; }
		public MetricStatusJson(bool exceeded, int metricIndex)
		{
			this.exceeded = exceeded;
			this.metricIndex = metricIndex;
		}
	}

}
