namespace FineCodeCoverageTests.TestContainerDiscoverer_Tests
{
    using AutoMoq;
    using FineCodeCoverage.Logging;
    using FineCodeCoverage.Core.Utilities;
    using FineCodeCoverage.Impl;
    using FineCodeCoverage.Options;
    using FineCodeCoverage.Output.JsMessages.Logging;
    using Moq;
    using NUnit.Framework;

    internal class RunCoverageConditions_Tests
    {
        private AutoMoqer mocker;
        private bool CoverageConditionsMet(
            bool runWhenTestsFail,
            long numberFailedTests,
            long totalTests,
            int runWhenTestsExceed)
        {
            var mockTestOperation = new Mock<ITestOperation>();
            _ = mockTestOperation.Setup(o => o.FailedTests).Returns(numberFailedTests);
            _ = mockTestOperation.Setup(o => o.TotalTests).Returns(totalTests);

            var mockAppOptions = new Mock<IAppOptions>();
            _ = mockAppOptions.Setup(o => o.RunWhenTestsFail).Returns(runWhenTestsFail);
            _ = mockAppOptions.Setup(o => o.RunWhenTestsExceed).Returns(runWhenTestsExceed);

            this.mocker = new AutoMoqer();
            var runCoverageConditions = this.mocker.Create<RunCoverageConditions>();

            return runCoverageConditions.Met(mockTestOperation.Object, mockAppOptions.Object);
        }

        private bool Skip_Coverage_Failed_Tests() => this.CoverageConditionsMet(false, 1, 0, 0);

        [Test]
        public void Should_Not_Be_Met_When_Tests_Fail_And_RunWhenTestsFail_Is_False()
        {
            var met = this.Skip_Coverage_Failed_Tests();
            Assert.That(met, Is.False);
        }

        [Test]
        public void Should_Log_When_Skipping_Coverage_Due_To_Failed_Tests()
        {
            _ = this.Skip_Coverage_Failed_Tests();
            this.mocker.Verify<ILogger>(logger => logger.Log($"Skipping coverage due to failed tests.  Option {nameof(AppOptions.RunWhenTestsFail)} is false"));
        }

        [Test]
        public void Should_Send_ToolWindow_Warning_LogMessage_When_Skipping_Coverage_Due_To_Failed_Tests()
        {
            _ = this.Skip_Coverage_Failed_Tests();
            this.mocker.GetMock<IEventAggregator>().AssertSimpleSingleLog(
                $"Skipping coverage due to failed tests.  Option {nameof(AppOptions.RunWhenTestsFail)} is false",
                MessageContext.Warning
            );
        }

        private bool TotalTests_Do_Not_Exceed() => this.CoverageConditionsMet(true, 0, 1, 2);

        [Test]
        public void Should_Not_Be_Met_When_TotalTests_Do_Not_Exceed_Settings()
        {
            var met = this.TotalTests_Do_Not_Exceed();
            Assert.That(met, Is.False);
        }

        [Test]
        public void Should_Log_When_Skipping_Coverage_Due_To_Insufficient_Tests()
        {
            _ = this.TotalTests_Do_Not_Exceed();

            this.mocker.Verify<ILogger>(logger => logger.Log($"Skipping coverage as total tests (1) <= {nameof(AppOptions.RunWhenTestsExceed)} (2)"));
        }

        [Test]
        public void Should_Send_ToolWindow_Warning_LogMessage_When_Skipping_Coverage_Due_To_Insufficient_Tests()
        {
            _ = this.TotalTests_Do_Not_Exceed();

            this.mocker.GetMock<IEventAggregator>().
                AssertSimpleSingleLog(
                    $"Skipping coverage as total tests (1) <= {nameof(AppOptions.RunWhenTestsExceed)} (2)",
                    MessageContext.Warning
                );
        }

        [Test]
        public void Should_Be_Met_When_TotalTests_Is_Zero()
        {
            var met = this.CoverageConditionsMet(true, 0, 0, 10);
            Assert.That(met, Is.True);
        }

        [Test]
        public void Should_Be_Met_When_All_Conditions_Are_Met()
        {
            var met = this.CoverageConditionsMet(false, 0, 2, 1);
            Assert.That(met, Is.True);
        }

    }
}
