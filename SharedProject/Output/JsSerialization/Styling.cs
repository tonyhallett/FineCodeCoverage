using System.Collections.Generic;

namespace FineCodeCoverage.Output.JsSerialization
{
	public class Styling
	{
        
#pragma warning disable IDE1006 // Naming Styles
        public Dictionary<string, Dictionary<string, string>> categoryColours { get; set; }
        public bool themeIsHighContrast;
        public string fontName { get; set; }
		public string fontSize { get; set; }
#pragma warning restore IDE1006 // Naming Styles
	}

}
