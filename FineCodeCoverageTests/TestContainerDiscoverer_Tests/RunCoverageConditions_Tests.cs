using AutoMoq;
using FineCodeCoverage.Core.Utilities;
using FineCodeCoverage.Impl;
using FineCodeCoverage.Options;
using Moq;
using NUnit.Framework;
using FineCodeCoverage.Output.JsMessages.Logging;

namespace FineCodeCoverageTests.TestContainerDiscoverer_Tests
{
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
            mockTestOperation.Setup(o => o.FailedTests).Returns(numberFailedTests);
            mockTestOperation.Setup(o => o.TotalTests).Returns(totalTests);

            var mockAppOptions = new Mock<IAppOptions>();
            mockAppOptions.Setup(o => o.RunWhenTestsFail).Returns(runWhenTestsFail);
            mockAppOptions.Setup(o => o.RunWhenTestsExceed).Returns(runWhenTestsExceed);

            mocker = new AutoMoqer();
            var runCoverageConditions = mocker.Create<RunCoverageConditions>();

            return runCoverageConditions.Met(mockTestOperation.Object, mockAppOptions.Object);
        }

        private bool Skip_Coverage_Failed_Tests()
        {
            return CoverageConditionsMet(false, 1, 0, 0);
        }

        [Test]
        public void Should_Not_Be_Met_When_Tests_Fail_And_RunWhenTestsFail_Is_False()
        {
            var met = Skip_Coverage_Failed_Tests();
            Assert.IsFalse(met);
        }

        [Test]
        public void Should_Log_When_Skipping_Coverage_Due_To_Failed_Tests()
        {
            Skip_Coverage_Failed_Tests();
            mocker.Verify<ILogger>(logger => logger.Log($"Skipping coverage due to failed tests.  Option {nameof(AppOptions.RunWhenTestsFail)} is false"));
        }

        [Test]
        public void Should_Send_ToolWindow_Warning_LogMessage_When_Skipping_Coverage_Due_To_Failed_Tests()
        {
            Skip_Coverage_Failed_Tests();
            mocker.GetMock<IEventAggregator>().AssertSimpleSingleLog(
                $"Skipping coverage due to failed tests.  Option {nameof(AppOptions.RunWhenTestsFail)} is false",
                MessageContext.Warning
            );
        }

        private bool TotalTests_Do_Not_Exceed()
        {
            return CoverageConditionsMet(true, 0, 1, 2);
        }

        [Test]
        public void Should_Not_Be_Met_When_TotalTests_Do_Not_Exceed_Settings()
        {
            var met = TotalTests_Do_Not_Exceed();
            Assert.IsFalse(met);
        }

        [Test]
        public void Should_Log_When_Skipping_Coverage_Due_To_Insufficient_Tests()
        {
            TotalTests_Do_Not_Exceed();

            mocker.Verify<ILogger>(logger => logger.Log($"Skipping coverage as total tests (1) <= {nameof(AppOptions.RunWhenTestsExceed)} (2)"));
        }

        [Test]
        public void Should_Send_ToolWindow_Warning_LogMessage_When_Skipping_Coverage_Due_To_Insufficient_Tests()
        {
            TotalTests_Do_Not_Exceed();

            mocker.GetMock<IEventAggregator>().
                AssertSimpleSingleLog(
                    $"Skipping coverage as total tests (1) <= {nameof(AppOptions.RunWhenTestsExceed)} (2)", 
                    MessageContext.Warning
                );
        }

        [Test]
        public void Should_Be_Met_When_TotalTests_Is_Zero()
        {
            var met = CoverageConditionsMet(true, 0, 0, 10);
            Assert.True(met);
        }

        [Test]
        public void Should_Be_Met_When_All_Conditions_Are_Met()
        {
            var met = CoverageConditionsMet(false, 0, 2, 1);
            Assert.True(met);
        }
    
    }
}