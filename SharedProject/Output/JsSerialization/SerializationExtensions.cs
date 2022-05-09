using FineCodeCoverage.Engine.ReportGenerator;
using System.Collections.Generic;
using System.Linq;

namespace FineCodeCoverage.Output.JsSerialization
{
	public static class SerializationExtensions
	{
		public static Dictionary<string, Dictionary<string, string>> ToDictionary(this List<CategoryColour> categoryColours)
		{
			var categoryColoursDictionary = categoryColours.ToDictionary(categoryColour => categoryColour.Name, categoryColour =>
			{
				return categoryColour.ColourNames.ToDictionary(colourName => colourName.JsName, colourName =>
				{
					var colour = colourName.Color;
					return $"rgba({colour.R},{colour.G},{colour.B},{colour.A})";
				});
			});
			return categoryColoursDictionary;
		}
	}
}
