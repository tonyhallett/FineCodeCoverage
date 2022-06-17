using Newtonsoft.Json;
using Palmmedia.ReportGenerator.Core.Parser.Analysis;
using System.Collections.Generic;
using System.Linq;

namespace FineCodeCoverage.Output.JsSerialization.ReportGenerator
{
    public class ClassJson
    {
#pragma warning disable IDE1006 // Naming Styles
        [JsonIgnore]
        public int index { get; }

        [JsonIgnore]
        public Class Class { get; }

        public int assemblyIndex { get; set; }

        public List<CodeFileJson> files { get; set; }

        public string name { get; set; }
        public string displayName { get; set; }

        public int? coveredBranches { get; set; }
        public int? totalBranches { get; set; }
        public decimal? branchCoverageQuota { get; }

        public int coveredCodeElements { get; set; }
        public int totalCodeElements { get; set; }
        public decimal? codeElementCoverageQuota { get; set; }

        public int coveredLines { get; set; }
        public int coverableLines { get; set; }
        public decimal? coverageQuota { get; set; }
        public int? totalLines { get; set; }

        public CoverageType coverageType { get; set; }


#pragma warning restore IDE1006 // Naming Styles
        [JsonConstructor]
        public ClassJson() { }
        public ClassJson(Class @class, int index, int assemblyIndex)
        {
            Class = @class;
            files = @class.Files.Select(file => new CodeFileJson(file)).ToList();
            this.assemblyIndex = assemblyIndex;

            name = @class.Name;
            displayName = @class.DisplayName;

            totalBranches = @class.TotalBranches;
            coveredBranches = @class.CoveredBranches;
            branchCoverageQuota = @class.BranchCoverageQuota;

            totalLines = @class.TotalLines;
            coverableLines = @class.CoverableLines;
            coveredLines = @class.CoveredLines;
            coverageQuota = @class.CoverageQuota;
            coverageType = @class.CoverageType;

            totalCodeElements = @class.TotalCodeElements;
            coveredCodeElements = @class.CoveredCodeElements;
            codeElementCoverageQuota = @class.CodeElementCoverageQuota;
            
            this.index = index;
        }
    }

}
