namespace FineCodeCoverageTests.CoverageRunner_Tests
{
    using System.Collections.Generic;
    using AutoMoq;
    using FineCodeCoverage.Impl;
    using NUnit.Framework;

    internal class TestExplorerInstantiationHook_Tests
    {
        private TestInstantiationPathAware testInstantiationPathAware1;
        private TestInstantiationPathAware testInstantiationPathAware2;
        private TestExplorerInstantiationHook testExplorerInstantiationHook;

        internal class TestInstantiationPathAware : ITestInstantiationPathAware
        {
            public bool Notified { get; private set; }
            public void TestExplorerInstantion() => this.Notified = true;
        }

        [SetUp]
        public void SetUp()
        {
            var mocker = new AutoMoqer();
            this.testInstantiationPathAware1 = new TestInstantiationPathAware();
            this.testInstantiationPathAware2 = new TestInstantiationPathAware();
            mocker.SetInstance<IEnumerable<ITestInstantiationPathAware>>(new List<ITestInstantiationPathAware> {
                this.testInstantiationPathAware1,
                this.testInstantiationPathAware2
            });

            this.testExplorerInstantiationHook = mocker.Create<TestExplorerInstantiationHook>();
        }

        [Test]
        public void Should_Provide_ExecutorUri() =>
            Assert.That(
                this.testExplorerInstantiationHook.ExecutorUri.ToString(),
                Is.EqualTo("executor://finecodecoverage.executor/v1")
            );

        [Test]
        public void Should_Provide_Empty_TestContainers() =>
            Assert.That(this.testExplorerInstantiationHook.TestContainers, Is.Empty);

        [Test]
        public void Should_Notify_All_TestInstantiationPathAware() =>
            Assert.Multiple(() =>
            {
                Assert.That(this.testInstantiationPathAware1.Notified, Is.True);
                Assert.That(this.testInstantiationPathAware2.Notified, Is.True);
            });
    }
}
