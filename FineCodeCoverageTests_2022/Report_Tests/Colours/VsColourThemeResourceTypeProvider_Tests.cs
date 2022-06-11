namespace FineCodeCoverageTests.ReportTests
{
    using FineCodeCoverage.Core.ReportGenerator.Colours;
    using NUnit.Framework;
    using TestThemeResourceTypes;

    internal class VsColourThemeResourceTypeProvider_Tests
    {
        [Test]
        public void Should_Extract_With_Reflection()
        {
            var vsColourThemeResourceTypeProvider = new VsColourThemeResourceTypeProvider();
            var vsColourThemeResourceTypes = vsColourThemeResourceTypeProvider.Provide(typeof(ThemeColours1).Assembly);

            Assert.That(vsColourThemeResourceTypes, Has.Count.EqualTo(1));
            var themeColours1Type = vsColourThemeResourceTypes[0];
            Assert.Multiple(() =>
            {
                Assert.That(themeColours1Type.Category, Is.EqualTo(ThemeColours1.Category));
                Assert.That(themeColours1Type.Name, Is.EqualTo(nameof(ThemeColours1)));
            });

            Assert.That(themeColours1Type.ColourThemeResourceKeyProperties, Has.Count.EqualTo(2));
            var firstColourThemeResourceKeyProperty = themeColours1Type.ColourThemeResourceKeyProperties[0];
            Assert.Multiple(() =>
            {
                Assert.That(firstColourThemeResourceKeyProperty.PropertyName, Is.EqualTo(nameof(ThemeColours1.AForegroundColourKey)));
                Assert.That(firstColourThemeResourceKeyProperty.ThemeResourceKey, Is.SameAs(ThemeColours1.AForegroundColourKey));
                var secondColourThemeResourceKeyProperty = themeColours1Type.ColourThemeResourceKeyProperties[1];
                Assert.That(secondColourThemeResourceKeyProperty.PropertyName, Is.EqualTo(nameof(ThemeColours1.ABackgroundColourKey)));
                Assert.That(secondColourThemeResourceKeyProperty.ThemeResourceKey, Is.SameAs(ThemeColours1.ABackgroundColourKey));
            });
        }
    }
}
