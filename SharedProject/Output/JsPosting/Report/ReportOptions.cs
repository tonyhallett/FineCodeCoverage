using FineCodeCoverage.Options;

namespace FineCodeCoverage.Output.JsPosting
{
    public class ReportOptions
    {
#pragma warning disable IDE1006 // Naming Styles
        public bool hideFullyCovered { get; private set; }

        public bool namespacedClasses { get; private set; }
#pragma warning restore IDE1006 // Naming Styles
        
        internal static ReportOptions Create(IAppOptions appOptions)
        {
            return new ReportOptions
            {
                hideFullyCovered = appOptions.HideFullyCovered,
                //appOptions.StickyCoverageTable
                namespacedClasses = appOptions.NamespacedClasses
            };
        }
    }
}
