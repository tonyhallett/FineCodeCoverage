namespace FineCodeCoverageTests.Coverlet_Tests
{
    using System.Collections.Generic;
    using System.IO;
    using System.Threading;
    using AutoMoq;
    using FineCodeCoverage.Logging;
    using FineCodeCoverage.Core.Utilities;
    using FineCodeCoverage.Engine.Coverlet;
    using FineCodeCoverage.Engine.Model;
    using Moq;
    using NUnit.Framework;

    public class CoverletConsoleUtil_Tests
    {
        private AutoMoqer mocker;
        private CoverletConsoleUtil coverletConsoleUtil;

        [SetUp]
        public void SetUp()
        {
            this.mocker = new AutoMoqer();
            this.coverletConsoleUtil = this.mocker.Create<CoverletConsoleUtil>();
        }

        [Test]
        public void Should_Initilize_IFCCCoverletConsoleExeProvider()
        {
            var ct = CancellationToken.None;
            this.coverletConsoleUtil.Initialize("appDataFolder", ct);
            this.mocker.Verify<IFCCCoverletConsoleExecutor>(fccExeProvider => fccExeProvider.Initialize("appDataFolder", ct));
        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        public void Should_GetCoverletExePath_From_First_That_Returns_Non_Null(int providingExeProvider)
        {
            var coverageProject = new Mock<ICoverageProject>().Object;
            var coverletSettings = "coverlet settings";

            var executeRequests = new List<ExecuteRequest>
            {
                new ExecuteRequest(),
                new ExecuteRequest(),
                new ExecuteRequest(),
                new ExecuteRequest()
            };

            ExecuteRequest GetExecuteRequest(int order)
            {
                if (order != providingExeProvider)
                {
                    return null;
                }
                return executeRequests[order];
            };

            var mockLocalExecutor = new Mock<ICoverletConsoleExecutor>();
            var mockCustomPathExecutor = new Mock<ICoverletConsoleExecutor>();
            var mockGlobalExecutor = new Mock<ICoverletConsoleExecutor>();
            var mockFCCCoverletConsoleExecutor = new Mock<IFCCCoverletConsoleExecutor>();
            var mockFCCExecutor = mockFCCCoverletConsoleExecutor.As<ICoverletConsoleExecutor>();
            var mockExecutors = new List<Mock<ICoverletConsoleExecutor>>
            {
                mockLocalExecutor,
                mockCustomPathExecutor,
                mockGlobalExecutor,
                mockFCCExecutor
            };
            var callOrder = new List<int>();
            for (var i = 0; i < mockExecutors.Count; i++)
            {
                var order = i;
                var mockExeProvider = mockExecutors[i];
                _ = mockExeProvider.Setup(p => p.GetRequest(coverageProject, coverletSettings))
                    .Returns(GetExecuteRequest(i)).Callback<ICoverageProject, string>((cp, s) => callOrder.Add(order));
            }

            var coverletConsoleUtil = new CoverletConsoleUtil(null, null, mockGlobalExecutor.Object, mockCustomPathExecutor.Object, mockLocalExecutor.Object, mockFCCCoverletConsoleExecutor.Object);

            var executeRequest = coverletConsoleUtil.GetExecuteRequest(coverageProject, coverletSettings);
            Assert.Multiple(() =>
            {
                Assert.That(executeRequest, Is.SameAs(GetExecuteRequest(providingExeProvider)));
                Assert.That(callOrder, Has.Count.EqualTo(providingExeProvider + 1));

                var previousCallOrder = -1;
                foreach (var call in callOrder)
                {
                    Assert.That(call - previousCallOrder, Is.EqualTo(1));
                    previousCallOrder = call;
                }
            });
        }
    }

    public class CoverletConsoleGlobalExeProvider_Tests
    {
        private AutoMoqer mocker;
        private CoverletConsoleDotnetToolsGlobalExecutor globalExeProvider;

        [SetUp]
        public void SetUp()
        {
            this.mocker = new AutoMoqer();
            this.globalExeProvider = this.mocker.Create<CoverletConsoleDotnetToolsGlobalExecutor>();
        }

        [Test]
        public void Should_Return_Null_From_GetRequest_If_Not_Enabled_In_Options()
        {
            var mockCoverageProject = new Mock<ICoverageProject>();
            _ = mockCoverageProject.Setup(cp => cp.Settings.CoverletConsoleGlobal).Returns(false);

            Assert.That(this.globalExeProvider.GetRequest(mockCoverageProject.Object, null), Is.Null);
        }

        [Test]
        public void Should_Return_Null_If_Enabled_But_Not_Installed()
        {
            var mockCoverageProject = new Mock<ICoverageProject>();
            _ = mockCoverageProject.Setup(cp => cp.Settings.CoverletConsoleGlobal).Returns(true);
            var dotNetToolListCoverlet = this.mocker.GetMock<IDotNetToolListCoverlet>();
            _ = dotNetToolListCoverlet.Setup(dotnet => dotnet.Global()).Returns((CoverletToolDetails)null);

            Assert.That(this.globalExeProvider.GetRequest(mockCoverageProject.Object, null), Is.Null);
            dotNetToolListCoverlet.VerifyAll();
        }

        [Test]
        public void Should_Log_When_Enabled_And_Unsuccessful()
        {
            var mockCoverageProject = new Mock<ICoverageProject>();
            _ = mockCoverageProject.Setup(cp => cp.Settings.CoverletConsoleGlobal).Returns(true);
            var dotNetToolListCoverlet = this.mocker.GetMock<IDotNetToolListCoverlet>();
            _ = dotNetToolListCoverlet.Setup(dotnet => dotnet.Global()).Returns((CoverletToolDetails)null);

            _ = this.globalExeProvider.GetRequest(mockCoverageProject.Object, null);
            this.mocker.Verify<ILogger>(l => l.Log("Unable to use Coverlet console global tool"));

        }

        private ExecuteRequest GetRequest_For_Globally_Installed_Coverlet_Console()
        {
            var mockCoverageProject = new Mock<ICoverageProject>();
            _ = mockCoverageProject.Setup(cp => cp.Settings.CoverletConsoleGlobal).Returns(true);
            _ = mockCoverageProject.Setup(cp => cp.ProjectOutputFolder).Returns("TheOutputFolder");
            var dotNetToolListCoverlet = this.mocker.GetMock<IDotNetToolListCoverlet>();
            _ = dotNetToolListCoverlet.Setup(dotnet => dotnet.Global()).Returns(new CoverletToolDetails { Command = "TheCommand" });

            return this.globalExeProvider.GetRequest(mockCoverageProject.Object, "coverlet settings");
        }

        [Test]
        public void Should_Request_Execute_With_The_Coverlet_Console_Command()
        {
            var request = this.GetRequest_For_Globally_Installed_Coverlet_Console();

            Assert.That(request.FilePath, Is.EqualTo("TheCommand"));
        }

        [Test]
        public void Should_Request_Arguments_The_Coverlet_Settings()
        {
            var request = this.GetRequest_For_Globally_Installed_Coverlet_Console();

            Assert.That(request.Arguments, Is.EqualTo("coverlet settings"));
        }

        [Test]
        public void Should_Request_WorkingDirectory_To_The_Project_Output_Folder()
        {
            var request = this.GetRequest_For_Globally_Installed_Coverlet_Console();

            Assert.That(request.WorkingDirectory, Is.EqualTo("TheOutputFolder"));
        }

    }

    public class CoverletConsoleCustomPathExecutor_Tests
    {
        private string tempCoverletExe;
        private AutoMoqer mocker;
        private CoverletConsoleCustomPathExecutor customPathExecutor;

        [SetUp]
        public void SetUp()
        {
            this.mocker = new AutoMoqer();
            this.customPathExecutor = this.mocker.Create<CoverletConsoleCustomPathExecutor>();
        }

        [TearDown]
        public void Delete_ProjectFile()
        {
            if (this.tempCoverletExe != null)
            {
                File.Delete(this.tempCoverletExe);
            }
        }

        [TestCase(null)]
        [TestCase("")]
        public void Should_Return_Null_If_Not_Set_In_Options(string coverletConsoleCustomPath)
        {
            var mockCoverageProject = new Mock<ICoverageProject>();
            _ = mockCoverageProject.Setup(cp => cp.Settings.CoverletConsoleCustomPath).Returns(coverletConsoleCustomPath);

            Assert.That(this.customPathExecutor.GetRequest(mockCoverageProject.Object, null), Is.Null);
        }

        [Test]
        public void Should_Return_Null_If_File_Does_Not_Exist()
        {
            var mockCoverageProject = new Mock<ICoverageProject>();
            _ = mockCoverageProject.Setup(cp => cp.Settings.CoverletConsoleCustomPath).Returns("alnlkalk.exe");

            Assert.That(this.customPathExecutor.GetRequest(mockCoverageProject.Object, null), Is.Null);
        }

        [Test]
        public void Should_Return_Null_If_Not_An_Exe()
        {
            this.tempCoverletExe = Path.Combine(Path.GetTempPath(), "thecoverletexecutable.notexe");
            File.WriteAllText(this.tempCoverletExe, "");

            var mockCoverageProject = new Mock<ICoverageProject>();
            _ = mockCoverageProject.Setup(cp => cp.Settings.CoverletConsoleCustomPath).Returns(this.tempCoverletExe);

            Assert.That(this.customPathExecutor.GetRequest(mockCoverageProject.Object, null), Is.Null);
        }

        private ExecuteRequest Get_Request_For_Custom_Path()
        {
            this.tempCoverletExe = Path.Combine(Path.GetTempPath(), "thecoverletexecutable.exe");
            File.WriteAllText(this.tempCoverletExe, "");

            var mockCoverageProject = new Mock<ICoverageProject>();
            _ = mockCoverageProject.Setup(cp => cp.Settings.CoverletConsoleCustomPath).Returns(this.tempCoverletExe);
            _ = mockCoverageProject.Setup(cp => cp.ProjectOutputFolder).Returns("TheOutputFolder");
            return this.customPathExecutor.GetRequest(mockCoverageProject.Object, "coverlet settings");
        }

        [Test]
        public void Should_Set_FilePath_To_The_Exe()
        {
            var executeRequest = this.Get_Request_For_Custom_Path();

            Assert.That(executeRequest.FilePath, Is.EqualTo(this.tempCoverletExe));
        }

        [Test]
        public void Should_Set_Arguments_To_Coverlet_Settings()
        {
            var executeRequest = this.Get_Request_For_Custom_Path();

            Assert.That(executeRequest.Arguments, Is.EqualTo("coverlet settings"));
        }

        [Test]
        public void Should_Request_WorkingDirectory_To_The_Project_Output_Folder()
        {
            var request = this.Get_Request_For_Custom_Path();

            Assert.That(request.WorkingDirectory, Is.EqualTo("TheOutputFolder"));
        }

    }

    public class CoverletConsoleLocalExeProvider_Tests
    {
        private AutoMoqer mocker;
        private CoverletConsoleDotnetToolsLocalExecutor localExecutor;

        [SetUp]
        public void SetUp()
        {
            this.mocker = new AutoMoqer();
            this.localExecutor = this.mocker.Create<CoverletConsoleDotnetToolsLocalExecutor>();
        }

        [Test]
        public void Should_Return_Null_If_Not_Enabled_In_Options()
        {
            var mockCoverageProject = new Mock<ICoverageProject>();
            _ = mockCoverageProject.Setup(cp => cp.Settings.CoverletConsoleLocal).Returns(false);

            Assert.That(this.localExecutor.GetRequest(mockCoverageProject.Object, null), Is.Null);
        }

        [Test]
        public void Should_Return_Null_If_No_DotNetConfig_Ascendant_Directory()
        {
            var projectOutputFolder = "projectoutputfolder";
            var mockCoverageProject = new Mock<ICoverageProject>();
            _ = mockCoverageProject.Setup(cp => cp.Settings.CoverletConsoleLocal).Returns(true);
            _ = mockCoverageProject.Setup(cp => cp.ProjectOutputFolder).Returns(projectOutputFolder);

            var mockDotNetConfigFinder = this.mocker.GetMock<IDotNetConfigFinder>();
            _ = mockDotNetConfigFinder.Setup(f => f.GetConfigDirectories(projectOutputFolder)).Returns(new List<string>());

            Assert.That(this.localExecutor.GetRequest(mockCoverageProject.Object, null), Is.Null);
            mockDotNetConfigFinder.VerifyAll();
        }

        [Test]
        public void Should_Return_Null_If_None_Of_The_DotNetConfig_Containing_Directories_Are_Local_Tool()
        {
            var projectOutputFolder = "projectoutputfolder";
            var mockCoverageProject = new Mock<ICoverageProject>();
            _ = mockCoverageProject.Setup(cp => cp.Settings.CoverletConsoleLocal).Returns(true);
            _ = mockCoverageProject.Setup(cp => cp.ProjectOutputFolder).Returns(projectOutputFolder);

            var mockDotNetConfigFinder = this.mocker.GetMock<IDotNetConfigFinder>();
            _ = mockDotNetConfigFinder.Setup(f => f.GetConfigDirectories(projectOutputFolder)).Returns(new List<string> { "ConfigDirectory1", "ConfigDirectory2" });

            var mockDotNetToolListCoverlet = this.mocker.GetMock<IDotNetToolListCoverlet>();
            _ = mockDotNetToolListCoverlet.Setup(dotnet => dotnet.Local("ConfigDirectory1")).Returns((CoverletToolDetails)null);
            _ = mockDotNetToolListCoverlet.Setup(dotnet => dotnet.Local("ConfigDirectory2")).Returns((CoverletToolDetails)null);

            Assert.That(this.localExecutor.GetRequest(mockCoverageProject.Object, null), Is.Null);
            mockDotNetToolListCoverlet.VerifyAll();


        }

        [Test]
        public void Shoul_Log_If_None_Of_The_DotNetConfig_Containing_Directories_Are_Local_Tool()
        {
            var projectOutputFolder = "projectoutputfolder";
            var mockCoverageProject = new Mock<ICoverageProject>();
            _ = mockCoverageProject.Setup(cp => cp.Settings.CoverletConsoleLocal).Returns(true);
            _ = mockCoverageProject.Setup(cp => cp.ProjectOutputFolder).Returns(projectOutputFolder);

            var mockDotNetConfigFinder = this.mocker.GetMock<IDotNetConfigFinder>();
            _ = mockDotNetConfigFinder.Setup(f => f.GetConfigDirectories(projectOutputFolder))
                .Returns(new List<string> { "ConfigDirectory1", "ConfigDirectory2" });

            var mockDotNetToolListCoverlet = this.mocker.GetMock<IDotNetToolListCoverlet>();
            _ = mockDotNetToolListCoverlet.Setup(dotnet => dotnet.Local("ConfigDirectory1")).Returns((CoverletToolDetails)null);
            _ = mockDotNetToolListCoverlet.Setup(dotnet => dotnet.Local("ConfigDirectory2")).Returns((CoverletToolDetails)null);

            _ = this.localExecutor.GetRequest(mockCoverageProject.Object, null);
            this.mocker.Verify<ILogger>(l => l.Log("Unable to use Coverlet console local tool"));
        }

        private ExecuteRequest Get_Request_For_Local_Install(bool firstConfigDirectoryLocalInstall, bool secondConfigDirectoryLocalInstall)
        {
            var projectOutputFolder = "projectoutputfolder";
            var mockCoverageProject = new Mock<ICoverageProject>();
            _ = mockCoverageProject.Setup(cp => cp.Settings.CoverletConsoleLocal).Returns(true);
            _ = mockCoverageProject.Setup(cp => cp.ProjectOutputFolder).Returns(projectOutputFolder);

            var mockDotNetConfigFinder = this.mocker.GetMock<IDotNetConfigFinder>();
            _ = mockDotNetConfigFinder.Setup(f => f.GetConfigDirectories(projectOutputFolder))
                .Returns(new List<string> { "ConfigDirectory1", "ConfigDirectory2" });

            var mockDotNetToolListCoverlet = this.mocker.GetMock<IDotNetToolListCoverlet>();
            var coverletToolDetails = new CoverletToolDetails { Command = "TheCommand" };
            _ = mockDotNetToolListCoverlet.Setup(dotnet => dotnet.Local("ConfigDirectory1"))
                .Returns(firstConfigDirectoryLocalInstall ? coverletToolDetails : null);
            _ = mockDotNetToolListCoverlet.Setup(dotnet => dotnet.Local("ConfigDirectory2"))
                .Returns(secondConfigDirectoryLocalInstall ? coverletToolDetails : null);
            return this.localExecutor.GetRequest(mockCoverageProject.Object, "coverlet settings");
        }

        [Test]
        public void Should_Use_The_WorkingDirectory_Of_The_Nearest_Local_Tool_Install()
        {
            var executeRequest = this.Get_Request_For_Local_Install(true, true);

            Assert.That(executeRequest.WorkingDirectory, Is.EqualTo("ConfigDirectory1"));
        }

        [Test]
        public void Should_Use_The_WorkingDirectory_Of_The_Nearest_Local_Tool_Install_Up()
        {
            var executeRequest = this.Get_Request_For_Local_Install(false, true);

            Assert.That(executeRequest.WorkingDirectory, Is.EqualTo("ConfigDirectory2"));
        }

        [Test]
        public void Should_Use_The_DotNet_Command()
        {
            var executeRequest = this.Get_Request_For_Local_Install(true, true);

            Assert.That(executeRequest.FilePath, Is.EqualTo("dotnet"));
        }

        [Test]
        public void Should_Use_The_Coverlet_Command()
        {
            var executeRequest = this.Get_Request_For_Local_Install(true, true);

            Assert.That(executeRequest.Arguments, Is.EqualTo("TheCommand coverlet settings"));
        }
    }
}
