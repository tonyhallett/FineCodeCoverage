using Palmmedia.ReportGenerator.Core;
using Palmmedia.ReportGenerator.Core.Reporting;
using System.Collections.Generic;
using System.ComponentModel.Composition;

namespace FineCodeCoverage.Core.ReportGenerator
{
    internal class FCCReportConfiguration
    {
        public FCCReportConfiguration(
            IEnumerable<string> reportFilePatterns,
            string targetDirectory,
            IEnumerable<string> sourceDirectories,
            string historyDirectory,
            IEnumerable<string> reportTypes,
            IEnumerable<string> plugins,
            IEnumerable<string> assemblyFilters,
            IEnumerable<string> classFilters,
            IEnumerable<string> fileFilters,
            string verbosityLevel,
            string tag)
        {
            ReportFilePatterns = reportFilePatterns;
            TargetDirectory = targetDirectory;
            SourceDirectories = sourceDirectories;
            HistoryDirectory = historyDirectory;
            ReportTypes = reportTypes;
            Plugins = plugins;
            AssemblyFilters = assemblyFilters;
            ClassFilters = classFilters;
            FileFilters = fileFilters;
            VerbosityLevel = verbosityLevel;
            Tag = tag;
        }

        public IEnumerable<string> ReportFilePatterns { get; }
        public string TargetDirectory { get; }
        public IEnumerable<string> SourceDirectories { get; }
        public string HistoryDirectory { get; }
        public IEnumerable<string> ReportTypes { get; }
        public IEnumerable<string> Plugins { get; }
        public IEnumerable<string> AssemblyFilters { get; }
        public IEnumerable<string> ClassFilters { get; }
        public IEnumerable<string> FileFilters { get; }
        public string VerbosityLevel { get; }
        public string Tag { get; }
    }

    [Export(typeof(IReportConfigurationFactory))]
    internal class ReportConfigurationFactory : IReportConfigurationFactory
    {
        public IReportConfiguration Create(FCCReportConfiguration fccReportConfiguration )
        {
            return new ReportConfiguration(
                fccReportConfiguration.ReportFilePatterns,
                fccReportConfiguration.TargetDirectory,
                fccReportConfiguration.SourceDirectories,
                fccReportConfiguration.HistoryDirectory,
                fccReportConfiguration.ReportTypes,
                fccReportConfiguration.Plugins,
                fccReportConfiguration.AssemblyFilters,
                fccReportConfiguration.ClassFilters,
                fccReportConfiguration.FileFilters,
                fccReportConfiguration.VerbosityLevel,
                fccReportConfiguration.Tag);

        }
    }
}
