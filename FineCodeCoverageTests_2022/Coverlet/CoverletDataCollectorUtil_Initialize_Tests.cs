namespace FineCodeCoverageTests.Coverlet_Tests
{
    using System.IO;
    using System.Threading;
    using AutoMoq;
    using FineCodeCoverage.Core.Utilities;
    using FineCodeCoverage.Engine.Coverlet;
    using NUnit.Framework;

    public class CoverletDataCollectorUtil_Initialize_Tests
    {
        private AutoMoqer mocker;
        private CoverletDataCollectorUtil coverletDataCollector;

        [SetUp]
        public void SetUp()
        {
            this.mocker = new AutoMoqer();
            this.coverletDataCollector = this.mocker.Create<CoverletDataCollectorUtil>();
        }


        [Test]
        public void Should_Ensure_Unzipped_And_Sets_The_Quoted_TestAdapterPathArg()
        {
            var ct = CancellationToken.None;
            var zipDetails = new ZipDetails { Path = "path", Version = "version" };
            var mockToolZipProvider = this.mocker.GetMock<IToolZipProvider>();
            _ = mockToolZipProvider.Setup(zp => zp.ProvideZip(CoverletDataCollectorUtil.zipPrefix)).Returns(zipDetails);

            var mockToolFolder = this.mocker.GetMock<IToolFolder>();
            _ = mockToolFolder.Setup(cf => cf.EnsureUnzipped("appdatafolder", CoverletDataCollectorUtil.zipDirectoryName, zipDetails, ct)).Returns("zipdestination");

            this.coverletDataCollector.Initialize("appdatafolder", ct);
            Assert.That(this.coverletDataCollector.TestAdapterPathArg, Is.EqualTo($@"""{Path.Combine("zipdestination", "build", "netstandard1.0")}"""));

        }
    }
}
