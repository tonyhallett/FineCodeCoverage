using FineCodeCoverage.Core.ReportGenerator.Colours;
using FineCodeCoverage.Core.Utilities;
using FineCodeCoverage.Options;
using FineCodeCoverage.Output;
using FineCodeCoverage.Output.HostObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FineCodeCoverageWebViewReport
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public class EventAggregator : IEventAggregator
        {
            public IEventSubscriptionManager AddListener(object listener, bool? holdStrongReference = null)
            {
                return null;
            }

            public IEventSubscriptionManager AddListener<T>(IListener<T> listener, bool? holdStrongReference = null)
            {
                throw new NotImplementedException();
            }

            public IEventSubscriptionManager RemoveListener(object listener)
            {
                throw new NotImplementedException();
            }

            public void SendMessage<TMessage>(TMessage message, Action<Action> marshal = null)
            {
                throw new NotImplementedException();
            }

            public void SendMessage<TMessage>(Action<Action> marshal = null) where TMessage : new()
            {
                throw new NotImplementedException();
            }
        }
        public class ReportColoursProvider : IReportColoursProvider
        {
            public event EventHandler<List<CategorizedNamedColours>> CategorizedNamedColoursChanged;

            public class NamedColour : INamedColour
            {
                public System.Drawing.Color Colour { get; set; }
                public string JsName { get; set; }

                public bool UpdateColour(IVsColourTheme vsColourTheme)
                {
                    throw new NotImplementedException();
                }
            }

            public List<CategorizedNamedColours> GetCategorizedNamedColoursList()
            {
                return new List<CategorizedNamedColours>
                {
                    new CategorizedNamedColours
                    {
                        Name = "EnvironmentColors",
                        NamedColours = new List<INamedColour>
                        {
                           new NamedColour
                           {
                               JsName = "ToolWindowText",
                               Colour = System.Drawing.Color.Red
                           },
                           new NamedColour
                           {
                               JsName = "ToolWindowBackground",
                               Colour = System.Drawing.Color.Green

                           }
                        }
                    }
                };
            }
        }

        internal class AppOptions : IAppOptions
        {
            public string[] Exclude { get;set; }
            public string[] ExcludeByAttribute { get;set; }
            public string[] ExcludeByFile { get;set; }
            public string[] Include { get;set; }
            public bool RunInParallel { get;set; }
            public int RunWhenTestsExceed { get;set; }
            public string ToolsDirectory { get;set; }
            public bool RunWhenTestsFail { get;set; }
            public bool RunSettingsOnly { get;set; }
            public bool CoverletConsoleGlobal { get;set; }
            public string CoverletConsoleCustomPath { get;set; }
            public bool CoverletConsoleLocal { get;set; }
            public string CoverletCollectorDirectoryPath { get;set; }
            public string OpenCoverCustomPath { get;set; }
            public string FCCSolutionOutputDirectoryName { get;set; }
            public int ThresholdForCyclomaticComplexity { get;set; }
            public int ThresholdForNPathComplexity { get;set; }
            public int ThresholdForCrapScore { get;set; }
            public bool CoverageColoursFromFontsAndColours { get;set; }
            public bool ShowCoverageInOverviewMargin { get;set; }
            public bool ShowCoveredInOverviewMargin { get;set; }
            public bool ShowUncoveredInOverviewMargin { get;set; }
            public bool ShowPartiallyCoveredInOverviewMargin { get;set; }
            public bool StickyCoverageTable { get;set; }
            public bool NamespacedClasses { get;set; }
            public bool HideFullyCovered { get;set; }
            public bool AdjacentBuildOutput { get;set; }
            public RunMsCodeCoverage RunMsCodeCoverage { get;set; }
            public string[] ModulePathsExclude { get;set; }
            public string[] ModulePathsInclude { get;set; }
            public string[] CompanyNamesExclude { get;set; }
            public string[] CompanyNamesInclude { get;set; }
            public string[] PublicKeyTokensExclude { get;set; }
            public string[] PublicKeyTokensInclude { get;set; }
            public string[] SourcesExclude { get;set; }
            public string[] SourcesInclude { get;set; }
            public string[] AttributesExclude { get;set; }
            public string[] AttributesInclude { get;set; }
            public string[] FunctionsInclude { get;set; }
            public string[] FunctionsExclude { get;set; }
            public bool Enabled { get;set; }
            public bool IncludeTestAssembly { get;set; }
            public bool IncludeReferencedProjects { get;set; }
        }

        internal class AppOptionsProvider : IAppOptionsProvider
        {
            public event Action<IAppOptions> OptionsChanged;

            public IAppOptions Get()
            {
                return new AppOptions();
            }
        }

        public class EnvironmentFont : IEnvironmentFont
        {
            private Action<FontDetails> fontDetailsChangedHandler;
            public void Initialize(FrameworkElement frameworkElement, Action<FontDetails> fontDetailsChangedHandler)
            {
                this.fontDetailsChangedHandler = fontDetailsChangedHandler;
                fontDetailsChangedHandler(new FontDetails(40, "Arial"));
            }
        }

        public MainWindow()
        {
            

            InitializeComponent();

            var outputToolWindowControl = new OutputToolWindowControl(
                new EventAggregator(),
                new ReportColoursProvider(),
                new List<IWebViewHostObjectRegistration>(),
                new AppOptionsProvider(),
                new EnvironmentFont(),
                false
            );

            this.Content = outputToolWindowControl;
        }
    }
}
