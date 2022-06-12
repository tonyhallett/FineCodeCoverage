namespace FineCodeCoverageTests.CoverageProject_Tests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using System.Xml.Linq;
    using FineCodeCoverage;
    using FineCodeCoverage.Core.Utilities;
    using FineCodeCoverage.Engine.Model;
    using FineCodeCoverage.Options;
    using Moq;
    using NUnit.Framework;

    public class CoverageProject_Settings_Tests
    {
        //[Test]
        public void Should_Get_Settings_From_CoverageProjectSettingsManager()
        {

        }

    }

    public class FCCSettingsFilesProvider_Tests
    {
        [Test]
        public void Should_Return_All_FCC_Options_In_Project_Folder_And_Ascendants_Top_Level_First()
        {
            var fccOptionsElements = this.Provide("<Root1></Root1>", "<Root2></Root2>");

            Assert.Multiple(() =>
            {
                Assert.That(fccOptionsElements, Has.Count.EqualTo(2));
                Assert.That(fccOptionsElements[0].Name.ToString(), Is.EqualTo("Root2"));
                Assert.That(fccOptionsElements[1].Name.ToString(), Is.EqualTo("Root1"));
            });
        }

        [Test]
        public void Should_Stop_At_TopLevel()
        {
            var fccOptionsElements = this.Provide("<Root1 topLevel='true'></Root1>", "<Root2></Root2>");
            Assert.That(fccOptionsElements, Has.Count.EqualTo(1));
            Assert.That(fccOptionsElements[0].Name.ToString(), Is.EqualTo("Root1"));
        }

        [Test]
        public void Should_Ignore_Exceptions()
        {
            var fccOptionsElements = this.Provide("<Bad", "<Root2></Root2>");
            Assert.That(fccOptionsElements, Has.Count.EqualTo(1));
            Assert.That(fccOptionsElements[0].Name.ToString(), Is.EqualTo("Root2"));
        }

        private List<XElement> Provide(string projectDirectoryFCCOptions, string solutionParentDirectoryFCCOptions)
        {
            var projectPath = "projectPath";
            var mockFileUtil = new Mock<IFileUtil>();
            var projectDirectoryFCCOptionsPath = Path.Combine(projectPath, FCCSettingsFilesProvider.fccOptionsFileName);
            _ = mockFileUtil.Setup(fileUtil => fileUtil.Exists(projectDirectoryFCCOptionsPath)).Returns(true);
            _ = mockFileUtil.Setup(fileUtil => fileUtil.ReadAllText(projectDirectoryFCCOptionsPath)).Returns(projectDirectoryFCCOptions);

            var solutionPath = "Solution";
            var solutionDirectoryFCCOptionsPath = Path.Combine(solutionPath, FCCSettingsFilesProvider.fccOptionsFileName);
            _ = mockFileUtil.Setup(fileUtil => fileUtil.DirectoryParentPath(projectPath)).Returns(solutionPath);

            // will want a gap where it does not exist
            _ = mockFileUtil.Setup(fileUtil => fileUtil.Exists(solutionDirectoryFCCOptionsPath)).Returns(false);

            var solutionParentPath = "SolutionParent";
            var solutionParentDirectoryFCCOptionsPath = Path.Combine(solutionParentPath, FCCSettingsFilesProvider.fccOptionsFileName);
            _ = mockFileUtil.Setup(fileUtil => fileUtil.DirectoryParentPath(solutionPath)).Returns(solutionParentPath);

            _ = mockFileUtil.Setup(fileUtil => fileUtil.Exists(solutionParentDirectoryFCCOptionsPath)).Returns(true);
            _ = mockFileUtil.Setup(fileUtil => fileUtil.ReadAllText(solutionParentDirectoryFCCOptionsPath)).Returns(solutionParentDirectoryFCCOptions);
            _ = mockFileUtil.Setup(fileUtil => fileUtil.DirectoryParentPath(solutionParentPath)).Returns((string)null);


            var fccOptionsProvider = new FCCSettingsFilesProvider(mockFileUtil.Object);
            return fccOptionsProvider.Provide(projectPath);
        }
    }

    public class CoverageProjectSettingsProvider_Tests
    {
        [Test]
        public async Task Should_Return_The_FineCodeCoverage_Labelled_PropertyGroup_Async()
        {
            var coverageProjectSettingsProvider = new CoverageProjectSettingsProvider(null);
            var mockCoverageProject = new Mock<ICoverageProject>();
            var fccLabelledPropertyGroup = @"
    <PropertyGroup Label='FineCodeCoverage'>
        <Setting1/>
    </PropertyGroup>

";
            var projectFileXElement = XElement.Parse($@"
<Project>
    {fccLabelledPropertyGroup}
</Project>
");
            _ = mockCoverageProject.Setup(cp => cp.ProjectFileXElement).Returns(projectFileXElement);
            var coverageProject = mockCoverageProject.Object;
            var coverageProjectSettings = await coverageProjectSettingsProvider.ProvideAsync(coverageProject);
            XmlAssert.NoXmlDifferences(coverageProjectSettings.ToString(), fccLabelledPropertyGroup);
        }

        [TestCase(true)]
        [TestCase(false)]
        public async Task Should_Return_Using_VsBuild_When_No_Labelled_PropertyGroup_Async(bool returnNull)
        {
            var mockCoverageProject = new Mock<ICoverageProject>();
            var coverageProjectGuid = Guid.NewGuid();
            _ = mockCoverageProject.Setup(cp => cp.Id).Returns(coverageProjectGuid);
            var fccLabelledPropertyGroup = @"
    <PropertyGroup Label='NotFineCodeCoverage'>
    </PropertyGroup>

";
            var projectFileXElement = XElement.Parse($@"
<Project>
    {fccLabelledPropertyGroup}
</Project>
");
            _ = mockCoverageProject.Setup(cp => cp.ProjectFileXElement).Returns(projectFileXElement);

            var mockVsBuildFCCSettingsProvider = new Mock<IVsBuildFCCSettingsProvider>();
            var settingsElementFromVsBuildFCCSettingsProvider = returnNull ? null : new XElement("Root");
            _ = mockVsBuildFCCSettingsProvider.Setup(
                vsBuildFCCSettingsProvider =>
                vsBuildFCCSettingsProvider.GetSettingsAsync(coverageProjectGuid)
            ).ReturnsAsync(settingsElementFromVsBuildFCCSettingsProvider);

            var coverageProjectSettingsProvider = new CoverageProjectSettingsProvider(mockVsBuildFCCSettingsProvider.Object);

            var coverageProject = mockCoverageProject.Object;
            var coverageProjectSettings = await coverageProjectSettingsProvider.ProvideAsync(coverageProject);

            Assert.That(coverageProjectSettings, Is.SameAs(settingsElementFromVsBuildFCCSettingsProvider));
        }
    }

    public class SettingsMerger_Tests
    {
        [Test]
        public void Should_Use_Global_Settings_If_No_Project_Level_Or_FCC_Settings_Files()
        {
            var mockAppOptions = new Mock<IAppOptions>(MockBehavior.Strict);
            var appOptions = mockAppOptions.Object;

            var settingsMerger = new SettingsMerger(null);
            var mergedSettings = settingsMerger.Merge(appOptions, new List<XElement>(), null);

            Assert.That(mergedSettings, Is.SameAs(appOptions));
        }

        [Test]
        public void Should_Overwrite_GlobalOptions_Bool_Properties_From_Settings_File()
        {
            var mockAppOptions = new Mock<IAppOptions>(MockBehavior.Strict);
            _ = mockAppOptions.SetupSet(o => o.IncludeReferencedProjects = true);
            var appOptions = mockAppOptions.Object;

            var settingsMerger = new SettingsMerger(null);
            var settingsFileElement = this.CreateIncludeReferencedProjectsElement(true);
            var mergedSettings = settingsMerger.Merge(appOptions, new List<XElement> { settingsFileElement }, null);

            Assert.That(mergedSettings, Is.SameAs(appOptions));
        }

        [TestCase(true)]
        [TestCase(false)]
        public void Should_Overwrite_GlobalOptions_Bool_Properties_From_Settings_File_In_Order(bool last)
        {
            var mockAppOptions = new Mock<IAppOptions>();
            _ = mockAppOptions.SetupAllProperties();
            var appOptions = mockAppOptions.Object;

            var settingsMerger = new SettingsMerger(null);
            var settingsFileElementTop = this.CreateIncludeReferencedProjectsElement(!last);
            var settingsFileElementLast = this.CreateIncludeReferencedProjectsElement(last);
            var mergedSettings = settingsMerger.Merge(
                appOptions,
                new List<XElement> { settingsFileElementTop, settingsFileElementLast },
                null);

            Assert.Multiple(() =>
            {
                Assert.That(mergedSettings, Is.SameAs(appOptions));
                Assert.That(appOptions.IncludeReferencedProjects, Is.EqualTo(last));
            });
        }

        [TestCase(true)]
        [TestCase(false)]
        public void Should_Overwrite_GlobalOptions_Bool_Properties_From_Project(bool last)
        {
            var mockAppOptions = new Mock<IAppOptions>();
            _ = mockAppOptions.SetupAllProperties();
            var appOptions = mockAppOptions.Object;

            var settingsMerger = new SettingsMerger(null);
            var settingsFileElement = this.CreateIncludeReferencedProjectsElement(!last);
            var projectElement = this.CreateIncludeReferencedProjectsElement(last);
            var mergedSettings = settingsMerger.Merge(
                appOptions,
                new List<XElement> { settingsFileElement },
                projectElement);

            Assert.Multiple(() =>
            {
                Assert.That(mergedSettings, Is.SameAs(appOptions));
                Assert.That(appOptions.IncludeReferencedProjects, Is.EqualTo(last));
            });
        }

        [Test]
        public void Should_Overwrite_Int_Properties()
        {
            var mockAppOptions = new Mock<IAppOptions>();
            _ = mockAppOptions.SetupAllProperties();
            var appOptions = mockAppOptions.Object;

            var intElement = XElement.Parse($@"
<Root>
    <ThresholdForCyclomaticComplexity>123</ThresholdForCyclomaticComplexity>
</Root>
");

            var settingsMerger = new SettingsMerger(null);
            var mergedSettings = settingsMerger.Merge(
                appOptions,
                new List<XElement> { },
                intElement);

            Assert.Multiple(() =>
            {
                Assert.That(mergedSettings, Is.SameAs(appOptions));
                Assert.That(appOptions.ThresholdForCyclomaticComplexity, Is.EqualTo(123));
            });
        }

        [Test]
        public void Should_Overwrite_Enum_Properties()
        {
            var mockAppOptions = new Mock<IAppOptions>();
            _ = mockAppOptions.SetupAllProperties();
            var appOptions = mockAppOptions.Object;

            var enumElement = XElement.Parse($@"
<Root>
    <RunMsCodeCoverage>IfInRunSettings</RunMsCodeCoverage>
</Root>
");

            var settingsMerger = new SettingsMerger(null);
            var mergedSettings = settingsMerger.Merge(
                appOptions,
                new List<XElement> { },
                enumElement);

            Assert.Multiple(() =>
            {
                Assert.That(mergedSettings, Is.SameAs(appOptions));
                Assert.That(appOptions.RunMsCodeCoverage, Is.EqualTo(RunMsCodeCoverage.IfInRunSettings));
            });
        }

        [Test]
        public void Should_Overwrite_String_Properties()
        {
            var mockAppOptions = new Mock<IAppOptions>();
            _ = mockAppOptions.SetupAllProperties();
            var appOptions = mockAppOptions.Object;

            var stringElement = XElement.Parse($@"
<Root>
    <ToolsDirectory>ToolsDirectory</ToolsDirectory>
</Root>
");

            var settingsMerger = new SettingsMerger(null);
            var mergedSettings = settingsMerger.Merge(
                appOptions,
                new List<XElement> { },
                stringElement);

            Assert.Multiple(() =>
            {
                Assert.That(mergedSettings, Is.SameAs(appOptions));
                Assert.That(appOptions.ToolsDirectory, Is.EqualTo("ToolsDirectory"));
            });
        }

        [Test]
        public void Should_Overwrite_String_Array_By_Default()
        {
            var mockAppOptions = new Mock<IAppOptions>();
            _ = mockAppOptions.SetupAllProperties();
            var appOptions = mockAppOptions.Object;
            appOptions.Exclude = new string[] { "global" };
            var stringArrayElement = XElement.Parse($@"
<Root>
  <Exclude>
    1
    2
  </Exclude>
</Root>
");

            var settingsMerger = new SettingsMerger(null);
            var mergedSettings = settingsMerger.Merge(
                appOptions,
                new List<XElement> { },
                stringArrayElement);

            Assert.Multiple(() =>
            {
                Assert.That(mergedSettings, Is.SameAs(appOptions));
                Assert.That(appOptions.Exclude, Is.EqualTo(new string[] { "1", "2" }));
            });
        }

        [Test]
        public void Should_Overwrite_String_Array_DefaultMerge_False()
        {
            var mockAppOptions = new Mock<IAppOptions>();
            _ = mockAppOptions.SetupAllProperties();
            var appOptions = mockAppOptions.Object;
            appOptions.Exclude = new string[] { "global" };
            var stringArrayElement = XElement.Parse($@"
<Root defaultMerge='false'>
  <Exclude>
    1
    2
  </Exclude>
</Root>
");

            var settingsMerger = new SettingsMerger(null);
            var mergedSettings = settingsMerger.Merge(
                appOptions,
                new List<XElement> { },
                stringArrayElement);

            Assert.Multiple(() =>
            {
                Assert.That(mergedSettings, Is.SameAs(appOptions));
                Assert.That(appOptions.Exclude, Is.EqualTo(new string[] { "1", "2" }));
            });
        }

        [Test]
        public void Should_Overwrite_String_Array_DefaultMerge_True_Property_Merge_false()
        {
            var mockAppOptions = new Mock<IAppOptions>();
            _ = mockAppOptions.SetupAllProperties();
            var appOptions = mockAppOptions.Object;
            appOptions.Exclude = new string[] { "global" };
            var stringArrayElement = XElement.Parse($@"
<Root defaultMerge='true'>
  <Exclude merge='false'>
    1
    2
  </Exclude>
</Root>
");

            var settingsMerger = new SettingsMerger(null);
            var mergedSettings = settingsMerger.Merge(
                appOptions,
                new List<XElement> { },
                stringArrayElement);

            Assert.Multiple(() =>
            {
                Assert.That(mergedSettings, Is.SameAs(appOptions));
                Assert.That(appOptions.Exclude, Is.EqualTo(new string[] { "1", "2" }));
            });
        }

        [Test]
        public void Should_Overwrite_String_Array_DefaultMerge_Not_Bool()
        {
            var mockAppOptions = new Mock<IAppOptions>();
            _ = mockAppOptions.SetupAllProperties();
            var appOptions = mockAppOptions.Object;
            appOptions.Exclude = new string[] { "global" };
            var stringArrayElement = XElement.Parse($@"
<Root defaultMerge='xxx'>
  <Exclude>
    1
    2
  </Exclude>
</Root>
");

            var settingsMerger = new SettingsMerger(null);
            var mergedSettings = settingsMerger.Merge(
                appOptions,
                new List<XElement> { },
                stringArrayElement);

            Assert.Multiple(() =>
            {
                Assert.That(mergedSettings, Is.SameAs(appOptions));
                Assert.That(appOptions.Exclude, Is.EqualTo(new string[] { "1", "2" }));
            });
        }

        [Test]
        public void Should_Merge_String_Array_If_DefaultMerge()
        {
            var mockAppOptions = new Mock<IAppOptions>();
            _ = mockAppOptions.SetupAllProperties();
            var appOptions = mockAppOptions.Object;
            appOptions.Exclude = new string[] { "global" };
            var stringArrayElement = XElement.Parse($@"
<Root defaultMerge='true'>
  <Exclude>
    1
    2
  </Exclude>
</Root>
");

            var settingsMerger = new SettingsMerger(null);
            var mergedSettings = settingsMerger.Merge(
                appOptions,
                new List<XElement> { },
                stringArrayElement);

            Assert.Multiple(() =>
            {
                Assert.That(mergedSettings, Is.SameAs(appOptions));
                Assert.That(appOptions.Exclude, Is.EqualTo(new string[] { "global", "1", "2" }));
            });
        }

        [Test]
        public void Should_Merge_If_Property_Element_Merge()
        {
            var mockAppOptions = new Mock<IAppOptions>();
            _ = mockAppOptions.SetupAllProperties();
            var appOptions = mockAppOptions.Object;
            appOptions.Exclude = new string[] { "global" };
            var stringArrayElement = XElement.Parse($@"
<Root defaultMerge='false'>
  <Exclude merge='true'>
    1
    2
  </Exclude>
</Root>
");

            var settingsMerger = new SettingsMerger(null);
            var mergedSettings = settingsMerger.Merge(
                appOptions,
                new List<XElement> { },
                stringArrayElement);

            Assert.Multiple(() =>
            {
                Assert.That(mergedSettings, Is.SameAs(appOptions));
                Assert.That(appOptions.Exclude, Is.EqualTo(new string[] { "global", "1", "2" }));
            });
        }

        [Test]
        public void Should_Not_Throw_If_Merge_Current_Null_String_Array_Type()
        {
            var mockAppOptions = new Mock<IAppOptions>();
            _ = mockAppOptions.SetupAllProperties();
            var appOptions = mockAppOptions.Object;
            appOptions.Exclude = null;
            var stringArrayElement = XElement.Parse($@"
<Root>
  <Exclude merge='true'>
    1
    2
  </Exclude>
</Root>
");

            var settingsMerger = new SettingsMerger(null);
            var mergedSettings = settingsMerger.Merge(
                appOptions,
                new List<XElement> { },
                stringArrayElement);

            Assert.Multiple(() =>
            {
                Assert.That(mergedSettings, Is.SameAs(appOptions));
                Assert.That(appOptions.Exclude, Is.EqualTo(new string[] { "1", "2" }));
            });
        }

        [TestCaseSource(nameof(XmlConversionCases))]
        public void Should_Convert_Xml_Value_Correctly(string propertyElement, string propertyName, object expectedConversion, bool expectedException)
        {
            var settingsMerger = new SettingsMerger(new Mock<ILogger>().Object);
            var settingsElement = XElement.Parse($"<Root>{propertyElement}</Root>");
            var property = typeof(IAppOptions).GetPublicProperties().First(p => p.Name == propertyName);

            var value = settingsMerger.GetValueFromXml(settingsElement, property);
            Assert.That(value, Is.EqualTo(expectedConversion));

        }

        [Test]
        public void Should_Throw_For_Unsupported_Conversion()
        {
            var settingsMerger = new SettingsMerger(new Mock<ILogger>().Object);
            var settingsElement = XElement.Parse($"<Root><PropertyType/></Root>");
            var unsupported = typeof(PropertyInfo).GetProperty(nameof(PropertyInfo.PropertyType));
            var expectedMessage = $"Cannot handle 'PropertyType' yet";
            _ = Assert.Throws<Exception>(() => settingsMerger.GetValueFromXml(settingsElement, unsupported), expectedMessage);
        }

        private static object[] XmlConversionCases()
        {
            string CreateElement(string elementName, string value) => $"<{elementName}>{value}</{elementName}>";
            var hideFullyCovered = nameof(IAppOptions.HideFullyCovered); // bool
            var thresholdForCrapScore = nameof(IAppOptions.ThresholdForCrapScore); // int
            var coverletConsoleCustomPath = nameof(IAppOptions.CoverletConsoleCustomPath); // string
            var exclude = nameof(IAppOptions.Exclude); // string[]
            var enumConversion = nameof(IAppOptions.RunMsCodeCoverage); // enum conversion
            var boolArray = @"
                true
                false
            ";
            var stringArray = @"
                1
                2
            ";
            var cases = new object[]
            {
                // boolean
                new object[]{ CreateElement(hideFullyCovered, "true"),hideFullyCovered,true, false },
                new object[]{ CreateElement(hideFullyCovered, "false"), hideFullyCovered, false, false },
                new object[]{ CreateElement(hideFullyCovered, "bad"), hideFullyCovered, null, false },
                new object[]{ CreateElement(hideFullyCovered, ""), hideFullyCovered, null, false },
                new object[]{ CreateElement(hideFullyCovered, boolArray), hideFullyCovered, true, false },

                // int
                new object[]{ CreateElement(thresholdForCrapScore, "1"), thresholdForCrapScore, 1, false },
                new object[]{ CreateElement(thresholdForCrapScore, "bad"), thresholdForCrapScore, null, false },
                new object[]{ CreateElement(thresholdForCrapScore, ""), thresholdForCrapScore, null, false },

                // string
                new object[]{ CreateElement(coverletConsoleCustomPath, "1"), coverletConsoleCustomPath, "1", false },
                // breaking change ( previous ignored )
                new object[]{ CreateElement(coverletConsoleCustomPath, ""), coverletConsoleCustomPath, "", false },

                // string[] 
                new object[]{ CreateElement(exclude, stringArray), exclude, new string[] { "1","2"}, false },
                new object[]{ CreateElement(exclude, ""), exclude, new string[] {}, false },

                // null for no property element
                new object[]{ CreateElement(exclude, "true"), hideFullyCovered, null, false},

                //exception for no type conversion
                new object[]{ CreateElement(enumConversion, "No"), enumConversion, RunMsCodeCoverage.No, false }

            };

            return cases;
        }


        private XElement CreateIncludeReferencedProjectsElement(bool include) => XElement.Parse($@"
<Root>
    <IncludeReferencedProjects>{include}</IncludeReferencedProjects>
</Root>
");

        //        [Test]
        //        public async Task Should_Prefer_ProjectLevel_From_FCC_Labelled_PropertyGroup_Over_Global()
        //        {
        //            var mockAppOptionsProvider = new Mock<IAppOptionsProvider>();
        //            var mockAppOptions = new Mock<IAppOptions>(MockBehavior.Strict);
        //            mockAppOptions.SetupSet(o => o.ThresholdForCrapScore = 123); // int type
        //            mockAppOptions.SetupSet(o => o.CoverletCollectorDirectoryPath = "CoverletCollectorDirectoryPath"); // string type
        //            
        //            mockAppOptions.SetupSet(o => o.Exclude = new string[] { "1","2"}); // string array
        //            var appOptions = mockAppOptions.Object;
        //            mockAppOptionsProvider.Setup(appOptionsProvider => appOptionsProvider.Get()).Returns(appOptions);

        //            var coverageProjectSettingsManager = new CoverageProjectSettingsManager(
        //                mockAppOptionsProvider.Object,
        //                // does not use if has FineCodeCoverage PropertyGroup with label
        //                new Mock<IVsBuildFCCSettingsProvider>(MockBehavior.Strict).Object,
        //                new Mock<IFCCSettingsFilesProvider>().Object,
        //                new Mock<ISettingsMerger>().Object
        //            );

        //            var mockCoverageProject = new Mock<ICoverageProject>();
        //            var projectFileElement = XElement.Parse(@"
        //<Project>

        //<PropertyGroup Label='FineCodeCoverage'>
        //    <ThresholdForCrapScore>123</ThresholdForCrapScore>
        //    <CoverletCollectorDirectoryPath>CoverletCollectorDirectoryPath</CoverletCollectorDirectoryPath>
        //    <IncludeReferencedProjects>true</IncludeReferencedProjects>
        //    <Exclude>
        //        1
        //        2
        //    </Exclude>
        //</PropertyGroup>
        //</Project>
        //");
        //            mockCoverageProject.Setup(cp => cp.ProjectFileXElement).Returns(projectFileElement);
        //            var coverageProject = mockCoverageProject.Object;
        //            var coverageProjectSettings = await coverageProjectSettingsManager.GetSettingsAsync(coverageProject);
        //            Assert.AreSame(appOptions, coverageProjectSettings);
        //            mockAppOptions.VerifyAll();
        //        }
    }

    public class CoverageProjectSettingsManager_Tests
    {
        [Test]
        public async Task Should_Provide_The_Merged_Result_Using_Global_Options_Async()
        {
            var mockAppOptionsProvider = new Mock<IAppOptionsProvider>();
            var mockAppOptions = new Mock<IAppOptions>();
            var globalOptions = mockAppOptions.Object;
            _ = mockAppOptionsProvider.Setup(appOptionsProvider => appOptionsProvider.Get()).Returns(globalOptions);

            var mockSettingsMerger = new Mock<ISettingsMerger>();
            var mergedSettings = new Mock<IAppOptions>().Object;
            _ = mockSettingsMerger.Setup(settingsMerger =>
                  settingsMerger.Merge(globalOptions, It.IsAny<List<XElement>>(), It.IsAny<XElement>())
            ).Returns(mergedSettings);

            var coverageProjectSettingsManager = new CoverageProjectSettingsManager(
                mockAppOptionsProvider.Object,
                new Mock<ICoverageProjectSettingsProvider>().Object,
                new Mock<IFCCSettingsFilesProvider>().Object,
                mockSettingsMerger.Object
            );

            var coverageProjectSettings = await coverageProjectSettingsManager.GetSettingsAsync(
                new Mock<ICoverageProject>().Object
            );
            Assert.That(coverageProjectSettings, Is.SameAs(mergedSettings));
        }

        [Test]
        public async Task Should_Provide_The_Merged_Result_Using_FCC_Settings_Files_Async()
        {
            var mockCoverageProject = new Mock<ICoverageProject>();
            _ = mockCoverageProject.Setup(cp => cp.ProjectFile).Returns("SomeProject/SomeProject.csproj");

            var mockFCCSettingsFilesProvider = new Mock<IFCCSettingsFilesProvider>();
            var settingsFileElements = new List<XElement>();
            _ = mockFCCSettingsFilesProvider.Setup(
                fccSettingsFilesProvider => fccSettingsFilesProvider.Provide("SomeProject")
            ).Returns(settingsFileElements);

            var mockSettingsMerger = new Mock<ISettingsMerger>();
            var mergedSettings = new Mock<IAppOptions>().Object;
            _ = mockSettingsMerger.Setup(settingsMerger =>
                  settingsMerger.Merge(It.IsAny<IAppOptions>(), settingsFileElements, It.IsAny<XElement>())
            ).Returns(mergedSettings);

            var coverageProjectSettingsManager = new CoverageProjectSettingsManager(
                new Mock<IAppOptionsProvider>().Object,
                new Mock<ICoverageProjectSettingsProvider>().Object,
                mockFCCSettingsFilesProvider.Object,
                mockSettingsMerger.Object
            );


            var coverageProject = mockCoverageProject.Object;
            var coverageProjectSettings = await coverageProjectSettingsManager.GetSettingsAsync(coverageProject);
            Assert.That(coverageProjectSettings, Is.SameAs(mergedSettings));
        }

        [Test]
        public async Task Should_Provide_The_Merged_Result_Using_Project_Settings_Async()
        {
            var coverageProject = new Mock<ICoverageProject>().Object;

            var coverageProjectSettingsElement = new XElement("Root");
            var mockCoverageProjectSettingsProvider = new Mock<ICoverageProjectSettingsProvider>();
            _ = mockCoverageProjectSettingsProvider.Setup(
                coverageProjectSettingsProvider => coverageProjectSettingsProvider.ProvideAsync(coverageProject)
            ).ReturnsAsync(coverageProjectSettingsElement);

            var mockSettingsMerger = new Mock<ISettingsMerger>();
            var mergedSettings = new Mock<IAppOptions>().Object;
            _ = mockSettingsMerger.Setup(settingsMerger =>
                  settingsMerger.Merge(It.IsAny<IAppOptions>(), It.IsAny<List<XElement>>(), coverageProjectSettingsElement)
            ).Returns(mergedSettings);

            var coverageProjectSettingsManager = new CoverageProjectSettingsManager(
                new Mock<IAppOptionsProvider>().Object,
                mockCoverageProjectSettingsProvider.Object,
                new Mock<IFCCSettingsFilesProvider>().Object,
                mockSettingsMerger.Object
            );

            var coverageProjectSettings = await coverageProjectSettingsManager.GetSettingsAsync(coverageProject);
            Assert.That(coverageProjectSettings, Is.SameAs(mergedSettings));
        }
    }
}
