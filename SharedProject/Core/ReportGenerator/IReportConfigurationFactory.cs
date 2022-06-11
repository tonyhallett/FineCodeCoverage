using Palmmedia.ReportGenerator.Core.Reporting;

namespace FineCodeCoverage.Core.ReportGenerator
{
	internal interface IReportConfigurationFactory
	{
		IReportConfiguration Create(FCCReportConfiguration fCCReportConfiguration);
	}
}
