﻿using System;
using System.ComponentModel.Design;
using Microsoft.VisualStudio.Shell;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;
using FineCodeCoverage.Core.Utilities;

namespace FineCodeCoverage.Output
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class OpenReportWindowCommand
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = PackageIds.cmdidOpenReportWindowCommand;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = PackageGuids.guidFCCPackageCmdSet;

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly AsyncPackage package;

        private readonly ILogger logger;
        private readonly IShownReportWindowHistory shownReportWindowHistory;

        /// <summary>
        /// Initializes a new instance of the <see cref="OpenReportWindowCommand"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        /// <param name="commandService">Command service to add command to, not null.</param>
        private OpenReportWindowCommand(AsyncPackage package, OleMenuCommandService commandService, ILogger logger, IShownReportWindowHistory shownReportWindowHistory)
        {
            this.logger = logger;
            this.shownReportWindowHistory = shownReportWindowHistory;
            this.package = package ?? throw new ArgumentNullException(nameof(package));
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

            var menuCommandID = new CommandID(CommandSet, CommandId);
            var menuItem = new MenuCommand(this.Execute, menuCommandID);
            commandService.AddCommand(menuItem);
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static OpenReportWindowCommand Instance
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the service provider from the owner package.
        /// </summary>
        public IAsyncServiceProvider ServiceProvider => this.package;

        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static async Task InitializeAsync(AsyncPackage package, ILogger logger, IShownReportWindowHistory shownReportWindowHistory)
        {
            // Switch to the main thread - the call to AddCommand in OpenReportWindowCommand's constructor requires
            // the UI thread.
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            var commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            Instance = new OpenReportWindowCommand(package, commandService, logger, shownReportWindowHistory);
        }

        /// <summary>
        /// Shows the tool window when the menu item is clicked.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event args.</param>
        public void Execute(object sender, EventArgs e)
            => _ = ThreadHelper.JoinableTaskFactory.RunAsync(async () =>
            {
                try
                {
                    _ = await this.ShowToolWindowAsync();
                }
                catch (Exception exception)
                {
                    this.logger.Log(exception);
                }
            });

        public async Task<ToolWindowPane> ShowToolWindowAsync()
        {
            this.shownReportWindowHistory.Showed();
            ToolWindowPane window = await this.package.ShowToolWindowAsync(
                typeof(ReportToolWindow), 0, true, this.package.DisposalToken
            );

            return this.ReturnOrThrowIfCannotCreateToolWindow(window);
        }

        private ToolWindowPane ReturnOrThrowIfCannotCreateToolWindow(ToolWindowPane window) 
            => (null == window) || (null == window.Frame)
                ? throw new NotSupportedException($"Cannot create '{Vsix.Name}' output window")
                : window;
    }
}
