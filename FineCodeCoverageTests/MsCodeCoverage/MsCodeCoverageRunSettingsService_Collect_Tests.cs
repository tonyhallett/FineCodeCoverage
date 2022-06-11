﻿using Moq;
using NUnit.Framework;
using Microsoft.VisualStudio.TestWindow.Extensibility;
using System;
using System.Collections.Generic;
using System.Linq;
using AutoMoq;
using System.Threading;
using FineCodeCoverage.Core.Utilities;
using FineCodeCoverage.Impl;
using FineCodeCoverage.Engine;
using System.Threading.Tasks;
using FineCodeCoverage.Engine.MsTestPlatform.CodeCoverage;
using FineCodeCoverage.Engine.Model;
using FineCodeCoverage.Options;

namespace FineCodeCoverageTests.MsCodeCoverage_Tests
{
    internal class MsCodeCoverageRunSettingsService_Test_Execution_Not_Finished_Tests
    {
        [Test]
        public async Task Should_Clean_Up_RunSettings_Coverage_Projects()
        {
            var autoMocker = new AutoMoqer();
            var msCodeCoverageRunSettingsService = autoMocker.Create<MsCodeCoverageRunSettingsService>();
            msCodeCoverageRunSettingsService.threadHelper = new TestThreadHelper();

            var mockUserRunSettingsService = autoMocker.GetMock<IUserRunSettingsService>();
            mockUserRunSettingsService.Setup(
                userRunSettingsService =>
                userRunSettingsService.Analyse(It.IsAny<IEnumerable<ICoverageProject>>(), It.IsAny<bool>(), It.IsAny<string>())
            ).Returns(new UserRunSettingsAnalysisResult());

            var mockAppOptionsProvider = autoMocker.GetMock<IAppOptionsProvider>();
            mockAppOptionsProvider.Setup(appOptionsProvider => appOptionsProvider.Get()).Returns(new Mock<IAppOptions>().Object);

            // is collecting
            var mockTestOperation = new Mock<ITestOperation>();
            var runSettingsCoverageProject = CreateCoverageProject(".runsettings");
            var coverageProjects = new List<ICoverageProject>
            {
                runSettingsCoverageProject,
                CreateCoverageProject(null)
                
            };
            mockTestOperation.Setup(testOperation => testOperation.GetCoverageProjectsAsync()).ReturnsAsync(coverageProjects);
            
            await msCodeCoverageRunSettingsService.IsCollectingAsync(mockTestOperation.Object);

            await msCodeCoverageRunSettingsService.TestExecutionNotFinishedAsync(mockTestOperation.Object);

            autoMocker.Verify<ITemplatedRunSettingsService>(
                templatedRunSettingsService => templatedRunSettingsService.CleanUpAsync(new List<ICoverageProject> { runSettingsCoverageProject })
            );
        }

        private ICoverageProject CreateCoverageProject(string runSettingsFile)
        {
            var mockCoverageProject = new Mock<ICoverageProject>();
            mockCoverageProject.Setup(coverageProject => coverageProject.RunSettingsFile).Returns(runSettingsFile);
            return mockCoverageProject.Object;
        }
    }

    internal class MsCodeCoverageRunSettingsService_Collect_Tests
    {
        private AutoMoqer autoMocker;
        private ICoverageProject runSettingsCoverageProject;

        [Test]
        public async Task Should_FCCEngine_RunAndProcessReport_With_CoberturaResults()
        {
            var resultsUris = new List<Uri>()
            {
                new Uri(@"C:\SomePath\result1.cobertura.xml", UriKind.Absolute),
                new Uri(@"C:\SomePath\result2.cobertura.xml", UriKind.Absolute),
                new Uri(@"C:\SomePath\result3.xml", UriKind.Absolute),
            };

            var expectedCoberturaFiles = new string[] { @"C:\SomePath\result1.cobertura.xml", @"C:\SomePath\result2.cobertura.xml" };
            await RunAndProcessReportAsync(resultsUris, expectedCoberturaFiles);
        }

        [Test]
        public async Task Should_Not_Throw_If_No_Results()
        {
            await RunAndProcessReportAsync(null, Array.Empty<string>());
        }

        [Test]
        public async Task Should_Clean_Up_RunSettings_Coverage_Projects_From_IsCollecting()
        {
            await RunAndProcessReportAsync(null, Array.Empty<string>());
            autoMocker.Verify<ITemplatedRunSettingsService>(
                templatedRunSettingsService => templatedRunSettingsService.CleanUpAsync(new List<ICoverageProject> { runSettingsCoverageProject })
            );
        }

        private async Task RunAndProcessReportAsync(IEnumerable<Uri> resultsUris,string[] expectedCoberturaFiles)
        {
            autoMocker = new AutoMoqer();
            var mockToolFolder = autoMocker.GetMock<IToolFolder>();
            mockToolFolder.Setup(tf => tf.EnsureUnzipped(
                It.IsAny<string>(), 
                It.IsAny<string>(), 
                It.IsAny<ZipDetails>(), 
                It.IsAny<CancellationToken>()
            )).Returns("ZipDestination");
            
            var msCodeCoverageRunSettingsService = autoMocker.Create<MsCodeCoverageRunSettingsService>();
            msCodeCoverageRunSettingsService.threadHelper = new TestThreadHelper();

            var mockFccEngine = new Mock<IFCCEngine>();
            var vsLinkedCancellationToken = CancellationToken.None;
            var coverageLines = new List<CoverageLine>();
            Func<CancellationToken, Task<List<CoverageLine>>> resultProvider = null;
            mockFccEngine.Setup(
                fccEngine => fccEngine.RunCancellableCoverageTask(
                    It.IsAny<Func<CancellationToken, Task<List<CoverageLine>>>>(), null
                )
            ).Callback<Func<CancellationToken, Task<List<CoverageLine>>>, Action>((_resultProvider, a) =>
              {
                  resultProvider = _resultProvider;
              });
            mockFccEngine.Setup(
                engine => engine.RunAndProcessReport(
                    It.Is<string[]>(coberturaFiles => 
                        !expectedCoberturaFiles.Except(coberturaFiles).Any() && 
                        !coberturaFiles.Except(expectedCoberturaFiles).Any()
                    ), vsLinkedCancellationToken
                )
            ).Returns(coverageLines);

            msCodeCoverageRunSettingsService.Initialize("", mockFccEngine.Object, CancellationToken.None);

            var mockOperation = new Mock<IOperation>();
            mockOperation.Setup(operation => operation.GetRunSettingsDataCollectorResultUri(new Uri(RunSettingsHelper.MsDataCollectorUri))).Returns(resultsUris);
            

            // IsCollecting
            var mockTestOperation = new Mock<ITestOperation>();
            runSettingsCoverageProject = CreateCoverageProject(".runsettings");
            var coverageProjects = new List<ICoverageProject>
            {
                CreateCoverageProject(null),
                runSettingsCoverageProject
            };
            mockTestOperation.Setup(testOperation => testOperation.GetCoverageProjectsAsync()).ReturnsAsync(coverageProjects);

            var mockAppOptionsProvider = autoMocker.GetMock<IAppOptionsProvider>();
            mockAppOptionsProvider.Setup(appOptionsProvider => appOptionsProvider.Get()).Returns(new Mock<IAppOptions>().Object);
            await msCodeCoverageRunSettingsService.IsCollectingAsync(mockTestOperation.Object);

            await msCodeCoverageRunSettingsService.CollectAsync(mockOperation.Object, mockTestOperation.Object);
            var processedCoverageLines = await resultProvider(vsLinkedCancellationToken);
            Assert.AreSame(coverageLines, processedCoverageLines);
        }

        private ICoverageProject CreateCoverageProject(string runSettingsFile)
        {
            var mockCoverageProject = new Mock<ICoverageProject>();
            mockCoverageProject.Setup(coverageProject => coverageProject.RunSettingsFile).Returns(runSettingsFile);
            return mockCoverageProject.Object;
        }
    }
}
