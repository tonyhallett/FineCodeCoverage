namespace FineCodeCoverageTests.WebView_Tests
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Threading;
    using System.Windows;
    using System.Windows.Controls;
    using AutoMoq;
    using FineCodeCoverage.Core.ReportGenerator.Colours;
    using FineCodeCoverage.Output.EnvironmentFont;
    using FineCodeCoverage.Output.JsPosting;
    using FineCodeCoverage.Output.JsSerialization;
    using FineCodeCoverage.Output.WebView;
    using Moq;
    using NUnit.Framework;

    internal class StylingJsonPoster_Tests
    {
        private Mock<IJsonPoster> mockJsonPoster;
        private Mock<IReportColoursProvider> mockReportColoursProvider;
        private StylingJsonPoster stylingJsonPoster;
        private TestEnvironmentFont testEnvironmentFont;

        private class TestEnvironmentFont : IEnvironmentFont
        {
            public FrameworkElement FrameworkElement { get; private set; }
            private Action<FontDetails> fontDetailsChangedHandler;

            public void Initialize(
                FrameworkElement frameworkElement,
                Action<FontDetails> fontDetailsChangedHandler
            )
            {
                this.FrameworkElement = frameworkElement;
                this.fontDetailsChangedHandler = fontDetailsChangedHandler;
            }

            public void Change(double size, string fontFamily) =>
                this.fontDetailsChangedHandler(new FontDetails(size, fontFamily));
        }

        private class TestNamedColour : INamedColour
        {
            public Color Colour { get; set; }
            public string JsName { get; set; }

            public bool UpdateColour(IVsColourTheme vsColourTheme) => throw new NotImplementedException();
        }

        [SetUp]
        public void SetUp()
        {
            this.Prepare();
            this.testEnvironmentFont.Change(10, "Arial");
        }

        private void Prepare()
        {
            this.testEnvironmentFont = new TestEnvironmentFont();
            var mocker = new AutoMoqer();
            this.mockReportColoursProvider = mocker.GetMock<IReportColoursProvider>();
            _ = this.mockReportColoursProvider.Setup(
                reportColoursProvider => reportColoursProvider.GetCategorizedNamedColoursList()
            )
            .Returns(new List<CategorizedNamedColours>
            {
                new CategorizedNamedColours
                {
                    Name = "Category1",
                    NamedColours = new List<INamedColour>
                    {
                        new TestNamedColour{ JsName = "Category1Colour1", Colour = Color.FromArgb(1,2,3,4)}
                    }
                }
            });
            var frameworkElement = new Button();
            mocker.SetInstance<IEnvironmentFont>(this.testEnvironmentFont);

            this.mockJsonPoster = new Mock<IJsonPoster>();
            var mockWebViewImpl = new Mock<IWebViewImpl>();
            _ = mockWebViewImpl.SetupGet(webViewImpl => webViewImpl.FrameworkElement).Returns(frameworkElement);

            this.stylingJsonPoster = mocker.Create<StylingJsonPoster>();

            this.stylingJsonPoster.Initialize(this.mockJsonPoster.Object);
            this.stylingJsonPoster.Ready(mockWebViewImpl.Object);
        }

        [Test, Apartment(ApartmentState.STA)]
        public void Should_Have_NotReadyPostBehaviour_As_KeepLast() =>
            Assert.That(this.stylingJsonPoster.NotReadyPostBehaviour, Is.EqualTo(NotReadyPostBehaviour.KeepLast));

        [Test, Apartment(ApartmentState.STA)] // in reality the implementation immediately changes
        public void Should_Not_Post_Until_EnvironmentFont_Changes()
        {
            this.Prepare();
            this.mockJsonPoster.VerifyNoOtherCalls();
        }

        [Test, Apartment(ApartmentState.STA)]
        public void Should_Post_Styling_With_Environment_Font_Details_When_EnvironmentFont_Changes() =>
            this.mockJsonPoster.Verify(jsonPoster => jsonPoster.PostJson(
                this.stylingJsonPoster.Type,
                It.Is<Styling>(styling => styling.fontName == "Arial" && styling.fontSize == "10px"))
            );

        [Test, Apartment(ApartmentState.STA)]
        public void Should_Post_Styling_With_Initial_CategorizedNamedColoursList_When_EnvironmentFont_Changes_Immediately()
        {
            var expectedCategoryColours = new Dictionary<string, Dictionary<string, string>>
            {
                { "Category1", new Dictionary<string,string>{ { "Category1Colour1", "rgba(2,3,4,1)"}} }
            };
            this.mockJsonPoster.Verify(jsonPoster => jsonPoster.PostJson(
                this.stylingJsonPoster.Type,
                Parameter.Is<Styling>().That(styling => styling.categoryColours, Is.EqualTo(expectedCategoryColours))
            ));
        }

        [Test, Apartment(ApartmentState.STA)]
        public void Should_Post_Styling_With_CategorizedNamedColoursList_When_CategorizedNamedColours_Changed()
        {
            var changedCategorizedNamedColours = new List<CategorizedNamedColours>{
                new CategorizedNamedColours
                {
                    Name = "Category1",
                    NamedColours = new List<INamedColour>
                    {
                        new TestNamedColour{ JsName = "Category1Colour1", Colour = Color.FromArgb(2,3,4,5)}
                    }
                }
            };

            this.mockReportColoursProvider.Raise(
                reportColoursProvider => reportColoursProvider.CategorizedNamedColoursChanged += null,
                this.mockReportColoursProvider.Object,
                changedCategorizedNamedColours
            );

            var expectedCategoryColours = new Dictionary<string, Dictionary<string, string>>
            {
                { "Category1", new Dictionary<string,string>{ { "Category1Colour1", "rgba(3,4,5,2)"}} }
            };

            this.mockJsonPoster.Verify(jsonPoster => jsonPoster.PostJson(
                this.stylingJsonPoster.Type,
                Parameter.Is<Styling>().That(styling => styling.categoryColours, Is.EqualTo(expectedCategoryColours))
            ));

        }

        [Test, Apartment(ApartmentState.STA)]
        public void Should_Repost_Styling_When_Refresh()
        {
            this.stylingJsonPoster.Refresh();

            this.mockJsonPoster.AssertReInvokes();
        }
    }
}
