namespace FineCodeCoverage.Output.EnvironmentFont
{
	public class FontDetails
	{
		public FontDetails(double size, string fontFamily)
		{
			Size = size;
			Family = fontFamily;
		}
		public double Size { get; }

		public string Family { get; }
	}
}
