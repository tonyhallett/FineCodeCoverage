using System;
using System.Collections.Generic;
using FineCodeCoverage.Engine.Model;

namespace FineCodeCoverage.Impl
{
    internal interface ITestOperation
    {
        long FailedTests { get; }
        long TotalTests { get; }
        List<string> UnsupportedProjects { get; }
        System.Threading.Tasks.Task<List<ICoverageProject>> GetCoverageProjectsAsync();
        string SolutionDirectory { get; }
        IEnumerable<Uri> GetRunSettingsDataCollectorResultUri(Uri collectorUri);
    }
}



