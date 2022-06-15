using FineCodeCoverage.Options;
using System;
using System.ComponentModel.Composition;

namespace FineCodeCoverage.Output.JsPosting
{
    [Export(typeof(IReportOptionsProvider))]
    internal class ReportOptionsProvider : IReportOptionsProvider
    {
        private readonly IAppOptionsProvider appOptionsProvider;
        private ReportOptions lastReportOptions;

        public event EventHandler<ReportOptions> ReportOptionsChanged;
        [ImportingConstructor]
        public ReportOptionsProvider(IAppOptionsProvider appOptionsProvider)
        {
            this.appOptionsProvider = appOptionsProvider;
            appOptionsProvider.OptionsChanged += AppOptionsProvider_OptionsChanged;
        }

        private void AppOptionsProvider_OptionsChanged(IAppOptions appOptions)
        {
            var newReportOptions = ReportOptions.Create(appOptions);
            if (ReportOptionsHaveChanged(lastReportOptions, newReportOptions))
            {
                lastReportOptions = newReportOptions;
                ReportOptionsChanged?.Invoke(this, newReportOptions);
            }

        }

        private static bool ReportOptionsHaveChanged(ReportOptions lastReportOptions, ReportOptions reportOptions)
        {
            if (lastReportOptions == null)
            {
                return true;
            }
            return !(lastReportOptions.namespacedClasses == reportOptions.namespacedClasses &&
                lastReportOptions.hideFullyCovered == reportOptions.hideFullyCovered
            );
        }

        public ReportOptions Provide()
        {
            if (lastReportOptions == null)
            {
                lastReportOptions = ReportOptions.Create(appOptionsProvider.Provide());
            }
            return lastReportOptions;
        }
    }
}
