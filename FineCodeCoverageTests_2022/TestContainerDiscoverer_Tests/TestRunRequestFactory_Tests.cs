namespace FineCodeCoverageTests.TestContainerDiscoverer_Tests
{
    using Microsoft.VisualStudio.TestWindow.Extensibility;
    using System;
    using System.Collections.Generic;
    using AutoMoq;
    using ILogger = FineCodeCoverage.ILogger;
    using FineCodeCoverage.Impl;
    using NUnit.Framework;

    internal class TestRunRequestFactory_Tests
    {
        private class ReflectionErrorOperation : IOperation
        {
            public TestOperationStates Kind => throw new NotImplementedException();

            public TimeSpan TotalRuntime => throw new NotImplementedException();

            public IEnumerable<Uri> GetRunSettingsDataCollectorResultUri(Uri collectorUri) => throw new NotImplementedException();
        }

        [Test]
        public void Should_Log_When_Reflection_Exception()
        {
            var mocker = new AutoMoqer();
            var testRunRequestFactory = mocker.Create<TestRunRequestFactory>();

            _ = Assert.Throws<Exception>(() => testRunRequestFactory.Create(new ReflectionErrorOperation()));

            mocker.Verify<ILogger>(logger => logger.Log("Error test container discoverer reflection"));
        }
    }
}
