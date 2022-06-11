namespace FineCodeCoverageTests
{
    using NUnit.Framework;
    using Org.XmlUnit.Builder;

    internal static class XmlAssert
    {
        public static void NoXmlDifferences(string actual, string expected)
        {
            var diff = DiffBuilder.Compare(Input.FromString(expected)).WithTest(Input.FromString(actual)).Build();
            Assert.That(diff.HasDifferences(), Is.False);
        }
    }
}
