namespace FineCodeCoverageTests.MsCodeCoverage_Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMoq;
    using FineCodeCoverage.Engine;
    using FineCodeCoverage.Engine.Model;
    using FineCodeCoverage.Engine.MsTestPlatform.CodeCoverage;
    using FineCodeCoverage.Impl;
    using FineCodeCoverage.Options;
    using Moq;
    using NUnit.Framework;

    internal class MsCodeCoverageRunSettingsService_Test_Execution_Not_Finished_Tests
    {
        [Test]
        public async Task Should_Clean_Up_RunSettings_Coverage_Projects_Async()
        {
            var autoMocker = new AutoMoqer();
            var msCodeCoverageRunSettingsService = autoMocker.Create<MsCodeCoverageRunSettingsService>();
            msCodeCoverageRunSettingsService.threadHelper = new TestThreadHelper();

            var mockUserRunSettingsService = autoMocker.GetMock<IUserRunSettingsService>();
            _ = mockUserRunSettingsService.Setup(
                userRunSettingsService =>
                userRunSettingsService.Analyse(It.IsAny<IEnumerable<ICoverageProject>>(), It.IsAny<bool>(), It.IsAny<string>())
            ).Returns(new UserRunSettingsAnalysisResult());

            var mockAppOptionsProvider = autoMocker.GetMock<IAppOptionsProvider>();
            _ = mockAppOptionsProvider.Setup(appOptionsProvider => appOptionsProvider.Provide()).Returns(new Mock<IAppOptions>().Object);

            // is collecting
            var mockTestOperation = new Mock<ITestOperation>();
            var runSettingsCoverageProject = this.CreateCoverageProject(".runsettings");
            var coverageProjects = new List<ICoverageProject>
            {
                runSettingsCoverageProject,
                this.CreateCoverageProject(null)

            };
            _ = mockTestOperation.Setup(testOperation => testOperation.GetCoverageProjectsAsync()).ReturnsAsync(coverageProjects);

            _ = await msCodeCoverageRunSettingsService.IsCollectingAsync(mockTestOperation.Object);

            await msCodeCoverageRunSettingsService.TestExecutionNotFinishedAsync(mockTestOperation.Object);

            autoMocker.Verify<ITemplatedRunSettingsService>(
                templatedRunSettingsService => templatedRunSettingsService.CleanUpAsync(new List<ICoverageProject> { runSettingsCoverageProject })
            );
        }

        private ICoverageProject CreateCoverageProject(string runSettingsFile)
        {
            var mockCoverageProject = new Mock<ICoverageProject>();
            _ = mockCoverageProject.Setup(coverageProject => coverageProject.RunSettingsFile).Returns(runSettingsFile);
            return mockCoverageProject.Object;
        }
    }

    internal class MsCodeCoverageRunSettingsService_Collect_Tests
    {
        private AutoMoqer autoMocker;
        private ICoverageProject runSettingsCoverageProject;

        [Test]
        public async Task Should_FCCEngine_RunAndProcessReport_With_CoberturaResults_Async()
        {
            var resultsUris = new List<Uri>()
            {
                new Uri(@"C:\SomePath\result1.cobertura.xml", UriKind.Absolute),
                new Uri(@"C:\SomePath\result2.cobertura.xml", UriKind.Absolute),
                new Uri(@"C:\SomePath\result3.xml", UriKind.Absolute),
            };

            var expectedCoberturaFiles = new string[] { @"C:\SomePath\result1.cobertura.xml", @"C:\SomePath\result2.cobertura.xml" };
            await this.RunAndProcessReportAsync(resultsUris, expectedCoberturaFiles);
        }

        [Test]
        public Task Should_Not_Throw_If_No_Results_Async() => this.RunAndProcessReportAsync(null, Array.Empty<string>());

        [Test]
        public async Task Should_Clean_Up_RunSettings_Coverage_Projects_From_IsCollecting_Async()
        {
            await this.RunAndProcessReportAsync(null, Array.Empty<string>());
            this.autoMocker.Verify<ITemplatedRunSettingsService>(
                templatedRunSettingsService => templatedRunSettingsService.CleanUpAsync(
                    new List<ICoverageProject> { this.runSettingsCoverageProject }
                )
            );
        }

        private async Task RunAndProcessReportAsync(IEnumerable<Uri> resultsUris, string[] expectedCoberturaFiles)
        {
            this.autoMocker = new AutoMoqer();
            var mockFccEngine = this.autoMocker.GetMock<IFCCEngine>();
            var msCodeCoverageRunSettingsService = this.autoMocker.Create<MsCodeCoverageRunSettingsService>();
            msCodeCoverageRunSettingsService.threadHelper = new TestThreadHelper();

            var vsLinkedCancellationToken = CancellationToken.None;
            var coverageLines = new List<CoverageLine>();
            Func<CancellationToken, Task<List<CoverageLine>>> resultProvider = null;
            _ = mockFccEngine.Setup(
                fccEngine => fccEngine.RunCancellableCoverageTask(
                    It.IsAny<Func<CancellationToken, Task<List<CoverageLine>>>>(), null
                )
            ).Callback<Func<CancellationToken, Task<List<CoverageLine>>>, Action>((rp, _) => resultProvider = rp);
            _ = mockFccEngine.Setup(
                engine => engine.RunAndProcessReport(
                    It.Is<string[]>(coberturaFiles =>
                        !expectedCoberturaFiles.Except(coberturaFiles).Any() &&
                        !coberturaFiles.Except(expectedCoberturaFiles).Any()
                    ), vsLinkedCancellationToken
                )
            ).Returns(coverageLines);

            // IsCollecting
            var mockTestOperation = new Mock<ITestOperation>();
            _ = mockTestOperation.Setup(testOperation => testOperation.GetRunSettingsDataCollectorResultUri(new Uri(RunSettingsHelper.MsDataCollectorUri))).Returns(resultsUris);
            this.runSettingsCoverageProject = this.CreateCoverageProject(".runsettings");
            var coverageProjects = new List<ICoverageProject>
            {
                this.CreateCoverageProject(null),
                this.runSettingsCoverageProject
            };
            _ = mockTestOperation.Setup(testOperation => testOperation.GetCoverageProjectsAsync()).ReturnsAsync(coverageProjects);

            var mockAppOptionsProvider = this.autoMocker.GetMock<IAppOptionsProvider>();
            _ = mockAppOptionsProvider.Setup(appOptionsProvider => appOptionsProvider.Provide())
                .Returns(new Mock<IAppOptions>().Object);
            _ = await msCodeCoverageRunSettingsService.IsCollectingAsync(mockTestOperation.Object);

            await msCodeCoverageRunSettingsService.CollectAsync(mockTestOperation.Object);
            var processedCoverageLines = await resultProvider(vsLinkedCancellationToken);

            Assert.That(processedCoverageLines, Is.SameAs(coverageLines));
        }

        private ICoverageProject CreateCoverageProject(string runSettingsFile)
        {
            var mockCoverageProject = new Mock<ICoverageProject>();
            _ = mockCoverageProject.Setup(coverageProject => coverageProject.RunSettingsFile).Returns(runSettingsFile);
            return mockCoverageProject.Object;
        }
    }
}
