using FineCodeCoverage.Output.HostObjects;
using FineCodeCoverage.Output.JsSerialization;
using FineCodeCoverage.Output.JsSerialization.ReportGenerator;
using FineCodeCoverageWebViewReport.InvocationsRecordingRegistration;
using FineCodeCoverageWebViewReport.JsonPosterRegistration;
using Newtonsoft.Json;
using OpenQA.Selenium;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Support.UI;
using Palmmedia.ReportGenerator.Core.CodeAnalysis;
using Palmmedia.ReportGenerator.Core.Parser.Analysis;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace FineCodeCoverageWebViewReport_Tests
{
    internal static class PostObjects
    {
        public static readonly Report Report = new Report
        {
            summaryResult = new SummaryResultJson
            {
                assemblies = new List<AssemblyJson>
                    {
                        new AssemblyJson("Assembly", "Assembly", new List<ClassJson>
                        {
                            new ClassJson
                            {
                                name = "Class1",
                                displayName = "Class1",
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

    internal static class HostObjects {

        public static void ExecutePostBack(this EdgeDriver edgeDriver,string hostObjectRegistrationName, object objectToPostBack)
        {
            var serialized = JsonConvert.SerializeObject(objectToPostBack);
            edgeDriver.ExecuteScript(
                $"window.chrome.webview.hostObjects.{hostObjectRegistrationName}.{nameof(IHostObject.postBack)}(arguments[0])",
                serialized
            );
        }

        public static List<Invocation> ExecuteGetInvocations(this EdgeDriver edgeDriver, string hostObjectRegistrationName)
        {
            var serialized = edgeDriver.ExecuteScript(
                $"return window.chrome.webview.hostObjects.{hostObjectRegistrationName}.{nameof(InvocationsRecordingHostObject.getInvocations)}()"
            );
            var invocations = JsonConvert.DeserializeObject<List<Invocation>>((string)serialized);
            return invocations;
        }

        public static List<Invocation> GetSourceFileOpenerHostObjectInvocations(this EdgeDriver edgeDriver)
        {
            return edgeDriver.ExecuteGetInvocations(SourceFileOpenerHostObjectRegistration.HostObjectName);
        }

        private static Invocation GetSingleInvocation(List<Invocation> invocations)
        {
            if(invocations.Count > 1)
            {
                throw new Exception("Multiple invocations");
            }
            return invocations[0];
        }

        public static Invocation GetSourceFileOpenerHostObjectInvocation(this EdgeDriver edgeDriver)
        {
            return GetSingleInvocation(edgeDriver.GetSourceFileOpenerHostObjectInvocations());
        }

        public static List<Invocation> GetFCCResourcesNavigatorHostObjectInvocations(this EdgeDriver edgeDriver)
        {
            return edgeDriver.ExecuteGetInvocations(FCCResourcesNavigatorRegistration.HostObjectName);
        }

        public static Invocation GetFCCResourcesNavigatorHostObjectInvocation(this EdgeDriver edgeDriver)
        {
            return GetSingleInvocation(edgeDriver.GetFCCResourcesNavigatorHostObjectInvocations());
        }
    }

    public static class IWebDriverWaitExtensions
    {
        public static T WaitUntil<T>(this IWebDriver webDriver,Func<T> condition, int seconds)
        {
            return new WebDriverWait(webDriver, TimeSpan.FromSeconds(seconds))
                .Until(driver =>
                {
                    return condition();
                });
        }

        public static ReadOnlyCollection<IWebElement> WaitUntilHasElements(
            this IWebDriver webDriver, Func<ReadOnlyCollection<IWebElement>> condition, int seconds)
        {
            return new WebDriverWait(webDriver, TimeSpan.FromSeconds(seconds))
                .Until(driver =>
                {
                    var elements = condition();
                    if (elements.Count() > 0)
                    {
                        return elements;
                    }
                    return null;
                });
        }
    }

    public static class PostStylingWaitForContent
    {
        public static void Do(EdgeDriver edgeDriver)
        {
            edgeDriver.ExecutePostBack(StylingJsonPosterRegistration.RegistrationName, PostObjects.GetStyling());

            edgeDriver.WaitForContent();
        }
    }


}