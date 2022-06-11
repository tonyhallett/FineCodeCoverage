namespace FineCodeCoverageTests.CoverageToolOutput_Tests
{
    using System.IO;
    using AutoMoq;
    using FineCodeCoverage.Engine;
    using FineCodeCoverage.Options;
    using Moq;
    using NUnit.Framework;

    internal class AppOptionsCoverageToolOutputFolderSolutionProvider_Tests
    {
        private AutoMoqer mocker;

        [SetUp]
        public void SetUp() => this.mocker = new AutoMoqer();

        [TestCase(null)]
        [TestCase("")]
        public void Should_Return_Null_Without_Getting_Solution_Folder_When_AppOption_FCCSolutionOutputDirectoryName_NotSet(string optionValue)
        {
            var mockAppOptionsProvider = this.mocker.GetMock<IAppOptionsProvider>();
            var mockAppOptions = new Mock<IAppOptions>();
            _ = mockAppOptions.SetupGet(options => options.FCCSolutionOutputDirectoryName).Returns(optionValue);
            _ = mockAppOptionsProvider.Setup(aop => aop.Get()).Returns(mockAppOptions.Object);

            var provider = this.mocker.Create<AppOptionsCoverageToolOutputFolderSolutionProvider>();
            var providedSolutionFolder = false;

            Assert.Multiple(() =>
            {
                Assert.That(provider.Provide(() =>
                {
                    providedSolutionFolder = true;
                    return null;
                }), Is.Null);

                Assert.That(providedSolutionFolder, Is.False);
            });

        }

        [Test]
        public void Should_Return_Null_If_No_Solution_Folder_Provided_To_It()
        {
            var mockAppOptionsProvider = this.mocker.GetMock<IAppOptionsProvider>();
            var mockAppOptions = new Mock<IAppOptions>();
            _ = mockAppOptions.SetupGet(options => options.FCCSolutionOutputDirectoryName).Returns("Value");
            _ = mockAppOptionsProvider.Setup(aop => aop.Get()).Returns(mockAppOptions.Object);
            var provider = this.mocker.Create<AppOptionsCoverageToolOutputFolderSolutionProvider>();
            Assert.That(provider.Provide(() => null), Is.Null);
        }

        [Test]
        public void Should_Combine_The_Solution_Folder_With_FCCSolutionOutputDirectoryName()
        {
            var mockAppOptionsProvider = this.mocker.GetMock<IAppOptionsProvider>();
            var mockAppOptions = new Mock<IAppOptions>();
            _ = mockAppOptions.SetupGet(options => options.FCCSolutionOutputDirectoryName).Returns("FCCOutput");
            _ = mockAppOptionsProvider.Setup(aop => aop.Get()).Returns(mockAppOptions.Object);
            var provider = this.mocker.Create<AppOptionsCoverageToolOutputFolderSolutionProvider>();
            Assert.That(Path.Combine("SolutionFolder", "FCCOutput"), Is.EqualTo(provider.Provide(() => "SolutionFolder")));
        }

        [Test]
        public void Should_Have_First_Order() =>
            MefOrderAssertions.TypeHasExpectedOrder(typeof(AppOptionsCoverageToolOutputFolderSolutionProvider), 1);
    }
}
