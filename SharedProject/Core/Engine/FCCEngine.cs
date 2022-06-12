using FineCodeCoverage.Core.ReportGenerator;
using FineCodeCoverage.Core.Utilities;
using FineCodeCoverage.Engine.Cobertura;
using FineCodeCoverage.Engine.Model;
using FineCodeCoverage.Engine.MsTestPlatform;
using FineCodeCoverage.Engine.MsTestPlatform.CodeCoverage;
using FineCodeCoverage.Impl;
using FineCodeCoverage.Options;
using FineCodeCoverage.Output.JsMessages;
using FineCodeCoverage.Output.JsMessages.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Threading;
using System.Threading.Tasks;

namespace FineCodeCoverage.Engine
{

    [Export(typeof(IFCCEngine))]
    internal class FCCEngine : IFCCEngine
    {
        class DisplayCoverageResultState
        {
            public CancellationTokenSource CancellationTokenSource { get; set; }
            public Action CleanUp { get; set; }
        }

        // tests
        internal CancellationTokenSource cancellationTokenSource; 
        internal Task reloadCoverageTask;

        public string AppDataFolderPath { get; private set; }
        
        private readonly ICoverageUtilManager coverageUtilManager;
        private readonly ICoberturaUtil coberturaUtil;        
        private readonly IMsCodeCoverageRunSettingsService msCodeCoverageRunSettingsService;
        private readonly IMsTestPlatformUtil msTestPlatformUtil;
        private readonly IReportGeneratorUtil reportGeneratorUtil;
        private readonly ILogger logger;
        private readonly ICoverageToolOutputManager coverageOutputManager;
        private readonly IEventAggregator eventAggregator;
        private readonly IDisposeAwareTaskRunner disposeAwareTaskRunner;
        private readonly IExecutionTimer executionTimer;
        private readonly IAppDataFolder appDataFolder;
        private IInitializeStatusProvider initializeStatusProvider;

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
            IDisposeAwareTaskRunner disposeAwareTaskRunner,
            IExecutionTimer executionTimer
        )
        {
            this.eventAggregator = eventAggregator;
            this.disposeAwareTaskRunner = disposeAwareTaskRunner;
            this.executionTimer = executionTimer;
            this.coverageOutputManager = coverageOutputManager;
            this.coverageUtilManager = coverageUtilManager;
            this.coberturaUtil = coberturaUtil;
            this.msTestPlatformUtil = msTestPlatformUtil;
            this.reportGeneratorUtil = reportGeneratorUtil;
            this.logger = logger;
            this.appDataFolder = appDataFolder;
            this.msCodeCoverageRunSettingsService = msCodeCoverageRunSettingsService;

            solutionEvents.AfterClosing += SolutionEvents_AfterClosing;
            appOptionsProvider.OptionsChanged += AppOptionsProvider_OptionsChanged;

        }

        private void SolutionEvents_AfterClosing(object sender, EventArgs e)
        {
            eventAggregator.SendMessage(new ClearReportMessage());
        }

        private void AppOptionsProvider_OptionsChanged(IAppOptions appOptions)
        {
            if (!appOptions.Enabled)
            {
                ClearUI();
            }
        }

        public void Initialize(IInitializeStatusProvider initializeStatusProvider, CancellationToken cancellationToken)
        {
            this.initializeStatusProvider = initializeStatusProvider;

            appDataFolder.Initialize(cancellationToken);
            AppDataFolderPath = appDataFolder.DirectoryPath;

            msTestPlatformUtil.Initialize(AppDataFolderPath, cancellationToken);
            coverageUtilManager.Initialize(AppDataFolderPath, cancellationToken);
            msCodeCoverageRunSettingsService.Initialize(AppDataFolderPath, this,cancellationToken);
        }

        public void ClearUI()
        {
            ClearCoverageLines();
            eventAggregator.SendMessage(new ClearReportMessage());
        }

        public void StopCoverage()
        {           
            if (cancellationTokenSource != null)
            {
                try
                {
                    cancellationTokenSource.Cancel();
                }
                catch (ObjectDisposedException) { }
            }
        }
        
        private CancellationTokenSource Reset()
        {
            ClearCoverageLines();

            StopCoverage();

            cancellationTokenSource = disposeAwareTaskRunner.CreateLinkedCancellationTokenSource();

            return cancellationTokenSource;
        }

        private void ClearCoverageLines()
        {
            RaiseCoverageLines(null);
        }

        private void RaiseCoverageLines(List<CoverageLine> coverageLines)
        {
            eventAggregator.SendMessage(new NewCoverageLinesMessage { CoverageLines = coverageLines});
        }

        private void ThrowIfNoCoverageOutput(string[] coverOutputFiles)
        {
            if (coverOutputFiles.Length == 0)
            {
                throw new Exception("No coverage output files available for processing");
            }
        }

        private void LogGeneratingReports()
        {
            var generatingReportsMessage = "Generating reports";
            logger.Log(generatingReportsMessage);
            eventAggregator.SendMessage(LogMessage.Simple(MessageContext.ReportGeneratorStart, generatingReportsMessage));
        }

        private void LogGeneratedReport(TimeSpan duration)
        {
            var generatedReportsMessage = $"Generated reports - duration {duration.ToStringHoursMinutesSeconds()}";

            logger.Log(generatedReportsMessage);
            eventAggregator.SendMessage(LogMessage.Simple(MessageContext.ReportGeneratorCompleted, generatedReportsMessage));
        }

        private string GenerateReport(string[] coverOutputFiles, string reportOutputFolder, CancellationToken vsShutdownLinkedCancellationToken)
        {
            LogGeneratingReports();

            string unifiedXmlFile = null;
            var duration = executionTimer.Time(() =>
            {
                unifiedXmlFile = reportGeneratorUtil.Generate(coverOutputFiles, reportOutputFolder, vsShutdownLinkedCancellationToken);
            });

            LogGeneratedReport(duration);
            return unifiedXmlFile;
        }

        public List<CoverageLine> RunAndProcessReport(string[] coverOutputFiles, CancellationToken vsShutdownLinkedCancellationToken)
        {
            ThrowIfNoCoverageOutput(coverOutputFiles);

            vsShutdownLinkedCancellationToken.ThrowIfCancellationRequested();
            var reportOutputFolder = coverageOutputManager.GetReportOutputFolder();
            vsShutdownLinkedCancellationToken.ThrowIfCancellationRequested();

            var unifiedXmlFile = GenerateReport(coverOutputFiles, reportOutputFolder, vsShutdownLinkedCancellationToken);

            vsShutdownLinkedCancellationToken.ThrowIfCancellationRequested();
            return coberturaUtil.ProcessCoberturaXml(unifiedXmlFile);
        }

        private void LogCancelled()
        {
            logger.Log(CoverageStatus.Cancelled.Message());
            eventAggregator.SimpleLogToolWindow("Coverage cancelled", MessageContext.CoverageCancelled);
        }

        private void LogFaulted(AggregateException exception)
        {
            var innerException = exception.InnerExceptions[0];
            logger.Log(
                CoverageStatus.Error.Message(),
                innerException
            );
            eventAggregator.LogToolWindowFailure("An exception occurred. See the ");
        }

        private void LogCompleted()
        {
            logger.Log(CoverageStatus.Done.Message());
            eventAggregator.SimpleLogToolWindow("Coverage completed", MessageContext.CoverageCompleted);
        }

        private void CoverageTaskCancelled()
        {
            if (!disposeAwareTaskRunner.IsVsShutdown)
            {
                LogCancelled();
            }
        }

        private void SuccessfulCoverage(List<CoverageLine> coverageLines)
        {
            LogCompleted();
            RaiseCoverageLines(coverageLines);
        }

        private void EndOfCoverage(DisplayCoverageResultState displayCoverageResultState)
        {
            if (!disposeAwareTaskRunner.IsVsShutdown)
            {
                eventAggregator.SendMessage(new CoverageStoppedMessage());
                displayCoverageResultState.CleanUp?.Invoke();
                displayCoverageResultState.CancellationTokenSource.Dispose();
            }
        }

        private void DisplayCoverageResult(Task<List<CoverageLine>> coverageTask, object state)
        {
            var displayCoverageResultState = (DisplayCoverageResultState)state;
            switch (coverageTask.Status)
            {
                case TaskStatus.Canceled:
                    CoverageTaskCancelled();
                    break;
                case TaskStatus.Faulted:
                    LogFaulted(coverageTask.Exception);
                    break;
                default:
#pragma warning disable VSTHRD002 // Avoid problematic synchronous waits
                    var coverageLines = coverageTask.Result;
                    SuccessfulCoverage(coverageLines);
#pragma warning restore VSTHRD002 // Avoid problematic synchronous waits
                    break;
            }

            EndOfCoverage(displayCoverageResultState);
        }

        public void RunCancellableCoverageTask(
            Func<CancellationToken,Task<List<CoverageLine>>> reportResultProvider, Action cleanUp)
        {
            var vsLinkedCancellationTokenSource = Reset();
            var vsShutdownLinkedCancellationToken = vsLinkedCancellationTokenSource.Token;

            disposeAwareTaskRunner.RunAsync(() =>
            {
                reloadCoverageTask = Task.Run(async () =>
                {
                    await initializeStatusProvider.WaitForInitializedAsync(vsShutdownLinkedCancellationToken);
                    var result = await reportResultProvider(vsShutdownLinkedCancellationToken);
                    return result;

                }, vsShutdownLinkedCancellationToken)
                .ContinueWith(
                    DisplayCoverageResult, 
                    new DisplayCoverageResultState { 
                        CancellationTokenSource = vsLinkedCancellationTokenSource, 
                        CleanUp = cleanUp
                    }, 
                    TaskScheduler.Default
                );
                return reloadCoverageTask;
            });
        }
    }
}