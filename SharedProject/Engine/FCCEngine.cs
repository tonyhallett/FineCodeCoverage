using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FineCodeCoverage.Core.Utilities;
using FineCodeCoverage.Engine.Cobertura;
using FineCodeCoverage.Engine.Messages;
using FineCodeCoverage.Engine.Model;
using FineCodeCoverage.Engine.MsTestPlatform;
using FineCodeCoverage.Engine.MsTestPlatform.CodeCoverage;
using FineCodeCoverage.Options;
using FineCodeCoverage.ReportGeneration;
using Palmmedia.ReportGenerator.Core.Parser.Analysis;

namespace FineCodeCoverage.Engine
{
    [Export(typeof(IFCCEngine))]
    internal class FCCEngine : IFCCEngine, IDisposable
    {
        private class CoverageTaskState
        {
            public CancellationTokenSource CancellationTokenSource { get; set; }
            public Action CleanUp { get; set; }
        }

        internal int InitializeWait { get; set; } = 5000;
        internal const string initializationFailedMessagePrefix = "Initialization failed.  Please check the following error which may be resolved by reopening visual studio which will start the initialization process again.";
        private CancellationTokenSource cancellationTokenSource;

        public string AppDataFolderPath { get; private set; }
        private bool IsVsShutdown => this.disposeAwareTaskRunner.DisposalToken.IsCancellationRequested;

        private readonly ICoverageUtilManager coverageUtilManager;
        private readonly ICoberturaUtil coberturaUtil;
        private readonly IMsCodeCoverageRunSettingsService msCodeCoverageRunSettingsService;
        private readonly IMsTestPlatformUtil msTestPlatformUtil;
        private readonly IReportGeneratorUtil reportGeneratorUtil;
        private readonly ILogger logger;
        private readonly IAppDataFolder appDataFolder;

        private readonly ICoverageToolOutputManager coverageOutputManager;
        internal Task reloadCoverageTask;
#pragma warning disable IDE0052 // Remove unread private members
        private readonly ISolutionEvents solutionEvents; // keep alive
#pragma warning restore IDE0052 // Remove unread private members
        private readonly IEventAggregator eventAggregator;
        private readonly IDisposeAwareTaskRunner disposeAwareTaskRunner;
        private bool disposed = false;

        [ImportingConstructor]
        public FCCEngine(
            ICoverageUtilManager coverageUtilManager,
            ICoberturaUtil coberturaUtil,
            IMsTestPlatformUtil msTestPlatformUtil,
            IReportGeneratorUtil reportGeneratorUtil,
            ILogger logger,
            IAppDataFolder appDataFolder,
            ICoverageToolOutputManager coverageOutputManager,
            IMsCodeCoverageRunSettingsService msCodeCoverageRunSettingsService,
            ISolutionEvents solutionEvents,
            IAppOptionsProvider appOptionsProvider,
            IEventAggregator eventAggregator,
            IDisposeAwareTaskRunner disposeAwareTaskRunner
            )
        {
            this.solutionEvents = solutionEvents;
            this.eventAggregator = eventAggregator;
            this.disposeAwareTaskRunner = disposeAwareTaskRunner;
            solutionEvents.AfterClosing += (s, args) => this.ClearUI();
            appOptionsProvider.OptionsChanged += (appOptions) =>
            {
                if (!appOptions.Enabled)
                {
                    this.ClearUI();
                }
            };
            this.coverageOutputManager = coverageOutputManager;
            this.coverageUtilManager = coverageUtilManager;
            this.coberturaUtil = coberturaUtil;
            this.msTestPlatformUtil = msTestPlatformUtil;
            this.reportGeneratorUtil = reportGeneratorUtil;
            this.logger = logger;
            this.appDataFolder = appDataFolder;
            this.msCodeCoverageRunSettingsService = msCodeCoverageRunSettingsService;
        }

        internal string GetLogReloadCoverageStatusMessage(ReloadCoverageStatus reloadCoverageStatus) => $"================================== {reloadCoverageStatus.ToString().ToUpper()} ==================================";

        private void LogReloadCoverageStatus(ReloadCoverageStatus reloadCoverageStatus) => this.logger.Log(this.GetLogReloadCoverageStatusMessage(reloadCoverageStatus));

        public void Initialize(CancellationToken cancellationToken)
        {
            this.appDataFolder.Initialize(cancellationToken);
            this.AppDataFolderPath = this.appDataFolder.DirectoryPath;

            this.msTestPlatformUtil.Initialize(this.AppDataFolderPath, cancellationToken);
            this.coverageUtilManager.Initialize(this.AppDataFolderPath, cancellationToken);
            this.msCodeCoverageRunSettingsService.Initialize(this.AppDataFolderPath, this, cancellationToken);
        }

        public void ClearUI()
        {
            this.ClearCoverageLines();
            this.RaiseNewReport(null);
        }

        public void StopCoverage()
        {
            if (this.cancellationTokenSource != null)
            {
                try
                {
                    this.cancellationTokenSource.Cancel();
                }
                catch (ObjectDisposedException) { }
            }
        }

        private CancellationTokenSource Reset()
        {
            this.ClearCoverageLines();

            this.StopCoverage();

            this.cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(this.disposeAwareTaskRunner.DisposalToken);

            return this.cancellationTokenSource;
        }

        private async Task<string[]> RunCoverageAsync(List<ICoverageProject> coverageProjects, CancellationToken vsShutdownLinkedCancellationToken)
        {
            // process pipeline

            await this.PrepareCoverageProjectsAsync(coverageProjects, vsShutdownLinkedCancellationToken);

            foreach (ICoverageProject coverageProject in coverageProjects)
            {
                await coverageProject.StepAsync("Run Coverage Tool", async (project) =>
                {
                    DateTime start = DateTime.Now;

                    string coverageTool = this.coverageUtilManager.CoverageToolName(project);
                    string runCoverToolMessage = $"Run {coverageTool} ({project.ProjectName})";
                    this.logger.Log(runCoverToolMessage);
                    await this.coverageUtilManager.RunCoverageAsync(project, vsShutdownLinkedCancellationToken);

                    TimeSpan duration = DateTime.Now - start;
                    string durationMessage = $"Completed coverage for ({coverageProject.ProjectName}) : {duration}";
                    this.logger.Log(durationMessage);

                });

                if (coverageProject.HasFailed)
                {
                    string coverageStagePrefix = string.IsNullOrEmpty(coverageProject.FailureStage) ? "" : $"{coverageProject.FailureStage} ";
                    string failureMessage = $"{coverageProject.FailureStage}({coverageProject.ProjectName}) Failed.";
                    this.logger.Log(failureMessage, coverageProject.FailureDescription);
                }
            }

            IEnumerable<ICoverageProject> passedProjects = coverageProjects.Where(p => !p.HasFailed);

            return passedProjects
                    .Select(x => x.CoverageOutputFile)
                    .ToArray();

        }

        private void ClearCoverageLines() => this.RaiseCoverageLines(null);

        private void RaiseCoverageLines(IFileLineCoverage coverageLines) => this.eventAggregator.SendMessage(new NewCoverageLinesMessage { CoverageLines = coverageLines });

        private void UpdateUI(IFileLineCoverage coverageLines, SummaryResult summaryResult)
        {
            this.RaiseCoverageLines(coverageLines);
            this.RaiseNewReport(summaryResult);
        }

        private void RaiseNewReport(SummaryResult summaryResult) => this.eventAggregator.SendMessage(new NewReportMessage(summaryResult));

        private ReportResult RunAndProcessReport(string[] coverOutputFiles, CancellationToken vsShutdownLinkedCancellationToken)
        {
            string reportOutputFolder = this.coverageOutputManager.GetReportOutputFolder();
            vsShutdownLinkedCancellationToken.ThrowIfCancellationRequested();
            ReportGeneratorResult result = this.reportGeneratorUtil.Generate(coverOutputFiles, reportOutputFolder, vsShutdownLinkedCancellationToken);

            vsShutdownLinkedCancellationToken.ThrowIfCancellationRequested();
            this.logger.Log("Processing cobertura");
            IFileLineCoverage coverageLines = this.coberturaUtil.ProcessCoberturaXml(result.UnifiedXmlFile);

            vsShutdownLinkedCancellationToken.ThrowIfCancellationRequested();
            return new ReportResult
            {
                FileLineCoverage = coverageLines,
                HotspotsFile = result.HotspotsFile,
                CoberturaFile = result.UnifiedXmlFile,
                SummaryResult = result.SummaryResult,
            };
        }

        private async Task PrepareCoverageProjectsAsync(List<ICoverageProject> coverageProjects, CancellationToken cancellationToken)
        {
            foreach (ICoverageProject project in coverageProjects)
            {
                if (string.IsNullOrWhiteSpace(project.ProjectFile))
                {
                    project.FailureDescription = $"Unsupported project type for DLL '{project.TestDllFile}'";
                    continue;
                }

                if (!project.Settings.Enabled)
                {
                    project.FailureDescription = $"Disabled";
                    continue;
                }

                CoverageProjectFileSynchronizationDetails fileSynchronizationDetails = await project.PrepareForCoverageAsync(cancellationToken);
                List<string> logs = fileSynchronizationDetails.Logs;
                if (logs.Any())
                {
                    logs.Add($"File synchronization duration : {fileSynchronizationDetails.Duration}");
                    this.logger.Log(logs);
                }
            }
        }

        private void DisplayCoverageResult(Task<ReportResult> t, object state)
        {
            var displayCoverageResultState = (CoverageTaskState)state;
            if (!this.IsVsShutdown)
            {
                switch (t.Status)
                {
                    case TaskStatus.Canceled:
                        this.LogReloadCoverageStatus(ReloadCoverageStatus.Cancelled);
                        break;
                    case TaskStatus.Faulted:
                        Exception innerException = t.Exception.InnerExceptions[0];
                        this.logger.Log(
                            this.GetLogReloadCoverageStatusMessage(ReloadCoverageStatus.Error),
                            innerException
                        );
                        break;
                    case TaskStatus.RanToCompletion:
                        this.LogReloadCoverageStatus(ReloadCoverageStatus.Done);
#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning disable VSTHRD002 // Avoid problematic synchronous waits
                        this.UpdateUI(t.Result.FileLineCoverage, t.Result.SummaryResult);
                        this.RaiseReportFiles(t.Result);
#pragma warning restore VSTHRD002 // Avoid problematic synchronous waits
#pragma warning restore IDE0079 // Remove unnecessary suppression
                        break;
                }
            }

            displayCoverageResultState.CleanUp?.Invoke();
            displayCoverageResultState.CancellationTokenSource.Dispose();
        }

        private void RaiseReportFiles(ReportResult reportResult)
        {
            if (reportResult.HotspotsFile != null)
            {
                this.eventAggregator.SendMessage(new ReportFilesMessage { CoberturaFile = reportResult.CoberturaFile, HotspotsFile = reportResult.HotspotsFile });
            }
        }

        public void RunAndProcessReport(string[] coberturaFiles, Action cleanUp = null) => this.RunCancellableCoverageTask((vsShutdownLinkedCancellationToken) =>
                                                                                                    {
                                                                                                        var reportResult = new ReportResult();

                                                                                                        if (coberturaFiles.Any())
                                                                                                        {
                                                                                                            reportResult = this.RunAndProcessReport(coberturaFiles, vsShutdownLinkedCancellationToken);
                                                                                                        }

                                                                                                        return Task.FromResult(reportResult);
                                                                                                    }, cleanUp);

        private void RunCancellableCoverageTask(
            Func<CancellationToken, Task<ReportResult>> reportResultProvider, Action cleanUp)
        {
            CancellationTokenSource vsLinkedCancellationTokenSource = this.Reset();
            CancellationToken vsShutdownLinkedCancellationToken = vsLinkedCancellationTokenSource.Token;

            this.disposeAwareTaskRunner.RunAsyncFunc(() =>
            {
                this.reloadCoverageTask = Task.Run(async () =>
                {
                    ReportResult result = await reportResultProvider(vsShutdownLinkedCancellationToken);
                    return result;

                }, vsShutdownLinkedCancellationToken)
                .ContinueWith(this.DisplayCoverageResult, new CoverageTaskState { CancellationTokenSource = vsLinkedCancellationTokenSource, CleanUp = cleanUp }, TaskScheduler.Default);
                return this.reloadCoverageTask;
            });
        }

        public void ReloadCoverage(Func<Task<List<ICoverageProject>>> coverageRequestCallback) => this.RunCancellableCoverageTask(async (vsShutdownLinkedCancellationToken) =>
                                                                                                           {
                                                                                                               var reportResult = new ReportResult();

                                                                                                               this.LogReloadCoverageStatus(ReloadCoverageStatus.Start);

                                                                                                               List<ICoverageProject> coverageProjects = await coverageRequestCallback();
                                                                                                               vsShutdownLinkedCancellationToken.ThrowIfCancellationRequested();

                                                                                                               this.coverageOutputManager.SetProjectCoverageOutputFolder(coverageProjects);

                                                                                                               string[] coverOutputFiles = await this.RunCoverageAsync(coverageProjects, vsShutdownLinkedCancellationToken);
                                                                                                               if (coverOutputFiles.Any())
                                                                                                               {
                                                                                                                   reportResult = this.RunAndProcessReport(coverOutputFiles, vsShutdownLinkedCancellationToken);
                                                                                                               }

                                                                                                               return reportResult;
                                                                                                           }, null);

        public void Dispose()
        {
            this.Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing && this.cancellationTokenSource != null)
                {
                    this.cancellationTokenSource.Dispose();
                }

                this.disposed = true;
            }
        }
    }
}