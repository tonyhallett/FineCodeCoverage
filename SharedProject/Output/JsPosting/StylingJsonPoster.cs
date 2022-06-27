using FineCodeCoverage.Core.ReportGenerator.Colours;
using FineCodeCoverage.Output.EnvironmentFont;
using FineCodeCoverage.Output.JsSerialization;
using FineCodeCoverage.Output.WebView;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Windows;

namespace FineCodeCoverage.Output.JsPosting
{
	[Export(typeof(IPostJson))]
	internal class StylingJsonPoster : IPostJson
	{
		private Styling styling;
		private IJsonPoster jsonPoster;
		private readonly IReportColoursProvider reportColoursProvider;
		private readonly IEnvironmentFont environmentFont;
		public const string PostType = "styling";

        public string Type => PostType;

        public NotReadyPostBehaviour NotReadyPostBehaviour => NotReadyPostBehaviour.KeepLast;

        [ImportingConstructor]
		public StylingJsonPoster(IReportColoursProvider reportColoursProvider, IEnvironmentFont environmentFont)
		{
			this.reportColoursProvider = reportColoursProvider;
			this.environmentFont = environmentFont;
		}

		public void Initialize(IJsonPoster jsonPoster)
        {
			this.jsonPoster = jsonPoster;
		}

		public void Ready(IWebViewImpl webViewImpl)
		{
			InitializeStyling(webViewImpl.FrameworkElement);
		}

		public void Refresh()
		{
			PostStyling();
		}

		private void InitializeStyling(FrameworkElement environmentFontElement)
		{
			styling = new Styling
			{
				categoryColours = reportColoursProvider.GetCategorizedNamedColoursList().SerializeAsDictionary()
			};

			environmentFont.Initialize(environmentFontElement, fontDetails =>
			{
				styling.fontName = fontDetails.Family;
				styling.fontSize = $"{fontDetails.Size}px";
				PostStyling();
			});

			reportColoursProvider.CategorizedNamedColoursChanged += ReportColoursProvider_CategorizedNamedColoursChanged;
		}

		private void ReportColoursProvider_CategorizedNamedColoursChanged(object sender, List<CategorizedNamedColours> reportColours)
		{
			styling.categoryColours = reportColours.SerializeAsDictionary();
			PostStyling();
		}

		private void PostStyling()
		{
			this.jsonPoster.PostJson(PostType, styling);
		}

	}

}
