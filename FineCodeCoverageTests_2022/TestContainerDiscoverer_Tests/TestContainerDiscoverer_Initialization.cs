namespace FineCodeCoverageTests.TestContainerDiscoverer_Tests
{
    using System.Collections.Generic;
    using System.Linq;
    using AutoMoq;
    using FineCodeCoverage.Impl;
    using Microsoft.VisualStudio.TestWindow.Extensibility;
    using Moq;
    using NUnit.Framework;

    internal class TestContainerDiscoverer_ITestInstantiationPathAware_Notification
    {
        [Test]
        public void Should_Occur_In_The_Constructor()
        {
            var mocker = new AutoMoqer();
            var mocks = new List<Mock<ITestInstantiationPathAware>>
            {
                new Mock<ITestInstantiationPathAware>(),
                new Mock<ITestInstantiationPathAware>()
            };
            mocker.SetInstance(mocks.Select(mock => mock.Object));
            var testContainerDiscoverer = mocker.Create<TestContainerDiscoverer>();

            var operationState = mocker.GetMock<IOperationState>().Object;
            mocks.ForEach(mock =>
                mock.Verify(testInstantiationPathAware => testInstantiationPathAware.Notify(operationState))
            );
        }

    }
}
