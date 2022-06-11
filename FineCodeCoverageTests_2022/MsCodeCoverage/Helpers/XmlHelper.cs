namespace FineCodeCoverageTests.MsCodeCoverage_Tests.Helpers
{
    using System.Globalization;
    using System.IO;
    using System.Xml;
    using System.Xml.XPath;

    internal static class XmlHelper
    {
        public const string XmlDeclaration = "<?xml version='1.0' encoding='utf-8'?>";

        public static IXPathNavigable CreateXPathNavigable(string contents)
        {
            var xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(XmlDeclaration + contents);
            return xmlDocument.CreateNavigator();
        }

        public static string DumpXmlContents(this IXPathNavigable xmlPathNavigable)
        {
            var navigator = xmlPathNavigable.CreateNavigator();
            navigator.MoveToRoot();
            using (var stringWriter = new StringWriter(CultureInfo.InvariantCulture))
            {
                navigator.WriteSubtree(new XmlTextWriter(stringWriter)
                {
                    Formatting = Formatting.Indented
                });
                return stringWriter.ToString();
            }
        }
    }
}
