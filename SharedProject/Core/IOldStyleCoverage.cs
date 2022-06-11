using FineCodeCoverage.Engine.Model;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FineCodeCoverage.Core
{
    internal interface IOldStyleCoverage : ICoverageService
    {
        void CollectCoverage(Func<Task<List<ICoverageProject>>> getCoverageProjects);
    }
}
