using System;
using System.ComponentModel.Composition;
using System.ComponentModel.Design;
using FineCodeCoverage.Core.Utilities;
using FineCodeCoverage.Core.Utilities.FCCVersioning;
using FineCodeCoverage.Output.Pane;
using Microsoft.VisualStudio.Shell;
using Task = System.Threading.Tasks.Task;

namespace FineCodeCoverage.Output
{
    interface IFCCGithubService
    {
        void Execute();
    }

    [Export(typeof(IFCCGithubService))]
    class FCCGithubService : IFCCGithubService
    {
        private readonly IFCCOutputWindowPaneCreator paneCreator;
        private readonly IVsVersion vsVersion;
        private readonly IFCCVersion fccVersion;

        [ImportingConstructor]
        public FCCGithubService(
            IFCCOutputWindowPaneCreator paneCreator,
            IVsVersion vsVersion,
            IFCCVersion fccVersion
        )
        {
            this.paneCreator = paneCreator;
            this.vsVersion = vsVersion;
            this.fccVersion = fccVersion;
        }
        public void Execute()
        {
            ThreadHelper.JoinableTaskFactory.Run(async () =>
            {
                var pane = await paneCreator.GetOrCreateAsync();
                var text = await pane.GetTextAsync();
                var semanticVersion = vsVersion.GetSemanticVersion();
                var releaseVersion = vsVersion.GetReleaseVersion();
                var displayVersion = vsVersion.GetDisplayVersion();
                var editionName = vsVersion.GetEditionName();
                var fccVersion = this.fccVersion.GetVersion();
                var st = "";
            });
        }
    }
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class GithubCommand
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = PackageIds.cmdidGithubCommand;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = PackageGuids.guidOutputToolWindowPackageCmdSet;

        private readonly MenuCommand command;
        private readonly IFCCGithubService fccGithubService;

        public static GithubCommand Instance
        {
            get;
            private set;
        }

        public static async Task InitializeAsync(AsyncPackage package, IFCCGithubService fccGithubService)
        {
            // Switch to the main thread - the call to AddCommand in OutputToolWindowCommand's constructor requires
            // the UI thread.
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            var commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            Instance = new GithubCommand(commandService, fccGithubService);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ClearUICommand"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        /// <param name="commandService">Command service to add command to, not null.</param>
        private GithubCommand(OleMenuCommandService commandService, IFCCGithubService fccGithubService)
        {
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

            var menuCommandID = new CommandID(CommandSet, CommandId);
            this.command = new MenuCommand(this.Execute, menuCommandID);
            commandService.AddCommand(this.command);
            this.fccGithubService = fccGithubService;
        }

        /// <summary>
        /// This function is the callback used to execute the command when the menu item is clicked.
        /// See the constructor to see how the menu item is associated with this function using
        /// OleMenuCommandService service and MenuCommand class.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>
        private void Execute(object sender, EventArgs e)
            => this.fccGithubService.Execute();
    }
}

