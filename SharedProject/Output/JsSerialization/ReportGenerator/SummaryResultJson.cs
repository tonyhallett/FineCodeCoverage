using Palmmedia.ReportGenerator.Core.Parser.Analysis;
using System.Collections.Generic;
using System.Linq;

namespace FineCodeCoverage.Output.JsSerialization.ReportGenerator
{
	public class SummaryResultJson
	{
		public int coveredLines { get; set; }
		public int coverableLines { get; set; }

		public int? totalLines { get; set; }

		public decimal? coverageQuota { get; set; }
		public int? coveredBranches { get; set; }
		public int? totalBranches { get; set; }
		public decimal? branchCoverageQuota { get; set; }
		public int coveredCodeElements { get; set; }
		public int totalCodeElements { get; set; }
		public decimal? codeElementCoverageQuota { get; set; }

		public List<AssemblyJson> assemblies { get; set; }
        public bool supportsBranchCoverage { get; }

        public SummaryResultJson() { }
		public SummaryResultJson(SummaryResult summaryResult)
		{
			coveredLines = summaryResult.CoveredLines;
			coverableLines = summaryResult.CoverableLines;
			totalLines = summaryResult.TotalLines;
			coverageQuota = summaryResult.CoverageQuota;
			coveredBranches = summaryResult.CoveredBranches;
			totalBranches = summaryResult.TotalBranches;
			branchCoverageQuota = summaryResult.BranchCoverageQuota;
			coveredCodeElements = summaryResult.CoveredCodeElements;
			totalCodeElements = summaryResult.TotalCodeElements;
			codeElementCoverageQuota = summaryResult.CodeElementCoverageQuota;

			assemblies = summaryResult.Assemblies.Select((assembly, i) => new AssemblyJson(assembly, i)).ToList();

			supportsBranchCoverage = summaryResult.SupportsBranchCoverage;
		}
	}

}
