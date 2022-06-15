using FineCodeCoverage.Core.Utilities;
using FineCodeCoverage.Output.JsMessages;
using FineCodeCoverage.Output.JsSerialization.ReportGenerator;
using FineCodeCoverage.Output.WebView;
using System.ComponentModel.Composition;

namespace FineCodeCoverage.Output.JsPosting
{
    [Export(typeof(IPostJson))]
	internal class ReportJsonPoster : IPostJson, IListener<NewReportMessage>, IListener<ClearReportMessage>
	{
		private IReport report;
		private IJsonPoster jsonPoster;
        private readonly IReportFactory reportFactory;
		public const string PostType = "report";

		[ImportingConstructor]
		public ReportJsonPoster(IEventAggregator eventAggregator, IReportFactory reportFactory)
		{
			eventAggregator.AddListener(this);
            this.reportFactory = reportFactory;
        }

		public void Ready(IJsonPoster jsonPoster, IWebViewImpl webViewImpl)
		{
			this.jsonPoster = jsonPoster;
		}

		public void Refresh()
		{
			PostReport();
		}

		private void PostReport()
		{
			jsonPoster.PostJson(PostType, report);
		}

		public void Handle(ClearReportMessage message)
		{
			report = null;
			PostReport();
		}

		public void Handle(NewReportMessage message)
		{
			report = reportFactory.Create(
				message.RiskHotspotAnalysisResult, 
				message.RiskHotspotsAnalysisThresholds, 
				message.SummaryResult
			);

			//GenerateStandaloneReport(message.ReportFilePath, report);
			PostReport();
		}
		//this should definitely not be in here
		//private void GenerateStandaloneReport(string reportPath, Report report)
		//{
		//	var reportJson = JsonConvert.SerializeObject(report);
		//	HtmlAgilityPack.HtmlDocument document = new HtmlAgilityPack.HtmlDocument();
		//	document.Load(standaloneReportPath);
		//	var script = document.CreateElement("script");
		//	script.InnerHtml = $"var report={reportJson}";
		//	document.DocumentNode.AppendChild(script); // todo ok ?
		//	document.Save(reportPath);
		//}
	}

}
