using FineCodeCoverage.Core.ReportGenerator.Colours;
using System.Collections.Generic;
using System.Linq;

namespace FineCodeCoverage.Output.JsSerialization
{
	public static class SerializationExtensions
	{
		public static Dictionary<string, Dictionary<string, string>> SerializeAsDictionary(this List<CategorizedNamedColours> categorizedNamedColoursList)
		{
			var categorizedNamedColoursDictionary = categorizedNamedColoursList.ToDictionary(
				categorizedNamedColours => categorizedNamedColours.Name, 
				categorizedNamedColours =>
				{
					return categorizedNamedColours.NamedColours.ToDictionary(
						namedColour => namedColour.JsName, 
						namedColour =>
						{
							var colour = namedColour.Colour;
							return $"rgba({colour.R},{colour.G},{colour.B},{(float)colour.A / 255})";
						}
					);
				}
			);
			return categorizedNamedColoursDictionary;
		}
	}
}
