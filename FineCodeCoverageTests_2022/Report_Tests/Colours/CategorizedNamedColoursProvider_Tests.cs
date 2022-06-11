namespace FineCodeCoverageTests.ReportTests
{
    using System.Collections.Generic;
    using FineCodeCoverage.Core.ReportGenerator.Colours;
    using Moq;
    using NUnit.Framework;

    internal class CategorizedNamedColoursProvider_Tests
    {
        [Test]
        public void Should_For_Non_Custom_Populate_From_EnvironmentColors_And_CommonControlsColors()
        {
            CategorizedNamedColoursSpecification categorizedNamedColoursSpecification = null;
            var populatedCategorizedNamedColoursList = new List<CategorizedNamedColours>();
            var mockIVsCategorizedNamedColoursPopulator = new Mock<IVsCategorizedNamedColoursPopulator>();
            _ = mockIVsCategorizedNamedColoursPopulator.Setup(
                vsCategorizedNamedColoursPopulator =>
                vsCategorizedNamedColoursPopulator.Populate(It.IsAny<CategorizedNamedColoursSpecification>())
                )
                .Returns(populatedCategorizedNamedColoursList)
                .Callback<CategorizedNamedColoursSpecification>(
                    callbackCategorizedNamedColoursSpecification => categorizedNamedColoursSpecification = callbackCategorizedNamedColoursSpecification
                );
            var categorizedNamedColoursProvider = new CategorizedNamedColoursProvider(mockIVsCategorizedNamedColoursPopulator.Object);

            var providedCategorizedNamedColoursList = categorizedNamedColoursProvider.Provide();

            Assert.Multiple(() =>
            {
                Assert.That(providedCategorizedNamedColoursList, Is.SameAs(populatedCategorizedNamedColoursList));
                Assert.That(categorizedNamedColoursSpecification.CategorizedNamedColoursList, Is.Null);
                Assert.That(
                    categorizedNamedColoursSpecification.FullCategoryTypes,
                    Is.EqualTo(new List<string> { "EnvironmentColors", "CommonControlsColors" })
                );
            });
        }
    }
}
