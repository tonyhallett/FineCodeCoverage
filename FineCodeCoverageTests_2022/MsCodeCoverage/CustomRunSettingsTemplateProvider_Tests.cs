namespace FineCodeCoverageTests.MsCodeCoverage_Tests
{
    using System;
    using System.IO;
    using AutoMoq;
    using FineCodeCoverage.Core.Utilities;
    using FineCodeCoverage.Engine.MsTestPlatform.CodeCoverage;
    using NUnit.Framework;

    internal class CustomRunSettingsTemplateProvider_Tests
    {
        private AutoMoqer autoMocker;
        private CustomRunSettingsTemplateProvider customRunSettingsTemplateProvider;
        private const string TemplateName = "fcc-ms-runsettings-template.xml";
        private const string ProjectDirectory = "ProjectDirectory";
        private const string SolutionDirectory = "SolutionDirectory";
        private readonly string templateInProjectDirectory = Path.Combine(ProjectDirectory, TemplateName);
        private readonly string templateInSolutionDirectory = Path.Combine(SolutionDirectory, TemplateName);


        [Flags]
        internal enum TemplateIn { None, ProjectDir, SolutionDir }

        [SetUp]
        public void Setup()
        {
            this.autoMocker = new AutoMoqer();
            this.customRunSettingsTemplateProvider = this.autoMocker.Create<CustomRunSettingsTemplateProvider>();
        }

        [TestCase(TemplateIn.ProjectDir)]
        [TestCase(TemplateIn.ProjectDir | TemplateIn.SolutionDir)]
        public void Should_Return_Non_Null_If_Found_In_The_Project_Directory(TemplateIn templateIn)
        {
            this.SetupFileUtil(templateIn);

            var results = this.customRunSettingsTemplateProvider.Provide(ProjectDirectory, SolutionDirectory);
            Assert.Multiple(() =>
            {
                Assert.That(results.Template, Is.EqualTo("ProjectRunSettings"));
                Assert.That(results.Path, Is.EqualTo(this.templateInProjectDirectory));
            });
        }

        [Test]
        public void Should_Return_Non_Null_If_Found_In_The_Solution_Directory()
        {
            this.SetupFileUtil(TemplateIn.SolutionDir);

            var results = this.customRunSettingsTemplateProvider.Provide(ProjectDirectory, SolutionDirectory);
            Assert.Multiple(() =>
            {
                Assert.That(results.Template, Is.EqualTo("SolutionRunSettings"));
                Assert.That(results.Path, Is.EqualTo(this.templateInSolutionDirectory));
            });
        }

        [Test]
        public void Should_Return_Null_If_Not_Found()
        {
            this.SetupFileUtil(TemplateIn.None);

            var results = this.customRunSettingsTemplateProvider.Provide(ProjectDirectory, SolutionDirectory);
            Assert.That(results, Is.Null);
        }

        [Test]
        public void Should_Not_Throw_For_Null_Directory()
        {
            this.SetupFileUtil(TemplateIn.ProjectDir);

            var results = this.customRunSettingsTemplateProvider.Provide(null, null);
            Assert.That(results, Is.Null);
        }

        private void SetupFileUtil(TemplateIn templateIn)
        {
            var mockFileUtil = this.autoMocker.GetMock<IFileUtil>();
            _ = mockFileUtil.Setup(f => f.Exists(this.templateInProjectDirectory))
                .Returns(templateIn.HasFlag(TemplateIn.ProjectDir));
            _ = mockFileUtil.Setup(f => f.Exists(this.templateInSolutionDirectory))
                .Returns(templateIn.HasFlag(TemplateIn.SolutionDir));
            _ = mockFileUtil.Setup(f => f.ReadAllText(this.templateInProjectDirectory))
                .Returns("ProjectRunSettings");
            _ = mockFileUtil.Setup(f => f.ReadAllText(this.templateInSolutionDirectory))
                .Returns("SolutionRunSettings");
        }
    }
}
