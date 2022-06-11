namespace FineCodeCoverageTests.CoverageToolOutput_Tests
{
    using System.IO;
    using FineCodeCoverage.Core.Utilities;
    using FineCodeCoverage.Engine;
    using NUnit.Framework;

    internal class SolutionFolderProvider_Tests
    {
        private string tempDirectory;
        private readonly FileUtil fileUtil = new FileUtil();

        [SetUp]
        public void Create_Temp_Directory()
        {
            this.tempDirectory = this.fileUtil.CreateTempDirectory();
            File.WriteAllText(Path.Combine(this.tempDirectory, "my.sln"), "");
        }

        [TearDown]
        public void Delete_Temp_Directories() => this.fileUtil.TryDeleteDirectory(this.tempDirectory);

        [Test]
        public void Should_Work_When_Solution_And_Test_Project_Are_In_Same_Folder()
        {
            var solutionFolderProvider = new SolutionFolderProvider();
            var provided = solutionFolderProvider.Provide(Path.Combine(this.tempDirectory, "my.proj"));

            Assert.That(provided, Is.EqualTo(this.tempDirectory));
        }

        [Test]
        public void Should_Look_up_Directory_Tree()
        {
            var projectDirectory = Directory.CreateDirectory(Path.Combine(this.tempDirectory, "Project"));

            var solutionFolderProvider = new SolutionFolderProvider();
            var provided = solutionFolderProvider.Provide(Path.Combine(projectDirectory.FullName, "my.proj"));

            Assert.That(provided, Is.EqualTo(this.tempDirectory));

        }

        [Test]
        public void Should_Return_Null_When_No_Solution_Directory_Ascendant()
        {
            this.tempDirectory = this.fileUtil.CreateTempDirectory();

            var solutionFolderProvider = new SolutionFolderProvider();
            var provided = solutionFolderProvider.Provide(Path.Combine(this.tempDirectory, "my.proj"));

            Assert.That(provided, Is.Null);
        }
    }
}
