using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Xml.Linq;

namespace FineCodeCoverage.Automation
{
	public static class DependencyPropertiesItemStatusConverter
	{
		public static Dictionary<string, string> ToLookupByDependencyPropertyName(string itemStatus)
		{
			var valuesElement = XElement.Parse(itemStatus);
			return valuesElement.Elements().ToDictionary(
				dpNamedElement => dpNamedElement.Name.ToString(),
				dpNamedElement => dpNamedElement.Element("Value").Value
			);
		}

		public static string ToString(IEnumerable<XElement> xelements)
        {
			var valuesXElement = new XElement("Values", xelements);
			return valuesXElement.ToString();
		}

        internal static string ToString(Dictionary<DependencyProperty, string> values)
        {
            var xelements = values.Select(kvp =>
            {
                return new XElement(kvp.Key.Name, new XElement("Value", kvp.Value));
            });
            var valuesXElement = new XElement("Values", xelements);
            return valuesXElement.ToString();
        }
    }
}
