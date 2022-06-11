namespace FineCodeCoverageTests.Coverlet_Tests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMoq;
    using FineCodeCoverage.Core.Coverlet;
    using FineCodeCoverage.Core.Utilities;
    using FineCodeCoverage.Engine.Coverlet;
    using FineCodeCoverage.Engine.Model;
    using FineCodeCoverage.Options;
    using Moq;
    using NUnit.Framework;

    public class CoverletDataCollectorUtil_RunAsync_Tests
    {
        private AutoMoqer mocker;
        private CoverletDataCollectorUtil coverletDataCollectorUtil;
        private Mock<ICoverageProject> mockCoverageProject;
        private Mock<IRunSettingsCoverletConfiguration> mockRunSettingsCoverletConfiguration;
        private Mock<IDataCollectorSettingsBuilder> mockDataCollectorSettingsBuilder;

        private string tempDirectory;
        [SetUp]
        public void SetUp()
        {
            this.mocker = new AutoMoqer();
            this.mockDataCollectorSettingsBuilder = new Mock<IDataCollectorSettingsBuilder>();
            _ = this.mocker.GetMock<IDataCollectorSettingsBuilderFactory>().Setup(f => f.Create())
                .Returns(this.mockDataCollectorSettingsBuilder.Object);

            this.coverletDataCollectorUtil = this.mocker.Create<CoverletDataCollectorUtil>();

            this.mockCoverageProject = new Mock<ICoverageProject>();
            _ = this.mockCoverageProject.Setup(cp => cp.Settings).Returns(new Mock<IAppOptions>().Object);
            _ = this.mockCoverageProject.Setup(cp => cp.CoverageOutputFolder).Returns("");
            _ = this.mockCoverageProject.Setup(cp => cp.ExcludedReferencedProjects).Returns(new List<string>());
            _ = this.mockCoverageProject.Setup(cp => cp.IncludedReferencedProjects).Returns(new List<string>());
            this.mockRunSettingsCoverletConfiguration = new Mock<IRunSettingsCoverletConfiguration>();
            this.coverletDataCollectorUtil.runSettingsCoverletConfiguration = this.mockRunSettingsCoverletConfiguration.Object;
            this.coverletDataCollectorUtil.coverageProject = this.mockCoverageProject.Object;
        }

        [TearDown]
        public void DeleteTempDirectory()
        {
            if (this.tempDirectory != null && Directory.Exists(this.tempDirectory))
            {
                Directory.Delete(this.tempDirectory);
            }
        }

        private DirectoryInfo CreateTemporaryDirectory()
        {
            this.tempDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            return Directory.CreateDirectory(this.tempDirectory);
        }


        [Test]
#pragma warning disable VSTHRD200 // Use "Async" suffix for async methods
        public async Task Should_Get_Settings_With_TestDllFile()
        {
            _ = this.mockCoverageProject.Setup(cp => cp.TestDllFile).Returns("test.dll");
            _ = this.mockCoverageProject.Setup(cp => cp.CoverageOutputFolder).Returns("");

            await this.coverletDataCollectorUtil.RunAsync(CancellationToken.None);

            this.mockDataCollectorSettingsBuilder.Verify(b => b.WithProjectDll("test.dll"));

        }

        [Test]
        public async Task Should_Get_Settings_With_Exclude_From_CoverageProject_And_RunSettings()
        {
            var projectExclude = new string[] { "excluded" };
            _ = this.mockCoverageProject.Setup(cp => cp.Settings.Exclude).Returns(projectExclude);
            _ = this.mockCoverageProject.Setup(cp => cp.CoverageOutputFolder).Returns("");
            var referencedExcluded = new List<string> { "referencedExcluded" };
            _ = this.mockCoverageProject.Setup(cp => cp.ExcludedReferencedProjects).Returns(referencedExcluded);
            _ = this.mockRunSettingsCoverletConfiguration.Setup(rsc => rsc.Exclude).Returns("rsexclude");

            await this.coverletDataCollectorUtil.RunAsync(CancellationToken.None);

            this.mockDataCollectorSettingsBuilder.Verify(b => b.WithExclude(new string[] { "[referencedExcluded]*", "excluded" }, "rsexclude"));
        }

        [Test]
        public async Task Should_Not_Throw_When_Project_Setttings_Exclude_Is_Null()
        {
            var referencedExcluded = new List<string> { "referencedExcluded" };
            _ = this.mockCoverageProject.Setup(cp => cp.ExcludedReferencedProjects).Returns(referencedExcluded);
            _ = this.mockCoverageProject.Setup(cp => cp.CoverageOutputFolder).Returns("");
            _ = this.mockRunSettingsCoverletConfiguration.Setup(rsc => rsc.Exclude).Returns("rsexclude");

            await this.coverletDataCollectorUtil.RunAsync(CancellationToken.None);

            this.mockDataCollectorSettingsBuilder.Verify(b => b.WithExclude(new string[] { "[referencedExcluded]*" }, "rsexclude"));
        }

        [Test]
        public async Task Should_Get_Settings_With_ExcludeByFile_From_CoverageProject_And_RunSettings()
        {
            var projectExcludeByFile = new string[] { "excludedByFile" };
            _ = this.mockCoverageProject.Setup(cp => cp.Settings.ExcludeByFile).Returns(projectExcludeByFile);
            _ = this.mockCoverageProject.Setup(cp => cp.CoverageOutputFolder).Returns("");

            _ = this.mockRunSettingsCoverletConfiguration.Setup(rsc => rsc.ExcludeByFile).Returns("rsexcludeByFile");

            await this.coverletDataCollectorUtil.RunAsync(CancellationToken.None);

            this.mockDataCollectorSettingsBuilder.Verify(b => b.WithExcludeByFile(projectExcludeByFile, "rsexcludeByFile"));
        }

        [Test]
        public async Task Should_Get_Settings_With_ExcludeByAttribute_From_CoverageProject_And_RunSettings()
        {
            var projectExcludeByAttribute = new string[] { "excludedByAttribute" };
            _ = this.mockCoverageProject.Setup(cp => cp.Settings.ExcludeByAttribute).Returns(projectExcludeByAttribute);
            _ = this.mockRunSettingsCoverletConfiguration.Setup(rsc => rsc.ExcludeByAttribute).Returns("rsexcludeByAttribute");

            await this.coverletDataCollectorUtil.RunAsync(CancellationToken.None);

            this.mockDataCollectorSettingsBuilder.Verify(b => b.WithExcludeByAttribute(projectExcludeByAttribute, "rsexcludeByAttribute"));
        }

        [Test]
        public async Task Should_Get_Settings_With_Include_From_CoverageProject_And_RunSettings()
        {
            var projectInclude = new string[] { "included" };
            _ = this.mockCoverageProject.Setup(cp => cp.Settings.Include).Returns(projectInclude);
            _ = this.mockCoverageProject.Setup(cp => cp.CoverageOutputFolder).Returns("");

            _ = this.mockRunSettingsCoverletConfiguration.Setup(rsc => rsc.Include).Returns("rsincluded");

            await this.coverletDataCollectorUtil.RunAsync(CancellationToken.None);

            this.mockDataCollectorSettingsBuilder.Verify(b => b.WithInclude(projectInclude, "rsincluded"));
        }

        [TestCase(true, "true")]
        [TestCase(false, "false")]
        public async Task Should_Get_Settings_With_IncludeTestAssembly_From_CoverageProject_And_RunSettings(bool projectIncludeTestAssembly, string runSettingsIncludeTestAssembly)
        {
            _ = this.mockCoverageProject.Setup(cp => cp.Settings.IncludeTestAssembly).Returns(projectIncludeTestAssembly);
            _ = this.mockCoverageProject.Setup(cp => cp.CoverageOutputFolder).Returns("");
            _ = this.mockRunSettingsCoverletConfiguration.Setup(rsc => rsc.IncludeTestAssembly).Returns(runSettingsIncludeTestAssembly);

            await this.coverletDataCollectorUtil.RunAsync(CancellationToken.None);

            this.mockDataCollectorSettingsBuilder.Verify(b => b.WithIncludeTestAssembly(projectIncludeTestAssembly, runSettingsIncludeTestAssembly));
        }

        [Test]
        public async Task Should_Initialize_With_Options_And_Run_Settings_First()
        {
            var settings = new Mock<IAppOptions>().Object;
            _ = this.mockCoverageProject.Setup(cp => cp.RunSettingsFile).Returns(".runsettings");
            _ = this.mockCoverageProject.Setup(cp => cp.CoverageOutputFolder).Returns("output");
            _ = this.mockCoverageProject.Setup(cp => cp.Settings).Returns(settings);

            await this.coverletDataCollectorUtil.RunAsync(CancellationToken.None);
            this.mockDataCollectorSettingsBuilder.Verify(b => b.Initialize(settings, ".runsettings", Path.Combine("output", "FCC.runsettings")));

            var invocations = this.mockDataCollectorSettingsBuilder.Invocations.GetEnumerator().ToIEnumerable().ToList();
            Assert.That(invocations.First().Method.Name, Is.EqualTo(nameof(IDataCollectorSettingsBuilder.Initialize)));
        }

        [Test]
        public async Task Should_Get_Settings_With_ResultsDirectory()
        {
            _ = this.mockCoverageProject.Setup(cp => cp.CoverageOutputFolder).Returns("outputfolder");

            await this.coverletDataCollectorUtil.RunAsync(CancellationToken.None);

            this.mockDataCollectorSettingsBuilder.Verify(b => b.WithResultsDirectory("outputfolder"));
        }

        [Test]
        public async Task Should_Get_Settings_With_Blame()
        {
            await this.coverletDataCollectorUtil.RunAsync(CancellationToken.None);

            this.mockDataCollectorSettingsBuilder.Verify(b => b.WithBlame());
        }

        [Test]
        public async Task Should_Get_Settings_With_NoLogo()
        {
            await this.coverletDataCollectorUtil.RunAsync(CancellationToken.None);

            this.mockDataCollectorSettingsBuilder.Verify(b => b.WithNoLogo());
        }

        [Test]
        public async Task Should_Get_Settings_With_Diagnostics()
        {
            _ = this.mockCoverageProject.Setup(cp => cp.CoverageOutputFolder).Returns("outputfolder");

            await this.coverletDataCollectorUtil.RunAsync(CancellationToken.None);

            this.mockDataCollectorSettingsBuilder.Verify(b => b.WithDiagnostics("outputfolder/diagnostics.log"));
        }

        [Test]
        public async Task Should_Get_Settings_With_IncludeDirectory_From_RunSettings()
        {
            _ = this.mockRunSettingsCoverletConfiguration.Setup(rsc => rsc.IncludeDirectory).Returns("includeDirectory");

            await this.coverletDataCollectorUtil.RunAsync(CancellationToken.None);

            this.mockDataCollectorSettingsBuilder.Verify(b => b.WithIncludeDirectory("includeDirectory"));
        }

        [Test]
        public async Task Should_Get_Settings_With_SingleHit_From_RunSettings()
        {
            _ = this.mockRunSettingsCoverletConfiguration.Setup(rsc => rsc.SingleHit).Returns("true...");

            await this.coverletDataCollectorUtil.RunAsync(CancellationToken.None);

            this.mockDataCollectorSettingsBuilder.Verify(b => b.WithSingleHit("true..."));
        }

        [Test]
        public async Task Should_Get_Settings_With_UseSourceLink_From_RunSettings()
        {
            _ = this.mockRunSettingsCoverletConfiguration.Setup(rsc => rsc.UseSourceLink).Returns("true...");

            await this.coverletDataCollectorUtil.RunAsync(CancellationToken.None);

            this.mockDataCollectorSettingsBuilder.Verify(b => b.WithUseSourceLink("true..."));
        }

        [Test]
        public async Task Should_Get_Settings_With_SkipAutoProps_From_RunSettings()
        {
            _ = this.mockRunSettingsCoverletConfiguration.Setup(rsc => rsc.SkipAutoProps).Returns("true...");

            await this.coverletDataCollectorUtil.RunAsync(CancellationToken.None);

            this.mockDataCollectorSettingsBuilder.Verify(b => b.WithSkipAutoProps("true..."));
        }

        [Test]
        public async Task Should_Log_VSTest_Run_With_Settings()
        {
            _ = this.mockCoverageProject.Setup(cp => cp.ProjectName).Returns("TestProject");
            _ = this.mockCoverageProject.Setup(cp => cp.CoverageOutputFolder).Returns("");
            _ = this.mockDataCollectorSettingsBuilder.Setup(sb => sb.Build()).Returns("settings string");

            await this.coverletDataCollectorUtil.RunAsync(CancellationToken.None);

            this.mocker.Verify<ILogger>(l => l.Log(this.coverletDataCollectorUtil.LogRunMessage("settings string")));
        }

        [Test]
        public async Task Should_Execute_DotNet_Test_Collect_XPlat_With_Settings_Using_The_ProcessUtil()
        {
            _ = this.mockCoverageProject.Setup(cp => cp.ProjectOutputFolder).Returns("projectOutputFolder");
            _ = this.mockCoverageProject.Setup(cp => cp.CoverageOutputFolder).Returns("");
            _ = this.mockDataCollectorSettingsBuilder.Setup(sb => sb.Build()).Returns("settings");
            this.coverletDataCollectorUtil.TestAdapterPathArg = "testadapterpath";

            var ct = CancellationToken.None;
            await this.coverletDataCollectorUtil.RunAsync(ct);

            this.mocker.Verify<IProcessUtil>(p => p.ExecuteAsync(It.Is<ExecuteRequest>(er => er.Arguments == @"test --collect:""XPlat Code Coverage"" settings --test-adapter-path testadapterpath" && er.FilePath == "dotnet" && er.WorkingDirectory == "projectOutputFolder"), ct));
        }

        [Test]
        public async Task Should_Use_Custom_TestAdapterPath_Quoted_If_Specified_In_Settings_And_Exists()
        {
            var ct = await this.Use_Custom_TestAdapterPath_Async();

            this.mocker.Verify<IProcessUtil>(p => p.ExecuteAsync(It.Is<ExecuteRequest>(er => er.Arguments == $@"test --collect:""XPlat Code Coverage"" settings --test-adapter-path ""{this.tempDirectory}""" && er.FilePath == "dotnet" && er.WorkingDirectory == "projectOutputFolder"), ct));
        }

        [Test]
        public async Task Should_Log_When_Using_Custom_TestAdapterPath()
        {
            _ = await this.Use_Custom_TestAdapterPath_Async();

            this.mocker.Verify<ILogger>(l => l.Log($"Using custom coverlet data collector : {this.tempDirectory}"));
        }

        [Test]
        public async Task Should_Use_The_ProcessResponseProcessor()
        {
            _ = this.mockCoverageProject.Setup(cp => cp.ProjectName).Returns("TestProject");
            _ = this.mockCoverageProject.Setup(cp => cp.CoverageOutputFolder).Returns("");

            var mockProcesUtil = this.mocker.GetMock<IProcessUtil>();
            var executeResponse = new ExecuteResponse();
            var ct = CancellationToken.None;
            _ = mockProcesUtil.Setup(p => p.ExecuteAsync(It.IsAny<ExecuteRequest>(), ct).Result).Returns(executeResponse);
            var mockProcessResponseProcessor = this.mocker.GetMock<IProcessResponseProcessor>();

            var logTitle = "Coverlet Collector Run (TestProject)";
            _ = mockProcessResponseProcessor.Setup(rp => rp.Process(executeResponse, It.IsAny<Func<int, bool>>(), true, logTitle, It.IsAny<Action>()));

            await this.coverletDataCollectorUtil.RunAsync(ct);

            mockProcessResponseProcessor.VerifyAll();
        }

        [TestCase(2, false)]
        [TestCase(1, true)]
        [TestCase(0, true)]
        public async Task Should_Only_Be_Successful_With_ExitCode_0_Or_1(int exitCode, bool expectedSuccess)
        {
            var mockProcessResponseProcessor = this.mocker.GetMock<IProcessResponseProcessor>();
            Func<int, bool> exitCodePredicate = null;
#pragma warning disable IDE1006 // Naming Styles
            _ = mockProcessResponseProcessor.Setup(rp => rp.Process(
                  It.IsAny<ExecuteResponse>(),
                  It.IsAny<Func<int, bool>>(),
                  true,
                  It.IsAny<string>(),
                  It.IsAny<Action>())
            ).Callback<ExecuteResponse, Func<int, bool>, bool, string, Action>(
                (_, cbExitCodePredicate, __, ___, ____) => exitCodePredicate = cbExitCodePredicate
            );
#pragma warning restore IDE1006 // Naming Styles

            await this.coverletDataCollectorUtil.RunAsync(CancellationToken.None);

            Assert.That(exitCodePredicate(exitCode), Is.EqualTo(expectedSuccess));
        }

        [Test]
        public async Task Should_Correct_The_CoberturaPath_Given_Successful_Execution()
        {
            _ = this.mockCoverageProject.Setup(cp => cp.CoverageOutputFolder).Returns("outputFolder");
            _ = this.mockCoverageProject.Setup(cp => cp.CoverageOutputFile).Returns("outputFile");
            var mockProcessResponseProcessor = this.mocker.GetMock<IProcessResponseProcessor>();
            Action successCallback = null;
#pragma warning disable IDE1006 // Naming Styles
            _ = mockProcessResponseProcessor.Setup(rp => rp.Process(
                  It.IsAny<ExecuteResponse>(),
                  It.IsAny<Func<int, bool>>(),
                  true, It.IsAny<string>(),
                  It.IsAny<Action>())
            ).Callback<ExecuteResponse, Func<int, bool>, bool, string, Action>(
                (_, __, ___, ____, cbSuccessCallback) => successCallback = cbSuccessCallback
            );
#pragma warning restore IDE1006 // Naming Styles

            await this.coverletDataCollectorUtil.RunAsync(CancellationToken.None);
            successCallback();

            this.mocker.Verify<ICoverletDataCollectorGeneratedCobertura>(gc => gc.CorrectPath("outputFolder", "outputFile"));
        }
#pragma warning restore VSTHRD200 // Use "Async" suffix for async methods
        private async Task<CancellationToken> Use_Custom_TestAdapterPath_Async()
        {
            _ = this.CreateTemporaryDirectory();
            _ = this.mockCoverageProject.Setup(cp => cp.ProjectOutputFolder).Returns("projectOutputFolder");
            _ = this.mockCoverageProject.Setup(cp => cp.CoverageOutputFolder).Returns("");
            _ = this.mockCoverageProject.Setup(cp => cp.Settings.CoverletCollectorDirectoryPath).Returns(this.tempDirectory);
            _ = this.mockDataCollectorSettingsBuilder.Setup(sb => sb.Build()).Returns("settings");
            this.coverletDataCollectorUtil.TestAdapterPathArg = "testadapterpath";
            var ct = CancellationToken.None;
            await this.coverletDataCollectorUtil.RunAsync(ct);
            return ct;
        }

    }

    public static class IEnumeratorExtensions
    {
        public static IEnumerable<T> ToIEnumerable<T>(this IEnumerator<T> enumerator)
        {
            while (enumerator.MoveNext())
            {
                yield return enumerator.Current;
            }
        }
    }

}
