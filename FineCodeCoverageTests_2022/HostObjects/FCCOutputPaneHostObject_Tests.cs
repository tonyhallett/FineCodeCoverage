namespace FineCodeCoverageTests.HostObjectTests
{
    using FineCodeCoverage.Core.Utilities;
    using FineCodeCoverage.Impl;
    using FineCodeCoverage.Output.HostObjects;
    using Moq;
    using NUnit.Framework;

    internal class FCCOutputPaneHostObject_Tests
    {
        [Test]
        public void Should_Have_Name()
        {
            var fccOutputPaneRegistration = new FCCOutputPaneHostObjectRegistration(null);
            Assert.That(fccOutputPaneRegistration.Name, Is.EqualTo(FCCOutputPaneHostObjectRegistration.HostObjectName));
        }

        [Test]
        public void Should_Send_ShowFCCOutputPaneMessage_When_Js_Calls_Show()
        {
            var mockEventAggregator = new Mock<IEventAggregator>();
            var fccOutputPaneRegistration = new FCCOutputPaneHostObjectRegistration(mockEventAggregator.Object);
            (fccOutputPaneRegistration.HostObject as FCCOutputPaneHostObject).show();

            mockEventAggregator.Verify(eventAggregator => eventAggregator.SendMessage(It.IsAny<ShowFCCOutputPaneMessage>(), null));
        }
    }
}
