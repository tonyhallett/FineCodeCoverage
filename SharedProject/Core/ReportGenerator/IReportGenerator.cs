using Palmmedia.ReportGenerator.Core;
using Palmmedia.ReportGenerator.Core.CodeAnalysis;
using Palmmedia.ReportGenerator.Core.Logging;
using Palmmedia.ReportGenerator.Core.Reporting;
using System;

namespace FineCodeCoverage.Core.ReportGenerator
{
    internal interface IReportGenerator
    {
        bool GenerateReport(IReportConfiguration reportConfiguration, Settings settings, RiskHotspotsAnalysisThresholds riskHotspotsAnalysisThresholds);
        void SetLogger(Action<VerbosityLevel, string> logger);
    }
}
