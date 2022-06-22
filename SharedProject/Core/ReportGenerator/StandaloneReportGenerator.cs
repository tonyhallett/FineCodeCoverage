using FineCodeCoverage.Core.Initialization;
using FineCodeCoverage.Core.ReportGenerator.Colours;
using FineCodeCoverage.Core.Utilities;
using FineCodeCoverage.Output.JsMessages;
using FineCodeCoverage.Output.JsPosting;
using FineCodeCoverage.Output.JsSerialization;
using FineCodeCoverage.Output.JsSerialization.ReportGenerator;
using FineCodeCoverage.Output.WebView;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FineCodeCoverage.Core.ReportGenerator
{
    [Export(typeof(IRequireInitialization))]
    internal class StandaloneReportGenerator : IRequireInitialization, IListener<NewReportMessage>
    {
        private readonly IReportFactory reportFactory;
        private readonly string standaloneReportPath;
        private class TempNamedColour : INamedColour
        {
            public Color Colour { get; set; }
            public string JsName { get; set; }

            public bool UpdateColour(IVsColourTheme vsColourTheme)
            {
                throw new System.NotImplementedException();
            }
        }
        
        [ImportingConstructor]
        public StandaloneReportGenerator(
            IEventAggregator eventAggregator, 
            IReportFactory reportFactory,
            IReportPathsProvider reportPathsProvider
        )
        {
            standaloneReportPath = reportPathsProvider.Provide().StandalonePath;
            eventAggregator.AddListener(this);
            this.reportFactory = reportFactory;
        }

        public void Handle(NewReportMessage message)
        {
            var report = reportFactory.Create(
                message.RiskHotspotAnalysisResult,
                message.RiskHotspotsAnalysisThresholds,
                message.SummaryResult
            );
            Generate(message.ReportFilePath, report);
        }

        private void Generate(string reportPath, IReport report)
        {
            HtmlAgilityPack.HtmlDocument document = LoadStandaloneReport();
            AddReportVariable(document, report);
            document.Save(reportPath);
        }

        private HtmlAgilityPack.HtmlDocument LoadStandaloneReport()
        {
            HtmlAgilityPack.HtmlDocument document = new HtmlAgilityPack.HtmlDocument();
            document.Load(standaloneReportPath);
            return document;
        }

        private void AddReportVariable(HtmlAgilityPack.HtmlDocument standaloneReport,IReport report)
        {
            var script = CreateScript(standaloneReport, report);
            PrependBody(standaloneReport, script);
        }

        private void PrependBody(HtmlAgilityPack.HtmlDocument standaloneReport, HtmlAgilityPack.HtmlNode node)
        {
            var body = standaloneReport.DocumentNode.Descendants("body").First();
            body.PrependChild(node);
        }

        private HtmlAgilityPack.HtmlNode CreateScript(HtmlAgilityPack.HtmlDocument standaloneReport,IReport report)
        {
            var script = $"{GetStylingVariable()}{GetReportOptionsVariable()}{GetVariable("report",report)}";
            return CreateScript(standaloneReport, script);
        }

        private HtmlAgilityPack.HtmlNode CreateScript(HtmlAgilityPack.HtmlDocument standaloneReport,string script)
        {
            var scriptElement = standaloneReport.CreateElement("script");
            scriptElement.AppendChild(standaloneReport.CreateTextNode(script));
            return scriptElement;
        }

        // temporary - to be determined how proceed 
        private string GetReportOptionsVariable()
        {
            return GetVariable("reportOptions", new ReportOptions());
        }

        private string GetVariable(string variableName, object value)
        {
            return $"var {variableName} = {JsonConvert.SerializeObject(value)};";
        }

        private string GetStylingVariable()
        {
            var categorizedNamedColours = new List<CategorizedNamedColours>
            {
                new CategorizedNamedColours
                {
                    Name = "EnvironmentColors",
                    NamedColours = new List<INamedColour>
                    {
                        new TempNamedColour{JsName="ToolWindowText",Colour = Color.Gray},
                        new TempNamedColour{JsName="ToolWindowBackground",Colour = Color.Black},
                    }
                }
            };
            var styling = new Styling
            {
                fontName = "Arial",
                fontSize = "20px",
                categoryColours = categorizedNamedColours.SerializeAsDictionary()
            };
            return GetVariable("styling", styling);
        }

        public Task InitializeAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
