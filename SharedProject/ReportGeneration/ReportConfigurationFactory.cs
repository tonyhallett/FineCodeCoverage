using System.ComponentModel.Composition;
using Palmmedia.ReportGenerator.Core;
using Palmmedia.ReportGenerator.Core.Reporting;

namespace FineCodeCoverage.ReportGeneration
{
    [Export(typeof(IReportConfigurationFactory))]
    internal class ReportConfigurationFactory : IReportConfigurationFactory
    {
        public IReportConfiguration Create(FCCReportConfiguration fccReportConfiguration) => new ReportConfiguration(
                fccReportConfiguration.CoverageOutputFiles,
                fccReportConfiguration.TargetDirectory,
                fccReportConfiguration.SourceDirectories,
                fccReportConfiguration.HistoryDirectory,
                fccReportConfiguration.ReportTypes,
                fccReportConfiguration.Plugins,
                fccReportConfiguration.AssemblyFilters,
                fccReportConfiguration.ClassFilters,
                fccReportConfiguration.FileFilters,
                fccReportConfiguration.VerbosityLevel,
                fccReportConfiguration.Tag
            );
    }
}
