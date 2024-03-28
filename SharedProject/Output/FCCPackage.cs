using System;
using System.Threading;
using FineCodeCoverage.Options;
using Microsoft.VisualStudio.Shell;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft;
using FineCodeCoverage.Engine;
using FineCodeCoverage.Core.Utilities;
using FineCodeCoverage.Initialization;
using FineCodeCoverage.Output.Pane;
using FineCodeCoverage.Github;
using FineCodeCoverage.Readme;
using System.Linq;
using System.Reflection;
using Microsoft.VisualStudio.ComponentModelHost;

namespace FineCodeCoverage.Output
{
    /// <summary>
    /// This is the class that implements the package exposed by this assembly.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The minimum requirement for a class to be considered a valid package for Visual Studio
    /// is to implement the IVsPackage interface and register itself with the shell.
    /// This package uses the helper classes defined inside the Managed Package Framework (MPF)
    /// to do it: it derives from the Package class that provides the implementation of the
    /// IVsPackage interface and uses the registration attributes defined in the framework to
    /// register itself and its components with the shell. These attributes tell the pkgdef creation
    /// utility what data to put into .pkgdef file.
    /// </para>
    /// <para>
    /// To get loaded into VS, the package must be referred by &lt;Asset Type="Microsoft.VisualStudio.VsPackage" ...&gt; in .vsixmanifest file.
    /// </para>
    /// </remarks>
    [ProvideBindingPath]
	[Guid(PackageGuids.guidFCCPackageString)]
	[ProvideMenuResource("Menus.ctmenu", 1)]
	[InstalledProductRegistration(Vsix.Name, Vsix.Description, Vsix.Id)]
	[ProvideOptionPage(typeof(AppOptionsPage), Vsix.Name, "General", 0, 0, true)]
    [ProvideProfile(typeof(AppOptionsPage), Vsix.Name, Vsix.Name, 101, 102, true)]
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
	[ProvideToolWindow(typeof(ReportToolWindow), Style = VsDockStyle.Tabbed, DockedHeight = 300, Window = EnvDTE.Constants.vsWindowKindOutput)]
	[ProvideToolWindow(typeof(ReadmeToolWindow),Orientation =ToolWindowOrientation.Right, Style = VsDockStyle.Tabbed, Width = 600, Height = 700)]
    public sealed class FCCPackage
        : AsyncPackage
	{
		private static IComponentModel componentModel;
        private IFCCEngine fccEngine;

		/// <summary>
		/// Initializes a new instance of the <see cref="OutputToolWindowPackage"/> class.
		/// </summary>
		public FCCPackage()
		{
			// Inside this method you can place any initialization code that does not require
			// any Visual Studio service because at this point the package object is created but
			// not sited yet inside Visual Studio environment. The place to do all the other
			// initialization is the Initialize method.
		}

        internal static readonly MethodInfo GetServiceMethod = typeof(IComponentModel).GetMethod("GetService");
        
        internal static object GetToolWindowContext(Type toolWindowType)
        {
            ConstructorInfo contextConstructor = toolWindowType.GetConstructors().Where(c => c.GetParameters().Length == 1).First();
            Type contextType = contextConstructor.GetParameters().First().ParameterType;
            object context = Activator.CreateInstance(contextType);
            foreach (PropertyInfo contextProperty in contextType.GetProperties())
            {
                Type propertyType = contextProperty.PropertyType;
                MethodInfo getService = GetServiceMethod.MakeGenericMethod(propertyType);
                contextProperty.SetValue(context, getService.Invoke(componentModel, new object[] { }));
            }

            return context;
        }
        
        /*
            Hack necessary for debugging in 2022 !
            https://developercommunity.visualstudio.com/t/vsix-tool-window-vs2022-different-instantiation-wh/1663280
        */
        internal static TContext GetToolWindowContext<TToolWindowType,TContext>() => (TContext)GetToolWindowContext(typeof(TToolWindowType));

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token to monitor for initialization cancellation, which can occur when VS is shutting down.</param>
        /// <param name="progress">A provider for progress updates.</param>
        /// <returns>A task representing the async work of package initialization, or an already completed task if there is none. Do not return null from this method.</returns>
        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
		{
			// When initialized asynchronously, the current thread may be a background thread at this point.
			// Do any initialization that requires the UI thread after switching to the UI thread.
			await this.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
			// you cannot MEF import in the constructor of the package
			componentModel = await this.GetServiceAsync(typeof(Microsoft.VisualStudio.ComponentModelHost.SComponentModel)) as Microsoft.VisualStudio.ComponentModelHost.IComponentModel;
			Assumes.Present(componentModel);
            await this.InitializeCommandsAsync(componentModel);
			await componentModel.GetService<IInitializer>().InitializeAsync(cancellationToken);
        }

        private async Task InitializeCommandsAsync(IComponentModel componentModel)
        {
            this.fccEngine = componentModel.GetService<IFCCEngine>();
            IEventAggregator eventAggregator = componentModel.GetService<IEventAggregator>();
            await OpenCoberturaCommand.InitializeAsync(this, eventAggregator);
            await OpenHotspotsCommand.InitializeAsync(this, eventAggregator);
            await ClearUICommand.InitializeAsync(this, this.fccEngine);
            await OpenFCCOutputPaneCommand.InitializeAsync(this, componentModel.GetService<IShowFCCOutputPane>());
            await OpenSettingsCommand.InitializeAsync(this);
            await OpenMarketplaceRateAndReviewCommand.InitializeAsync(this, componentModel.GetService<IOpenFCCVsMarketplace>());
            await OpenFCCGithubCommand.InitializeAsync(this, componentModel.GetService<IFCCGithubService>());
            await NewIssueCommand.InitializeAsync(this, componentModel.GetService<IFCCGithubService>());
            await OpenReadMeCommand.InitializeAsync(this, componentModel.GetService<IReadMeService>());
            await OpenReportWindowCommand.InitializeAsync(
                this,
                componentModel.GetService<ILogger>(),
                componentModel.GetService<IShownReportWindowHistory>()
            );
        }

        protected override Task<object> InitializeToolWindowAsync(Type toolWindowType, int id, CancellationToken cancellationToken) 
            => Task.FromResult<object>(GetToolWindowContext(toolWindowType));

        public override IVsAsyncToolWindowFactory GetAsyncToolWindowFactory(Guid toolWindowType) => (toolWindowType == typeof(ReportToolWindow).GUID) ? this : null;

        protected override string GetToolWindowTitle(Type toolWindowType, int id) 
            => toolWindowType == typeof(ReportToolWindow) ? $"{Vsix.Name} loading" : base.GetToolWindowTitle(toolWindowType, id);
    }
}
