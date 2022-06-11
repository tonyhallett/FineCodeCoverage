namespace FineCodeCoverageTests.ReportTests
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using FineCodeCoverage.Core.ReportGenerator.Colours;
    using Moq;
    using NUnit.Framework;

    internal class ReportColoursProvider_Tests
    {
        private class TestNamedColour : INamedColour
        {
            private readonly bool updatedColour;

            public TestNamedColour(bool updatedColour) => this.updatedColour = updatedColour;
            public Color Colour { get; set; }
            public string JsName { get; set; }

            public IVsColourTheme VsColourTheme { get; set; }

            public int UpdateColourCount { get; private set; }

            public bool UpdateColour(IVsColourTheme vsColourTheme)
            {
                this.VsColourTheme = vsColourTheme;
                this.UpdateColourCount++;
                return this.updatedColour;
            }
        }

        [Test]
        public void Get_Colours_Should_Get_All_CategoryColours_With_VsColourTheme_Applied()
        {
            var testColourName = new TestNamedColour(true);

            var mockVsColourTheme = new Mock<IVsColourTheme>();
            var mockedVsColourTheme = mockVsColourTheme.Object;
            var mockCategorizedNamedColoursProvider = new Mock<ICategorizedNamedColoursProvider>();
            var providedCategorizedNamedColoursList = new List<CategorizedNamedColours>
            {
                new CategorizedNamedColours
                {
                    NamedColours = new List<INamedColour>
                    {
                        testColourName
                    }
                }
            };
            _ = mockCategorizedNamedColoursProvider
                .Setup(categorizedNamedColoursProvider => categorizedNamedColoursProvider.Provide())
                .Returns(providedCategorizedNamedColoursList);
            var reportColoursProvider = new ReportColoursProvider(mockCategorizedNamedColoursProvider.Object, mockedVsColourTheme);

            var categorizedNamedColoursList = reportColoursProvider.GetCategorizedNamedColoursList();

            Assert.Multiple(() =>
            {
                Assert.That(categorizedNamedColoursList, Is.SameAs(providedCategorizedNamedColoursList));
                Assert.That(testColourName.VsColourTheme, Is.SameAs(mockedVsColourTheme));
            });
        }

        [Test]
        public void Should_Update_Colours_When_VsTheme_Changes_Raising_ColoursChanged_Event_If_Any_Colours_Change()
        {
            var testColourName = new TestNamedColour(true);

            var mockVsColourTheme = new Mock<IVsColourTheme>();
            var mockedVsColourTheme = mockVsColourTheme.Object;
            var mockCategorizedNamedColoursProvider = new Mock<ICategorizedNamedColoursProvider>();
            var categorizedNamedColoursList = new List<CategorizedNamedColours>
            {
                new CategorizedNamedColours
                {
                    NamedColours = new List<INamedColour>
                    {
                        testColourName
                    }
                }
            };
            _ = mockCategorizedNamedColoursProvider
                .Setup(categorizedNamedColoursProvider => categorizedNamedColoursProvider.Provide())
                .Returns(categorizedNamedColoursList);
            var reportColoursProvider = new ReportColoursProvider(mockCategorizedNamedColoursProvider.Object, mockedVsColourTheme);
            List<CategorizedNamedColours> changedCategorizedNamedColours = null;
            reportColoursProvider.CategorizedNamedColoursChanged += (sender, changed) =>
                changedCategorizedNamedColours = changed;
            _ = reportColoursProvider.GetCategorizedNamedColoursList();

            mockVsColourTheme.Raise(vsColourTheme => vsColourTheme.ThemeChanged += null, EventArgs.Empty);

            Assert.Multiple(() =>
            {
                Assert.That(changedCategorizedNamedColours, Is.SameAs(categorizedNamedColoursList));
                Assert.That(testColourName.UpdateColourCount, Is.EqualTo(2));
                Assert.That(mockCategorizedNamedColoursProvider.Invocations, Has.Count.EqualTo(1));
            });
        }

        [Test]
        public void Should_Not_Raise_ColoursChanged_Event_When_No_Colours_Change()
        {
            var testColourName = new TestNamedColour(false);

            var mockVsColourTheme = new Mock<IVsColourTheme>();
            var mockedVsColourTheme = mockVsColourTheme.Object;
            var mockCategorizedNamedColoursProvider = new Mock<ICategorizedNamedColoursProvider>();
            var categorizedNamedColoursList = new List<CategorizedNamedColours>
            {
                new CategorizedNamedColours
                {
                    NamedColours = new List<INamedColour>
                    {
                        testColourName
                    }
                }
            };
            _ = mockCategorizedNamedColoursProvider.Setup(
                categorizedNamedColoursProvider => categorizedNamedColoursProvider.Provide()
                ).Returns(categorizedNamedColoursList);
            var reportColoursProvider = new ReportColoursProvider(
                mockCategorizedNamedColoursProvider.Object, mockedVsColourTheme
            );
            List<CategorizedNamedColours> changedCategorizedNamedColours = null;
            reportColoursProvider.CategorizedNamedColoursChanged += (sender, changed) =>
                changedCategorizedNamedColours = changed;

            _ = reportColoursProvider.GetCategorizedNamedColoursList();

            mockVsColourTheme.Raise(vsColourTheme => vsColourTheme.ThemeChanged += null, EventArgs.Empty);

            Assert.That(changedCategorizedNamedColours, Is.Null);
        }

        [Test]
        public void Should_Not_Throw_If_No_CategorizedNamedColoursChanged_Handler()
        {
            var mockVsColourTheme = new Mock<IVsColourTheme>();
            var mockCategorizedNamedColoursProvider = new Mock<ICategorizedNamedColoursProvider>();
            var categorizedNamedColoursList = new List<CategorizedNamedColours>
            {
                new CategorizedNamedColours
                {
                    NamedColours = new List<INamedColour>
                    {
                        new TestNamedColour(true)
                    }
                }
            };
            _ = mockCategorizedNamedColoursProvider
                .Setup(categorizedNamedColoursProvider => categorizedNamedColoursProvider.Provide())
                .Returns(categorizedNamedColoursList);

            var reportColoursProvider = new ReportColoursProvider(
                mockCategorizedNamedColoursProvider.Object, mockVsColourTheme.Object);

            mockVsColourTheme.Raise(vsColourTheme => vsColourTheme.ThemeChanged += null, EventArgs.Empty);

        }
    }
}
