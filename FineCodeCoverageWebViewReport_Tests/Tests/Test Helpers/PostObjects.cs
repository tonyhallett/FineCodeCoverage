namespace FineCodeCoverageWebViewReport_Tests
{
    using FineCodeCoverage.Output.JsSerialization;
    using FineCodeCoverage.Output.JsSerialization.ReportGenerator;
    using Palmmedia.ReportGenerator.Core.CodeAnalysis;
    using Palmmedia.ReportGenerator.Core.Parser.Analysis;
    using System.Collections.Generic;

    internal static class PostObjects
    {
        public static Report CreateReport(string firstClassName = "Class1") => new Report
        {
            summaryResult = new SummaryResultJson
            {
                assemblies = new List<AssemblyJson>
                    {
                        new AssemblyJson("Assembly", "Assembly", new List<ClassJson>
                        {
                            new ClassJson
                            {
                                name = firstClassName,
                                displayName = firstClassName,
                                assemblyIndex = 0,
                                files = new List<CodeFileJson>
                                {
                                    new CodeFileJson
                                    {
                                        path = "Class1Path"
                                    }
                                }
                            },
                            new ClassJson
                            {
                                name = "Class2",
                                displayName = "Class2",
                                assemblyIndex = 0,
                                files = new List<CodeFileJson>
                                {
                                    new CodeFileJson
                                    {
                                        path = "Class2Path1"
                                    },
                                    new CodeFileJson
                                    {
                                        path = "Class2Path2"
                                    }
                                }
                            }
                        })

                    }
            },
            riskHotspotAnalysisResult = new RiskHotspotAnalysisResultJson(
                    true,
                    new List<RiskHotspotJson> {
                        new RiskHotspotJson(
                            0,1,1,
                            new MethodMetricJson("method","method",123, new List<MetricJson>
                            {
                                new MetricJson(
                                    MetricType.CoverageAbsolute,
                                    MetricMergeOrder.LowerIsBetter,
                                    "",
                                    "ametric",
                                    10
                                )
                            }),
                            new List<MetricStatusJson>
                            {
                                new MetricStatusJson(true,0)
                            }
                        )

                    }
                ),
            riskHotspotsAnalysisThresholds = new RiskHotspotsAnalysisThresholds()
        };

        public static Styling GetStyling()
        {
            var categoryColours = new Dictionary<string, Dictionary<string, string>>()
            {
                {
                    "EnvironmentColors",
                    new Dictionary<string, string>
                    {
                        { "ToolWindowText", "rgb(0,0,0)"},
                        { "ToolWindowBackground", "rgb(0,0,0)"}
                    }
                }
            };

            var styling = new Styling
            {
                fontName = "Arial",
                fontSize = "10px",
                categoryColours = categoryColours
            };

            return styling;
        }

    }


}
