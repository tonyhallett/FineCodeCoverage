namespace FineCodeCoverageTests.OutputToolWindowControl_Tests
{
    using System.Threading;
    using FineCodeCoverage.Core.ReportGenerator.Colours;
    using FineCodeCoverage.Core.Utilities;
    using FineCodeCoverage.Options;
    using FineCodeCoverage.Output;
    using FineCodeCoverage.Output.HostObjects;
    using Moq;
    using NUnit.Framework;

    internal class OutputToolWindowControl_Tests
    {
        [Apartment(ApartmentState.STA)]
        [Test]
        public void Should_Listen_For_Messages()
        {
            var mockEventAggregator = new Mock<IEventAggregator>();
            var outputToolWindowControl = new OutputToolWindowControl(
                mockEventAggregator.Object,
                new Mock<IReportColoursProvider>().Object,
                new System.Collections.Generic.List<IWebViewHostObjectRegistration>(),
                new Mock<IAppOptionsProvider>().Object,
                new Mock<IEnvironmentFont>().Object
            );

            mockEventAggregator.Verify(
                eventAggregator => eventAggregator.AddListener(outputToolWindowControl, null)
            );
        }
    }
}
