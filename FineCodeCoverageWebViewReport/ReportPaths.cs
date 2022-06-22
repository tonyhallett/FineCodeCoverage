using FineCodeCoverage.Output.WebView;
using Newtonsoft.Json;
using System;

namespace FineCodeCoverageWebViewReport
{
    internal class ReportPaths : IReportPaths
    {
        public string NavigationPath { get; set; }

        public bool ShouldWatch { get; set; }

        public string StandalonePath { get; set; }

        public string Serialize()
        {
            return JsonConvert.SerializeObject(this);
        }

        public static ReportPaths Deserialize(string serialized) => 
            JsonConvert.DeserializeObject<ReportPaths>(serialized);
    }
}
