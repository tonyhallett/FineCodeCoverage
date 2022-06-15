using System;

namespace FineCodeCoverage.Output.JsPosting
{
	internal interface IReportOptionsProvider
	{
		event EventHandler<ReportOptions> ReportOptionsChanged;
		ReportOptions Provide();

	}
}
