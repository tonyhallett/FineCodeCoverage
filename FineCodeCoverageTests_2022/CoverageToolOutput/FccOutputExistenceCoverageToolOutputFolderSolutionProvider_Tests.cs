namespace FineCodeCoverageTests.CoverageToolOutput_Tests
{
    using System.IO;
    using AutoMoq;
    using FineCodeCoverage.Core.Utilities;
    using FineCodeCoverage.Engine;
    using NUnit.Framework;

    internal class FccOutputExistenceCoverageToolOutputFolderSolutionProvider_Tests
    {
        private AutoMoqer mocker;

        [SetUp]
        public void SetUp() => this.mocker = new AutoMoqer();

        [Test]
        public void Should_Return_Null_If_No_Solution_Folder_Provided_To_It()
        {
            var provider = this.mocker.Create<FccOutputExistenceCoverageToolOutputFolderSolutionProvider>();
            Assert.That(provider.Provide(() => null), Is.Null);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void Should_Return_Path_To_FCC_Output_Folder_In_Solution_Folder_If_Exists(bool exists)
        {
            var solutionFolder = "SolutionFolder";
            var expected = Path.Combine("SolutionFolder", "fcc-output");

            var mockFileUtil = this.mocker.GetMock<IFileUtil>();
            _ = mockFileUtil.Setup(fu => fu.DirectoryExists(expected)).Returns(exists);

            var provider = this.mocker.Create<FccOutputExistenceCoverageToolOutputFolderSolutionProvider>();
            Assert.That(exists ? expected : null, Is.EqualTo(provider.Provide(() => solutionFolder)));
        }

        [Test]
        public void Should_Have_Second_Order() =>
            MefOrderAssertions.TypeHasExpectedOrder(typeof(FccOutputExistenceCoverageToolOutputFolderSolutionProvider), 2);
    }
}
