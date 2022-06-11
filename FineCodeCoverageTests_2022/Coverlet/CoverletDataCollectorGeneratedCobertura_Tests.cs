namespace FineCodeCoverageTests.Coverlet_Tests
{
    using System;
    using System.IO;
    using System.Threading;
    using FineCodeCoverage.Engine.Coverlet;
    using NUnit.Framework;

    public class CoverletDataCollectorGeneratedCobertura_Tests
    {
        private DirectoryInfo coverageOutputFolder;

        [SetUp]
        public void CreateCoverageOutputFolder() => this.coverageOutputFolder = this.CreateTemporaryDirectory();

        [TearDown]
        public void DeleteCoverageOutputFolder() => this.coverageOutputFolder.Delete(true);

        private DirectoryInfo CreateTemporaryDirectory()
        {
            var tempDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            return Directory.CreateDirectory(tempDirectory);

        }

        [Test]
        public void Should_Rename_And_Move_The_Generated()
        {
            this.CreateGeneratedFiles();
            var coverletDataCollectorGeneratedCobertura = new CoverletDataCollectorGeneratedCobertura();
            var coverageOutputFile = Path.Combine(this.coverageOutputFolder.FullName, "renamed.xml");
            coverletDataCollectorGeneratedCobertura.CorrectPath(this.coverageOutputFolder.FullName, coverageOutputFile);
            Assert.That(File.ReadAllText(coverageOutputFile), Is.EqualTo("last"));
        }

        [Test]
        public void Should_Delete_The_Generated_Directory()
        {
            var generatedDirectory = this.GetLastDirectoryPath();
            this.CreateGeneratedFiles();
            Assert.That(Directory.Exists(generatedDirectory), Is.True);
            var coverletDataCollectorGeneratedCobertura = new CoverletDataCollectorGeneratedCobertura();
            var coverageOutputFile = Path.Combine(this.coverageOutputFolder.FullName, "renamed.xml");
            coverletDataCollectorGeneratedCobertura.CorrectPath(this.coverageOutputFolder.FullName, coverageOutputFile);
            Assert.That(Directory.Exists(generatedDirectory), Is.False);
        }

        [Test]
        public void Should_Throw_If_Did_Not_Generate()
        {
            var coverletDataCollectorGeneratedCobertura = new CoverletDataCollectorGeneratedCobertura();
            var coverageOutputFile = Path.Combine(this.coverageOutputFolder.FullName, "renamed.xml");
            _ = Assert.Throws<Exception>(
                () => coverletDataCollectorGeneratedCobertura.CorrectPath(
                    this.coverageOutputFolder.FullName,
                    coverageOutputFile
                ),
                "Data collector did not generate coverage.cobertura.xml"
            );

        }

        private string GetLastDirectoryPath() => Path.Combine(this.coverageOutputFolder.FullName, "efgh");

        private void CreateGeneratedFiles()
        {
            var firstDirectory = Path.Combine(this.coverageOutputFolder.FullName, "abcd");
            var lastDirectory = this.GetLastDirectoryPath();

            this.WriteGeneratedCobertura(firstDirectory, false);
            Thread.Sleep(2000);
            this.WriteGeneratedCobertura(lastDirectory, true);
        }

        private void WriteGeneratedCobertura(string directory, bool last)
        {
            _ = Directory.CreateDirectory(directory);
            var generatedPath = Path.Combine(directory, CoverletDataCollectorGeneratedCobertura.collectorGeneratedCobertura);
            File.WriteAllText(generatedPath, last ? "last" : "first");
        }

    }
}
