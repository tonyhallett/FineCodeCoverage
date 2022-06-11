using System.IO;
using FineCodeCoverage.Core.Model;
using NUnit.Framework;

namespace FineCodeCoverageTests.CoverageProject_Tests
{
    public class ReferencedProject_Tests
    {
        private string tempProjectFilePath;


        [TearDown]
        public void Delete_ProjectFile()
        {
            if (this.tempProjectFilePath != null)
            {
                File.Delete(this.tempProjectFilePath);
            }
        }

        [TestCase(true)]
        [TestCase(false)]
        public void Should_ExcludeFromCodeCoverage_If_Has_Project_Property_FCCExcludeFromCodeCoverage(bool addProperty)
        {
            var property = addProperty ? $"<{ReferencedProject.excludeFromCodeCoveragePropertyName}/>" : "";
            this.WriteProperty(property);
            var referencedProject = new ReferencedProject(this.tempProjectFilePath, "");
            Assert.That(referencedProject.ExcludeFromCodeCoverage, Is.EqualTo(addProperty));

        }
        private void WriteProperty(string property)
        {
            var projectFileXml = $@"
<Project Sdk=""Microsoft.NET.Sdk"">
    <PropertyGroup>
        {property}
    </PropertyGroup>
</Project >
";
            this.tempProjectFilePath = Path.GetTempFileName();
            File.WriteAllText(this.tempProjectFilePath, projectFileXml);
        }
        private ReferencedProject SetUpProjectForAssemblyName(string assemblyName)
        {
            var property = string.IsNullOrEmpty(assemblyName) ? "" : $"<AssemblyName>{assemblyName}</AssemblyName>";
            this.WriteProperty(property);
            return new ReferencedProject(this.tempProjectFilePath);
        }

        [TestCase]
        public void Should_Use_The_AssemblyName_Project_Property_If_AssemblyName_Is_Not_Provided()
        {
            var referencedProject = this.SetUpProjectForAssemblyName("MyAssembly");
            Assert.That(referencedProject.AssemblyName, Is.EqualTo("MyAssembly"));
        }

        [TestCase]
        public void Should_Fallback_AssemblyName_To_The_Project_File_Name()
        {
            var referencedProject = this.SetUpProjectForAssemblyName(null);
            Assert.That(referencedProject.AssemblyName, Is.EqualTo(Path.GetFileNameWithoutExtension(this.tempProjectFilePath)));
        }
    }
}