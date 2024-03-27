﻿using System;
using System.ComponentModel.Design;
using System.IO;
using EnvDTE80;
using FineCodeCoverage.Core.Utilities;
using FineCodeCoverage.Engine.Messages;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using SharedProject.Core.CoverageToolOutput;
using Task = System.Threading.Tasks.Task;

namespace FineCodeCoverage.Output
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class OpenCoberturaCommand : IListener<ReportFilesMessage>, IListener<OutdatedOutputMessage>
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = PackageIds.cmdidOpenCoberturaCommand;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = PackageGuids.guidOutputToolWindowPackageCmdSet;

        private readonly DTE2 dte;
        private readonly MenuCommand command;
        private string coberturaFile;

        public static OpenCoberturaCommand Instance
        {
            get;
            private set;
        }

        public static async Task InitializeAsync(AsyncPackage package, IEventAggregator eventAggregator)
        {
            // Switch to the main thread - the call to AddCommand in OpenCoberturaCommand's constructor requires
            // the UI thread.
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            var commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            var dte = ServiceProvider.GlobalProvider.GetService(typeof(SDTE)) as DTE2;
            Instance = new OpenCoberturaCommand(commandService, eventAggregator, dte);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ClearUICommand"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        /// <param name="commandService">Command service to add command to, not null.</param>
        private OpenCoberturaCommand(OleMenuCommandService commandService, IEventAggregator eventAggregator, DTE2 dte)
        {
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

            var menuCommandID = new CommandID(CommandSet, CommandId);
            this.command = new MenuCommand(this.Execute, menuCommandID)
            {
                Enabled = false
            };
            commandService.AddCommand(this.command);
            _ = eventAggregator.AddListener(this);
            this.dte = dte;
        }

        public void Handle(ReportFilesMessage message)
        {
            this.coberturaFile = message.CoberturaFile;
            this.command.Enabled = true;
        }

        /// <summary>
        /// This function is the callback used to execute the command when the menu item is clicked.
        /// See the constructor to see how the menu item is associated with this function using
        /// OleMenuCommandService service and MenuCommand class.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>
        private void Execute(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            if (File.Exists(this.coberturaFile))
            {
                _ = this.dte.ItemOperations.OpenFile(this.coberturaFile, EnvDTE.Constants.vsViewKindPrimary);
            }
        }

        public void Handle(OutdatedOutputMessage message) => this.command.Enabled = false;
    }
}
