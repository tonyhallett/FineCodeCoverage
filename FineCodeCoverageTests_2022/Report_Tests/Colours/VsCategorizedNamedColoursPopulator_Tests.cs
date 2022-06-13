namespace FineCodeCoverageTests.ReportTests
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using FineCodeCoverage.Logging;
    using FineCodeCoverage.Core.ReportGenerator.Colours;
    using Microsoft.VisualStudio.PlatformUI;
    using Microsoft.VisualStudio.Shell;
    using Moq;
    using NUnit.Framework;

    internal class VsCategorizedNamedColoursPopulator_Tests
    {
        [Test]
        public void Should_Fulfill_From_Type_Name_Matching()
        {
            var themeResourceKey = new ThemeResourceKey(Guid.NewGuid(), "name", ThemeResourceKeyType.BackgroundColor);
            var vsColourThemeResourceTypes = new List<IVsColourThemeResourceType>
            {
                new TestVsColourThemeResourceType
                {
                    Name = "Category2",
                    ColourThemeResourceKeyProperties = new List<IThemeResourceKeyProperty>
                    {
                        new TestThemeResourceKeyProperty
                        {
                            PropertyName = "ThemeResourceKey1ColorKey",
                            ThemeResourceKey = themeResourceKey
                        }
                    }
                },
                new TestVsColourThemeResourceType
                {
                    Name = "Category1",
                    ColourThemeResourceKeyProperties = new List<IThemeResourceKeyProperty>
                    {

                    }
                }
            };
            var mockVsColourThemeResourceTypeProvider = new Mock<IVsColourThemeResourceTypeProvider>();
            _ = mockVsColourThemeResourceTypeProvider.Setup(
                vsColourThemeResourceTypeProvider => vsColourThemeResourceTypeProvider.Provide(typeof(EnvironmentColors).Assembly)
            ).Returns(vsColourThemeResourceTypes);

            var categorizedNamedColoursSpecification = new CategorizedNamedColoursSpecification
            {
                FullCategoryTypes = new List<string> { "Category1", "Category2" }
            };

            var vsCategorizedNamedColoursPopulator = new VsCategorizedNamedColoursPopulator(mockVsColourThemeResourceTypeProvider.Object, null);

            var populatedCategorizedNamedColoursList = vsCategorizedNamedColoursPopulator.Populate(categorizedNamedColoursSpecification);
            Assert.That(populatedCategorizedNamedColoursList, Has.Count.EqualTo(2));

            INamedColour namedColour = null;
            Assert.Multiple(() =>
            {
                var categorizedNamedColours2 = populatedCategorizedNamedColoursList[1];
                Assert.That(categorizedNamedColours2.Name, Is.EqualTo("Category2"));
                namedColour = categorizedNamedColours2.NamedColours[0];
                Assert.That(namedColour.JsName, Is.EqualTo("ThemeResourceKey1"));
                Assert.That(namedColour.Colour.IsEmpty, Is.True);
            });

            Assert.Multiple(() =>
            {
                var mockVsColourTheme = new Mock<IVsColourTheme>();
                _ = mockVsColourTheme.SetupSequence(vsColourTheme => vsColourTheme.GetThemedColour(themeResourceKey))
                    .Returns(Color.Red).Returns(Color.Blue).Returns(Color.Blue);
                var mockedVsColourTheme = mockVsColourTheme.Object;

                var updatedColour = namedColour.UpdateColour(mockedVsColourTheme);
                Assert.That(updatedColour, Is.True);
                Assert.That(namedColour.Colour, Is.EqualTo(Color.Red));

                updatedColour = namedColour.UpdateColour(mockedVsColourTheme);
                Assert.That(updatedColour, Is.True);
                Assert.That(namedColour.Colour, Is.EqualTo(Color.Blue));

                Assert.That(namedColour.UpdateColour(mockedVsColourTheme), Is.False);
            });
        }

        [Test]
        public void Should_Log_If_Type_Name_Does_Not_Exist()
        {
            var vsColourThemeResourceTypes = new List<IVsColourThemeResourceType>();
            var mockVsColourThemeResourceTypeProvider = new Mock<IVsColourThemeResourceTypeProvider>();
            _ = mockVsColourThemeResourceTypeProvider.Setup(
                vsColourThemeResourceTypeProvider => vsColourThemeResourceTypeProvider.Provide(typeof(EnvironmentColors).Assembly)
            ).Returns(vsColourThemeResourceTypes);

            var categorizedNamedColoursSpecification = new CategorizedNamedColoursSpecification
            {
                FullCategoryTypes = new List<string> { "MissingCategory" }
            };

            var mockLogger = new Mock<ILogger>();
            var vsCategorizedNamedColoursPopulator = new VsCategorizedNamedColoursPopulator(
                mockVsColourThemeResourceTypeProvider.Object,
                mockLogger.Object
            );

            _ = vsCategorizedNamedColoursPopulator.Populate(categorizedNamedColoursSpecification);

            mockLogger.Verify(logger => logger.Log("Cannot find vs colours category type MissingCategory"));
        }
    }


    internal class TestThemeResourceKeyProperty : IThemeResourceKeyProperty
    {
        public bool IsColour { get; set; }
        public string PropertyName { get; set; }
        public ThemeResourceKey ThemeResourceKey { get; set; }
    }

    internal class TestVsColourThemeResourceType : IVsColourThemeResourceType
    {
        public Guid Category { get; set; }

        public List<IThemeResourceKeyProperty> ColourThemeResourceKeyProperties { get; set; }

        public string Name { get; set; }
    }
}
