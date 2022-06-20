namespace FineCodeCoverageTests.MsCodeCoverage_Tests
{
    using System.Collections.Generic;
    using System.IO;
    using System.Xml.XPath;
    using AutoMoq;
    using FineCodeCoverage.Engine.MsTestPlatform.CodeCoverage;
    using Microsoft.VisualStudio.TestWindow.Extensibility;
    using Moq;
    using NUnit.Framework;

    internal class MsCodeCoverageRunSettingsService_IRunSettingsService_Tests
    {
        private AutoMoqer autoMocker;
        private MsCodeCoverageRunSettingsService msCodeCoverageRunSettingsService;
        [SetUp]
        public void CreateSut()
        {
            this.autoMocker = new AutoMoqer();
            this.msCodeCoverageRunSettingsService = this.autoMocker.Create<MsCodeCoverageRunSettingsService>();
            this.msCodeCoverageRunSettingsService.SetZipDestination("ZipDestination");
        }

        [Test]
        public void Should_Have_A_Name() =>
            Assert.That(string.IsNullOrWhiteSpace(this.msCodeCoverageRunSettingsService.Name), Is.False);

        [TestCase(RunSettingConfigurationInfoState.Discovery)]
        [TestCase(RunSettingConfigurationInfoState.None)]
        public void Should_Not_Delegate_To_UserRunSettingsService_When_Not_Test_Execution(RunSettingConfigurationInfoState state)
        {
            this.SetuserRunSettingsProjectDetailsLookup(false);
            this.msCodeCoverageRunSettingsService.collectionStatus = MsCodeCoverageCollectionStatus.Collecting;

            this.ShouldNotDelegateToUserRunSettingsService(state);
        }

        [TestCase(MsCodeCoverageCollectionStatus.NotCollecting)]
        [TestCase(MsCodeCoverageCollectionStatus.Error)]
        public void Should_Not_Delegate_To_UserRunSettingsService_When_Is_Not_Collecting(MsCodeCoverageCollectionStatus status)
        {
            this.msCodeCoverageRunSettingsService.collectionStatus = status;
            this.SetuserRunSettingsProjectDetailsLookup(false);

            this.ShouldNotDelegateToUserRunSettingsService(RunSettingConfigurationInfoState.Execution);
        }

        [Test]
        public void Should_Not_Delegate_To_UserRunSettingsService_When_No_User_RunSettings()
        {
            this.msCodeCoverageRunSettingsService.collectionStatus = MsCodeCoverageCollectionStatus.Collecting;
            this.SetuserRunSettingsProjectDetailsLookup(true);

            this.ShouldNotDelegateToUserRunSettingsService(RunSettingConfigurationInfoState.Execution);
        }

        private void ShouldNotDelegateToUserRunSettingsService(RunSettingConfigurationInfoState state)
        {
            var mockRunSettingsConfigurationInfo = new Mock<IRunSettingsConfigurationInfo>();
            _ = mockRunSettingsConfigurationInfo.Setup(ci => ci.RequestState).Returns(state);

            _ = this.autoMocker.GetMock<IUserRunSettingsService>()
                .Setup(userRunSettingsService => userRunSettingsService.AddFCCRunSettings(
                    It.IsAny<IXPathNavigable>(),
                    It.IsAny<IRunSettingsConfigurationInfo>(),
                    It.IsAny<Dictionary<string, IUserRunSettingsProjectDetails>>(),
                    It.IsAny<string>()
                )).Returns(new Mock<IXPathNavigable>().Object);

            Assert.That(
                this.msCodeCoverageRunSettingsService.AddRunSettings(null, mockRunSettingsConfigurationInfo.Object, null),
                Is.Null
            );
        }

        private void SetuserRunSettingsProjectDetailsLookup(bool empty)
        {
            var userRunSettingsProjectDetailsLookup = new Dictionary<string, IUserRunSettingsProjectDetails>();
            if (!empty)
            {
                userRunSettingsProjectDetailsLookup.Add("", null); // an entry
            }
            this.msCodeCoverageRunSettingsService.userRunSettingsProjectDetailsLookup = userRunSettingsProjectDetailsLookup;
        }

        [Test]
        public void Should_Delegate_To_UserRunSettingsService_With_UserRunSettingsProjectDetailsLookup_And_FCC_Ms_TestAdapter_Path_When_Applicable()
        {
            var inputRunSettingDocument = new Mock<IXPathNavigable>().Object;

            var mockRunSettingsConfigurationInfo = new Mock<IRunSettingsConfigurationInfo>();
            _ = mockRunSettingsConfigurationInfo.Setup(ci => ci.RequestState).Returns(RunSettingConfigurationInfoState.Execution);
            var runSettingsConfigurationInfo = mockRunSettingsConfigurationInfo.Object;

            // IsCollecting would set this
            var userRunSettingsProjectDetailsLookup = new Dictionary<string, IUserRunSettingsProjectDetails>
            {
                { "",null} // an entry
            };
            this.msCodeCoverageRunSettingsService.userRunSettingsProjectDetailsLookup = userRunSettingsProjectDetailsLookup;
            this.msCodeCoverageRunSettingsService.collectionStatus = MsCodeCoverageCollectionStatus.Collecting;


            var mockUserRunSettingsService = this.autoMocker.GetMock<IUserRunSettingsService>();
            var fccRunSettingDocument = new Mock<IXPathNavigable>().Object;
            var expectedFccMsTestAdapter = Path.Combine("ZipDestination", "build", "netstandard1.0");
            var addFCCRunSettingsSetup = mockUserRunSettingsService.Setup(userRunSettingsService => userRunSettingsService.AddFCCRunSettings(
                inputRunSettingDocument,
                runSettingsConfigurationInfo,
                It.IsAny<Dictionary<string, IUserRunSettingsProjectDetails>>(),
                expectedFccMsTestAdapter)
            ).Returns(fccRunSettingDocument);

            Assert.That(this.msCodeCoverageRunSettingsService.AddRunSettings(inputRunSettingDocument, mockRunSettingsConfigurationInfo.Object, null), Is.SameAs(fccRunSettingDocument));

            var addFCCRunSettingsInvocation = mockUserRunSettingsService.Invocations[0];
            Assert.That(addFCCRunSettingsInvocation.Arguments[2], Is.SameAs(userRunSettingsProjectDetailsLookup));
        }
    }
}
