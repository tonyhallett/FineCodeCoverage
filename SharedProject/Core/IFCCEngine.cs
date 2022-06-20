using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FineCodeCoverage.Engine.Model;

namespace FineCodeCoverage.Engine
{
    internal interface IFCCEngine
    {
        void StopCoverage();
        List<CoverageLine> RunAndProcessReport(string[] coverOutputFiles, CancellationToken vsShutdownLinkedCancellationToken);
        void ClearUI();

        void RunCancellableCoverageTask(
            Func<CancellationToken, Task<List<CoverageLine>>> reportResultProvider, Action cleanUp);
    }

}