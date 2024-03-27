using System;
using System.ComponentModel.Design;
using FineCodeCoverage.Readme;
using Microsoft.VisualStudio.Shell;
using Task = System.Threading.Tasks.Task;

namespace FineCodeCoverage.Output
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class OpenReadMeCommand
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = PackageIds.cmdidOpenReadMeCommand;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = PackageGuids.guidOutputToolWindowPackageCmdSet;

        private readonly MenuCommand command;
        private readonly IReadMeService readMeService;
        private readonly AsyncPackage package;

        public static OpenReadMeCommand Instance
        {
            get;
            private set;
        }

        public static async Task InitializeAsync(AsyncPackage package, IReadMeService readMeService)
        {
            // Switch to the main thread - the call to AddCommand in OutputToolWindowCommand's constructor requires
            // the UI thread.
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            var commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            Instance = new OpenReadMeCommand(commandService, readMeService,package);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ClearUICommand"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        /// <param name="commandService">Command service to add command to, not null.</param>
        private OpenReadMeCommand(OleMenuCommandService commandService, IReadMeService readMeService, AsyncPackage package)
        {
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

            var menuCommandID = new CommandID(CommandSet, CommandId);
            this.command = new MenuCommand(this.Execute, menuCommandID);
            commandService.AddCommand(this.command);
            this.readMeService = readMeService;
            this.package = package;
        }

        /// <summary>
        /// This function is the callback used to execute the command when the menu item is clicked.
        /// See the constructor to see how the menu item is associated with this function using
        /// OleMenuCommandService service and MenuCommand class.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>
        private void Execute(object sender, EventArgs e)
            //package.ShowToolWindowAsync(typeof(OutputToolWindow), 0, true, package.DisposalToken);
            => this.readMeService.ShowReadMe(package);
    }
}

