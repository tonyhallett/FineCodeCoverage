using FineCodeCoverage.Core.Utilities;
using FineCodeCoverage.Engine.Model;
using FineCodeCoverage.Impl;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Threading;
using Task = System.Threading.Tasks.Task;
using Microsoft.VisualStudio.TestWindow.Extensibility;
using System.Xml.XPath;
using FineCodeCoverage.Options;
using System.Threading.Tasks;
using FineCodeCoverage.Core.Utilities.VsThreading;
using FineCodeCoverage.Output.HostObjects;
using FineCodeCoverage.Output.JsMessages.Logging;
using FineCodeCoverage.Core.ReportGenerator;
using FineCodeCoverage.Core;
using ILogger = FineCodeCoverage.Logging.ILogger;

namespace FineCodeCoverage.Engine.MsTestPlatform.CodeCoverage
{
    [Export(typeof(IMsCodeCoverageRunSettingsService))]
    [Export(typeof(IRunSettingsService))]
    internal class MsCodeCoverageRunSettingsService : IMsCodeCoverageRunSettingsService, IRunSettingsService, ICoverageService
    {
        public string Name => "Fine Code Coverage MsCodeCoverageRunSettingsService";

        private class UserRunSettingsProjectDetails : IUserRunSettingsProjectDetails
        {
            public IMsCodeCoverageOptions Settings { get; set; }
            public string CoverageOutputFolder { get; set; }
            public string TestDllFile { get; set; }
            public List<string> ExcludedReferencedProjects { get; set; }
            public List<string> IncludedReferencedProjects { get; set; }
        }
        private class CoverageProjectsByType
        {
            public List<ICoverageProject> All { get; private set; }
            public List<ICoverageProject> RunSettings { get; private set; }
            public List<ICoverageProject> Templated { get; private set; }

            public bool HasTemplated()
            {
                return Templated.Any();
            }

            public static async Task<CoverageProjectsByType> CreateAsync(ITestOperation testOperation)
            {
                var coverageProjects = await testOperation.GetCoverageProjectsAsync();
                var coverageProjectsWithRunSettings = coverageProjects.Where(coverageProject => coverageProject.RunSettingsFile != null).ToList();
                var coverageProjectsWithoutRunSettings = coverageProjects.Except(coverageProjectsWithRunSettings).ToList();
                return new CoverageProjectsByType
                {
                    All = coverageProjects,
                    RunSettings = coverageProjectsWithRunSettings,
                    Templated = coverageProjectsWithoutRunSettings
                };
            }
        }

        private readonly IToolFolder toolFolder;
        private readonly IToolZipProvider toolZipProvider;
        private readonly IAppOptionsProvider appOptionsProvider;
        private readonly ICoverageToolOutputManager coverageOutputManager;
        private readonly IShimCopier shimCopier;
        private readonly ILogger logger;
        private readonly IReportGeneratorUtil reportGeneratorUtil;
        private readonly IEventAggregator eventAggregator;
        private IFCCEngine fccEngine;

        private const string zipPrefix = "microsoft.codecoverage";
        private const string zipDirectoryName = "msCodeCoverage";

        private const string msCodeCoverage = "Ms code coverage";
        internal Dictionary<string, IUserRunSettingsProjectDetails> userRunSettingsProjectDetailsLookup; // for tests

        private readonly IUserRunSettingsService userRunSettingsService;
        private readonly ITemplatedRunSettingsService templatedRunSettingsService;
        private string fccMsTestAdapterPath;
        private string shimPath;

        private CoverageProjectsByType coverageProjectsByType;
        private bool useMsCodeCoverage;
        
        internal MsCodeCoverageCollectionStatus collectionStatus; // for tests
        private RunMsCodeCoverage runMsCodeCoverage;
        private bool collectingWithUserRunSettings;
        private DateTime collectionStartTime;

        private bool IsCollecting => collectionStatus == MsCodeCoverageCollectionStatus.Collecting;

        internal IThreadHelper threadHelper = new VsThreadHelper();

        [ImportingConstructor]
        public MsCodeCoverageRunSettingsService(
            IToolFolder toolFolder, 
            IToolZipProvider toolZipProvider, 
            IAppOptionsProvider appOptionsProvider,
            ICoverageToolOutputManager coverageOutputManager,
            IUserRunSettingsService userRunSettingsService,
            ITemplatedRunSettingsService templatedRunSettingsService,
            IShimCopier shimCopier,
            ILogger logger,
            IReportGeneratorUtil reportGeneratorUtil,
            IEventAggregator eventAggregator
            )
        {
            this.toolFolder = toolFolder;
            this.toolZipProvider = toolZipProvider;
            this.appOptionsProvider = appOptionsProvider;
            this.coverageOutputManager = coverageOutputManager;
            this.shimCopier = shimCopier;
            this.logger = logger;
            this.reportGeneratorUtil = reportGeneratorUtil;
            this.eventAggregator = eventAggregator;
            this.userRunSettingsService = userRunSettingsService;
            this.templatedRunSettingsService = templatedRunSettingsService;
        }

        public void Initialize(string appDataFolder, IFCCEngine fccEngine, CancellationToken cancellationToken)
        {
            this.fccEngine = fccEngine;
            var zipDestination = toolFolder.EnsureUnzipped(appDataFolder, zipDirectoryName, toolZipProvider.ProvideZip(zipPrefix), cancellationToken);
            fccMsTestAdapterPath = Path.Combine(zipDestination, "build", "netstandard1.0");
            shimPath = Path.Combine(zipDestination, "build", "netstandard1.0", "CodeCoverage", "coreclr", "Microsoft.VisualStudio.CodeCoverage.Shim.dll");
        }
        
        #region set up for collection
       
        public async Task<MsCodeCoverageCollectionStatus> IsCollectingAsync(ITestOperation testOperation)
        {
            collectionStartTime = DateTime.Now;
            await InitializeIsCollectingAsync(testOperation);
            if( runMsCodeCoverage == RunMsCodeCoverage.No)
            {
                await CombinedLogActionAsync(() =>
                {
                    logger.Log($"See option {nameof(IAppOptions.RunMsCodeCoverage)} for a better ( Beta ) experience.  {FCCGithub.Readme}");
                    eventAggregator.SendMessage(new LogMessage
                    {
                        context = MessageContext.Info,
                        message = new LogMessagePart[] {
                        new Emphasized("See option "),
                        new Emphasized(nameof(IAppOptions.RunMsCodeCoverage),Emphasis.Italic),
                        new Emphasized(" for a better ( Beta ) experience. "),
                        new FCCLink
                        {
                            hostObject = FCCResourcesNavigatorRegistration.HostObjectName,
                            methodName = nameof(FCCResourcesNavigatorHostObject.readReadMe),
                            title = "view readme",
                            ariaLabel = "view readme",
                            arguments = new object[]{"Some test arg" }
                        }
                    }
                    });
                });
            }
            else
            {
                await TrySetUpForCollectionAsync(testOperation.SolutionDirectory);
            }
            
            await ReportCoveringStatusAsync();
            return collectionStatus;
        }

        private Task InitializeIsCollectingAsync(ITestOperation testOperation)
        {
            collectingWithUserRunSettings = false;
            collectionStatus = MsCodeCoverageCollectionStatus.NotCollecting;
            runMsCodeCoverage = appOptionsProvider.Provide().RunMsCodeCoverage;
            useMsCodeCoverage = runMsCodeCoverage == RunMsCodeCoverage.Yes;
            userRunSettingsProjectDetailsLookup = null;
            return CleanUpAsync(testOperation);
        }

        #region TrySetUpForCollectionAsync
        private async Task TrySetUpForCollectionAsync(string solutionDirectory)
        {
            IUserRunSettingsAnalysisResult analysisResult = await TryAnalyseUserRunSettingsAsync();
            if (analysisResult.Ok())
            {
                await SetUpForCollectionAsync(
                    analysisResult.ProjectsWithFCCMsTestAdapter,
                    analysisResult.SpecifiedMsCodeCoverage,
                    solutionDirectory
                );
            }
        }

        #region TryAnalyseUserRunSettingsAsync
        private async Task<IUserRunSettingsAnalysisResult> TryAnalyseUserRunSettingsAsync()
        {
            IUserRunSettingsAnalysisResult analysisResult = null;
            try
            {
                analysisResult = AnalyseUserRunSettings();
            }
            catch (Exception exc)
            {
                await ExceptionAnalysingUserRunSettingsAsync(exc);
            }

            return analysisResult;
        }

        private IUserRunSettingsAnalysisResult AnalyseUserRunSettings()
        {
            var analysisResult = userRunSettingsService.Analyse(
                    coverageProjectsByType.RunSettings,
                    useMsCodeCoverage,
                    fccMsTestAdapterPath
                );

            if (analysisResult.Suitable)
            {
                CollectingIfUserRunSettingsOnly();
            }

            return analysisResult;
        }

        private void CollectingIfUserRunSettingsOnly()
        {
            if (!coverageProjectsByType.HasTemplated())
            {
                collectionStatus = MsCodeCoverageCollectionStatus.Collecting;
                collectingWithUserRunSettings = true;
            }
        }

        private Task ExceptionAnalysingUserRunSettingsAsync(Exception exc)
        {
            collectionStatus = MsCodeCoverageCollectionStatus.Error;
            return CombinedLogExceptionAsync(exc, "Exception analysing runsettings files");
        }
        #endregion

        #region SetUpForCollectionAsync
        private async Task SetUpForCollectionAsync(
            List<ICoverageProject> coverageProjectsForShim, 
            bool specifiedMsCodeCoverageInRunSettings,
            string solutionDirectory
        )
        {
            await PrepareCoverageProjectsAsync();
            SetUserRunSettingsProjectDetails();
            
            await GenerateTemplatedRunSettingsIfRequiredAsync(
                specifiedMsCodeCoverageInRunSettings,
                coverageProjectsForShim,
                solutionDirectory
            );
            CopyShimWhenCollecting(coverageProjectsForShim);
        }

        private async Task PrepareCoverageProjectsAsync()
        {
            coverageOutputManager.SetProjectCoverageOutputFolder(coverageProjectsByType.All);
            foreach (var coverageProject in coverageProjectsByType.All)
            {
                await coverageProject.PrepareForCoverageAsync(CancellationToken.None, false);
            }
        }

        private void SetUserRunSettingsProjectDetails()
        {
            userRunSettingsProjectDetailsLookup = new Dictionary<string, IUserRunSettingsProjectDetails>();
            foreach (var coverageProjectWithRunSettings in coverageProjectsByType.RunSettings)
            {
                var userRunSettingsProjectDetails = new UserRunSettingsProjectDetails
                {
                    Settings = coverageProjectWithRunSettings.Settings,
                    CoverageOutputFolder = coverageProjectWithRunSettings.CoverageOutputFolder,
                    TestDllFile = coverageProjectWithRunSettings.TestDllFile,
                    ExcludedReferencedProjects = coverageProjectWithRunSettings.ExcludedReferencedProjects,
                    IncludedReferencedProjects = coverageProjectWithRunSettings.IncludedReferencedProjects
                };
                userRunSettingsProjectDetailsLookup.Add(coverageProjectWithRunSettings.TestDllFile, userRunSettingsProjectDetails);
            }
        }

        #region GenerateTemplatedRunSettingsIfRequiredAsync
        private async Task GenerateTemplatedRunSettingsIfRequiredAsync(
            bool runSettingsSpecifiedMsCodeCoverage,
            List<ICoverageProject> coverageProjectsForShim,
            string solutionDirectory
        )
        {
            if (ShouldGenerateTemplatedRunSettings(runSettingsSpecifiedMsCodeCoverage))
            {
                await GenerateTemplatedRunSettingsAsync(coverageProjectsForShim, solutionDirectory);
            }
        }

        private bool ShouldGenerateTemplatedRunSettings(bool runSettingsSpecifiedMsCodeCoverage)
        {
            return coverageProjectsByType.HasTemplated() && (useMsCodeCoverage || runSettingsSpecifiedMsCodeCoverage);
        }

        private async Task GenerateTemplatedRunSettingsAsync(
            List<ICoverageProject> coverageProjectsForShim,
            string solutionDirectory
        )
        {
            var generationResult = await templatedRunSettingsService.GenerateAsync(
                coverageProjectsByType.Templated,
                solutionDirectory,
                fccMsTestAdapterPath
            );

            await ProcessTemplateGenerationResultAsync(generationResult, coverageProjectsForShim);
        }

        private async Task ProcessTemplateGenerationResultAsync(IProjectRunSettingsFromTemplateResult generationResult, List<ICoverageProject> coverageProjectsForShim)
        {
            if (generationResult.ExceptionReason == null)
            {
                CollectingWithTemplate(generationResult, coverageProjectsForShim);
            }
            else
            {
                var exceptionReason = generationResult.ExceptionReason;
                await CombinedLogExceptionAsync(exceptionReason.Exception, exceptionReason.Reason);
                collectionStatus = MsCodeCoverageCollectionStatus.Error;
            }
        }

        private void CollectingWithTemplate(IProjectRunSettingsFromTemplateResult generationResult, List<ICoverageProject> coverageProjectsForShim)
        {
            coverageProjectsForShim.AddRange(generationResult.CoverageProjectsWithFCCMsTestAdapter);
            if (generationResult.CustomTemplatePaths.Any())
            {
                var loggerMessages = new List<string> { $"{msCodeCoverage} - custom template paths" }.Concat(generationResult.CustomTemplatePaths.Distinct());
                logger.Log(loggerMessages);
            }

            collectionStatus = MsCodeCoverageCollectionStatus.Collecting;
        }
        #endregion
        private void CopyShimWhenCollecting(List<ICoverageProject> coverageProjectsForShim)
        {
            if (IsCollecting)
            {
                shimCopier.Copy(shimPath, coverageProjectsForShim);
            }
        }
        #endregion
        #endregion

        private async Task ReportCoveringStatusAsync()
        {
            if (collectionStatus == MsCodeCoverageCollectionStatus.Collecting)
            {
                await CombinedLogAsync($"Starting coverage", MessageContext.CoverageStart);
                var withUserRunSettingsMessage = collectingWithUserRunSettings ? " with user runsettings" : "";
                await CombinedLogAsync($"{msCodeCoverage} collecting{withUserRunSettingsMessage}", MessageContext.CoverageToolStart);
            }
        }

        

        

        
        
        #endregion

        #region IRunSettingsService
        public IXPathNavigable AddRunSettings(IXPathNavigable inputRunSettingDocument, IRunSettingsConfigurationInfo configurationInfo, Microsoft.VisualStudio.TestWindow.Extensibility.ILogger log)
        {
            if (configurationInfo.IsTestExecution() && ShouldAddFCCRunSettings())
            {
                return userRunSettingsService.AddFCCRunSettings(inputRunSettingDocument, configurationInfo, userRunSettingsProjectDetailsLookup, fccMsTestAdapterPath);
            }
            return null;
        }

        private bool ShouldAddFCCRunSettings()
        {
            return IsCollecting && userRunSettingsProjectDetailsLookup != null && userRunSettingsProjectDetailsLookup.Count > 0;
        }

        #endregion

        #region CollectAsync
        public async Task CollectAsync(IOperation operation, ITestOperation testOperation)
        {
            await LogCoverageToolCompletedAsync();
            await CleanUpAsync(testOperation);

            var coberturaFiles = GetCoberturaFiles(operation);
            fccEngine.RunCancellableCoverageTask((vsShutdownLinkedCancellationToken) =>
            {
                var coverageLines = fccEngine.RunAndProcessReport(coberturaFiles, vsShutdownLinkedCancellationToken);
                return Task.FromResult(coverageLines);
            }, null);
        }

        private Task LogCoverageToolCompletedAsync()
        {
            var duration = DateTime.Now - collectionStartTime;
            var completedMessage = $"{msCodeCoverage} completed - duration {duration.ToStringHoursMinutesSeconds()}";

            return CombinedLogAsync(completedMessage, MessageContext.CoverageToolCompleted);

        }

        private string[] GetCoberturaFiles(IOperation operation)
        {
            var resultsUris = operation.GetRunSettingsMsDataCollectorResultUri();
            var coberturaFiles = new string[0];
            if (resultsUris != null)
            {
                coberturaFiles = resultsUris.Select(uri => uri.LocalPath).Where(f => f.EndsWith(".cobertura.xml")).ToArray();
            }
            return coberturaFiles;
        }
        #endregion

        public void StopCoverage()
        {
            fccEngine.StopCoverage();
        }

        
        public Task TestExecutionNotFinishedAsync(ITestOperation testOperation)
        {
            return CleanUpAsync(testOperation);
        }

        private async Task CleanUpAsync(ITestOperation testOperation)
        {
            coverageProjectsByType = await CoverageProjectsByType.CreateAsync(testOperation);
            await templatedRunSettingsService.CleanUpAsync(coverageProjectsByType.RunSettings);
        }

        #region Logging
        private async Task CombinedLogAsync(string message, MessageContext messageContext)
        {
            await CombinedLogActionAsync(() =>
            {
                logger.Log(message);
                eventAggregator.SendMessage(LogMessage.Simple(messageContext, message));
            });
        }

        private async Task CombinedLogActionAsync(Action action)
        {
            await threadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            action();
        }

        private Task CombinedLogExceptionAsync(Exception ex, string reason)
        {
            return CombinedLogActionAsync(() =>
            {
                logger.Log(reason, ex.ToString());
                eventAggregator.SendMessage(LogMessage.Simple(MessageContext.Error, reason));
            });
        }

        #endregion

    }

    public static class IRunSettingsConfigurationInfoExtensions { 
        public static bool IsTestExecution(this IRunSettingsConfigurationInfo configurationInfo)
        {
            return configurationInfo.RequestState == RunSettingConfigurationInfoState.Execution;
        }
        
    }

    internal static class UserRunSettingsAnalysisResultExtensions
    {
        public static bool Ok(this IUserRunSettingsAnalysisResult userRunSettingsAnalysisResult)
        {
            if (userRunSettingsAnalysisResult == null)
            {
                return false;
            }
            return userRunSettingsAnalysisResult.Suitable;

        }
    }

}
