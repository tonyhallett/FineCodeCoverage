namespace FineCodeCoverageTests.HostObjectTests
{
    using System.ComponentModel.Composition.Hosting;
    using System.Linq;
    using FineCodeCoverage.Output.HostObjects;
    using NUnit.Framework;

    internal class AllHostObjectsHave
    {
        [Ignore("ReflectionTypeLoad")]
        [Test]
        public void DifferentNonEmptyNames()
        {
            var catalog = new AssemblyCatalog(typeof(IWebViewHostObjectRegistration).Assembly);
            var compositionContainer = new CompositionContainer(catalog);
            var hostObjectRegistrations = compositionContainer.GetExports<IWebViewHostObjectRegistration>().Select(lazyRegistration => lazyRegistration.Value).ToList();
            var names = hostObjectRegistrations.Select(hostObjectRegistration => hostObjectRegistration.Name).ToList();

            Assert.That(names, Is.Not.Empty);
            Assert.Multiple(() =>
            {
                Assert.That(names.All(n => n.Length > 0), Is.True);
                Assert.That(names, Has.Count.EqualTo(names.Distinct().Count()));
            });
        }
    }
}
