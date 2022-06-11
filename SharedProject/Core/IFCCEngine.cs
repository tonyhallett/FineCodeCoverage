using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FineCodeCoverage.Engine.Model;
using FineCodeCoverage.Impl;

namespace FineCodeCoverage.Engine
{
    internal interface IFCCEngine
    {
        string AppDataFolderPath { get; }
        void Initialize(IInitializeStatusProvider initializeStatusProvider, System.Threading.CancellationToken cancellationToken);
        void StopCoverage();
        List<CoverageLine> RunAndProcessReport(string[] coverOutputFiles, CancellationToken vsShutdownLinkedCancellationToken);
        void ClearUI();

        void RunCancellableCoverageTask(
            Func<CancellationToken, Task<List<CoverageLine>>> reportResultProvider, Action cleanUp);
    }
        

}