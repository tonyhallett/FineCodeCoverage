namespace FineCodeCoverageTests.DotNetTool_Tests
{
    using System.Collections.Generic;
    using AutoMoq;
    using FineCodeCoverage.Logging;
    using FineCodeCoverage.Core.Utilities;
    using FineCodeCoverage.Engine.Coverlet;
    using NUnit.Framework;

    public static class DotNetToolListOutput
    {
        public static readonly string GlobalWithTool = "Package Id            Version Commands\r\n-------------------------------------------\r\ncoverlet.console      3.0.3        coverlet\r\n";
        public static readonly string GlobalToolPathNoTool = "Package Id      Version      Commands\r\n-------------------------------------\r\n";
        public static readonly string LocalWithTool = "Package Id            Version      Commands      Manifest                                                           \r\n--------------------------------------------------------------------------------------------------------------------\r\ncoverlet.console      3.0.3        coverlet      C:\\Users\\tonyh\\Source\\Repos\\SUT WithSpace\\.config\\dotnet-tools.json\r\n";
        public static readonly string LocalNoTool = "Package Id      Version Commands      Manifest\r\n---------------------------------------------------\r\n";
    }

    public class DotNetToolListParser_Tests
    {
        [Test]
        public void Should_Return_Empty_List_If_No_Tools_Installed()
        {
            var dotnetToolListParser = new DotNetToolListParser();
            Assert.That(dotnetToolListParser.Parse(DotNetToolListOutput.LocalNoTool), Is.Empty);
        }

        [Test]
        public void Should_Parse_Local()
        {
            var dotnetToolListParser = new DotNetToolListParser();
            var parsed = dotnetToolListParser.Parse(DotNetToolListOutput.LocalWithTool);
            Assert.That(parsed, Has.Count.EqualTo(1));
            var installedTool = parsed[0];
            Assert.Multiple(() =>
            {
                Assert.That(installedTool.Commands, Is.EqualTo("coverlet"));
                Assert.That(installedTool.Version, Is.EqualTo("3.0.3"));
                Assert.That(installedTool.PackageId, Is.EqualTo("coverlet.console"));
            });
        }

        [Test]
        public void Should_Parse_Global()
        {
            var dotnetToolListParser = new DotNetToolListParser();
            var parsed = dotnetToolListParser.Parse(DotNetToolListOutput.GlobalWithTool);
            Assert.That(parsed, Has.Count.EqualTo(1));
            var installedTool = parsed[0];
            Assert.Multiple(() =>
            {
                Assert.That(installedTool.Commands, Is.EqualTo("coverlet"));
                Assert.That(installedTool.Version, Is.EqualTo("3.0.3"));
                Assert.That(installedTool.PackageId, Is.EqualTo("coverlet.console"));
            });
        }
    }

    public class DotNetToolListCoverlet_Tests
    {
        private AutoMoqer mocker;
        private DotNetToolListCoverlet dotNetToolListCoverlet;

        [SetUp]
        public void SetUp()
        {
            this.mocker = new AutoMoqer();
            this.dotNetToolListCoverlet = this.mocker.Create<DotNetToolListCoverlet>();
        }

        [Test]
        public void Should_Execute_And_Parse_Global_Installed()
        {
            var mockExecutor = this.mocker.GetMock<IDotNetToolListExecutor>();
            var globalOutput = "global";
            _ = mockExecutor.Setup(executor => executor.Global()).Returns(new DotNetToolListExecutionResult { ExitCode = 0, Output = globalOutput });
            var mockParser = this.mocker.GetMock<IDotNetToolListParser>();
            _ = mockParser.Setup(parser => parser.Parse(globalOutput)).Returns(new List<DotNetTool> { new DotNetTool { PackageId = "coverlet.console", Commands = "theCommand", Version = "theVersion" } });
            var coverletToolDetails = this.dotNetToolListCoverlet.Global();

            Assert.Multiple(() =>
            {
                Assert.That(coverletToolDetails.Command, Is.EqualTo("theCommand"));
                Assert.That(coverletToolDetails.Version, Is.EqualTo("theVersion"));
            });
        }

        [Test]
        public void Should_Execute_And_Parse_Global_Not_Installed()
        {
            var mockExecutor = this.mocker.GetMock<IDotNetToolListExecutor>();
            var globalOutput = "global";
            _ = mockExecutor.Setup(executor => executor.Global()).Returns(new DotNetToolListExecutionResult { ExitCode = 0, Output = globalOutput });
            var mockParser = this.mocker.GetMock<IDotNetToolListParser>();
            _ = mockParser.Setup(parser => parser.Parse(globalOutput)).Returns(new List<DotNetTool> { new DotNetTool { PackageId = "not.coverlet.console", Commands = "theCommand", Version = "theVersion" } });
            var coverletToolDetails = this.dotNetToolListCoverlet.Global();
            Assert.That(coverletToolDetails, Is.Null);
        }

        [Test]
        public void Should_Log_Output_And_Return_Null_When_Parsing_Error()
        {
            var parsing = "this will be parsed";
            var mockExecutor = this.mocker.GetMock<IDotNetToolListExecutor>();
            _ = mockExecutor.Setup(executor => executor.Global()).Returns(new DotNetToolListExecutionResult { ExitCode = 0, Output = parsing });
            var mockParser = this.mocker.GetMock<IDotNetToolListParser>();
            _ = mockParser.Setup(parser => parser.Parse(parsing)).Throws(new System.Exception());
            var coverletToolDetails = this.dotNetToolListCoverlet.Global();
            Assert.That(coverletToolDetails, Is.Null);
            this.mocker.Verify<ILogger>(l => l.Log("Dotnet tool list Coverlet Error parsing", parsing));
        }

        [Test]
        public void Should_Log_Output_When_Executor_Error()
        {
            var mockExecutor = this.mocker.GetMock<IDotNetToolListExecutor>();
            var globalErrorOutput = "this is an error";
            _ = mockExecutor.Setup(executor => executor.Global()).Returns(new DotNetToolListExecutionResult { ExitCode = 1, Output = globalErrorOutput });
            var coverletToolDetails = this.dotNetToolListCoverlet.Global();
            Assert.That(coverletToolDetails, Is.Null);
            this.mocker.Verify<ILogger>(l => l.Log("Dotnet tool list Coverlet Error", globalErrorOutput));
        }

        [Test]
        public void Should_Execute_And_Parse_Local_Installed()
        {
            var localDirectory = "localDir";
            var mockExecutor = this.mocker.GetMock<IDotNetToolListExecutor>();
            var localOutput = "local";
            _ = mockExecutor.Setup(executor => executor.Local(localDirectory)).Returns(new DotNetToolListExecutionResult { ExitCode = 0, Output = localOutput });
            var mockParser = this.mocker.GetMock<IDotNetToolListParser>();
            _ = mockParser.Setup(parser => parser.Parse(localOutput)).Returns(new List<DotNetTool> { new DotNetTool { PackageId = "coverlet.console", Commands = "theCommand", Version = "theVersion" } });
            var coverletToolDetails = this.dotNetToolListCoverlet.Local(localDirectory);

            Assert.Multiple(() =>
            {
                Assert.That(coverletToolDetails.Command, Is.EqualTo("theCommand"));
                Assert.That(coverletToolDetails.Version, Is.EqualTo("theVersion"));
            });
        }

        [Test]
        public void Should_Execute_And_Parse_Global_Tools_Path_Installed()
        {
            var globalToolsDirectory = "globalToolsDir";
            var mockExecutor = this.mocker.GetMock<IDotNetToolListExecutor>();
            var globalToolsOutput = "local";
            _ = mockExecutor.Setup(executor => executor.GlobalToolsPath(globalToolsDirectory)).Returns(new DotNetToolListExecutionResult { ExitCode = 0, Output = globalToolsOutput });
            var mockParser = this.mocker.GetMock<IDotNetToolListParser>();
            _ = mockParser.Setup(parser => parser.Parse(globalToolsOutput)).Returns(new List<DotNetTool> { new DotNetTool { PackageId = "coverlet.console", Commands = "theCommand", Version = "theVersion" } });

            var coverletToolDetails = this.dotNetToolListCoverlet.GlobalToolsPath(globalToolsDirectory);

            Assert.Multiple(() =>
            {
                Assert.That(coverletToolDetails.Command, Is.EqualTo("theCommand"));
                Assert.That(coverletToolDetails.Version, Is.EqualTo("theVersion"));
            });
        }
    }
}
