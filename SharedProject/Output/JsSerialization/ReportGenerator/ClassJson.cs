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

        public int assemblyIndex { get; }

        public List<CodeFileJson> files { get; }

        public string name { get; }
        public string displayName { get; }

        public int? coveredBranches { get; }
        public int? totalBranches { get; }
        public decimal? branchCoverageQuota { get; }

        public int coveredCodeElements { get; }
        public int totalCodeElements { get; }
        public decimal? codeElementCoverageQuota { get; }

        public int coveredLines { get; }
        public int coverableLines { get; }
        public decimal? coverageQuota { get; }
        public int? totalLines { get; }

        public CoverageType coverageType { get; }
        
        
#pragma warning restore IDE1006 // Naming Styles

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
