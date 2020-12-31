using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.IO;
using System.Linq;
using EnvDTE;
using EnvDTE80;
using FineCodeCoverage.Engine;
using FineCodeCoverage.Engine.Cobertura;
using FineCodeCoverage.Engine.Coverlet;
using FineCodeCoverage.Engine.ReportGenerator;
using FineCodeCoverage.Engine.Utilities;
using FineCodeCoverage.Impl;
using Microsoft.VisualStudio.Shell;
using Task = System.Threading.Tasks.Task;

namespace FineCodeCoverage.Output
{
	/// <summary>
	/// Command handler
	/// </summary>
	internal sealed class OutputToolWindowCommand
	{
		/// <summary>
		/// Command ID.
		/// </summary>
		public const int CommandId = 0x0100;

		public const int ProjectCoverageId = 0x0111;
		
		public void ExecuteSolutionVSTestIntegration(object sender, EventArgs e)
		{
            ThreadHelper.ThrowIfNotOnUIThread();
#pragma warning disable VSTHRD002 // Avoid problematic synchronous waits
            var dte = this.package.GetServiceAsync(typeof(DTE)).GetAwaiter().GetResult() as DTE;
#pragma warning restore VSTHRD002 // Avoid problematic synchronous waits
			var solution = dte.Solution;
			var solutionDirectory = Path.GetDirectoryName(solution.FileName);
			var solutionResults = Path.Combine(solutionDirectory, "TestResults");
			var processStartInfo = new ProcessStartInfo
			{
				FileName = "dotnet",
				CreateNoWindow = true,
				UseShellExecute = false,
				RedirectStandardError = true,
				RedirectStandardOutput = true,
				WindowStyle = ProcessWindowStyle.Hidden,
				WorkingDirectory = solutionDirectory,
				Arguments = $"test --results-directory:\"{solutionResults}\" --collect:\"XPlat Code Coverage\"",
			};
			var fileSystemWatcher = new FileSystemWatcher();
			fileSystemWatcher.Path = Path.Combine(solutionDirectory, "TestResults");
			fileSystemWatcher.Filter = "coverage.cobertura.xml";
			fileSystemWatcher.IncludeSubdirectories = true;
			var coberturaFiles = new List<string>();
			fileSystemWatcher.Created += (object s, FileSystemEventArgs fseArgs) =>
			{
				coberturaFiles.Add(fseArgs.FullPath);
			};
			fileSystemWatcher.EnableRaisingEvents = true;
			var process = System.Diagnostics.Process.Start(processStartInfo);
			
			process.WaitForExit();
			var processOutput = process.GetOutput();
			if (process.ExitCode != 0)
			{
				return;
            }
            else
            {

				FCCEngine.CoverageLines.Clear();
				TestContainerDiscoverer.OnUpdateMarginTags(this, null);
				TestContainerDiscoverer.OnUpdateOutputWindow(this, null);
				var _reloadCoverageThread = new System.Threading.Thread(() =>
				{
					ReportGeneratorUtil.RunReportGenerator(coberturaFiles, true, out var unifiedHtmlFile, out var unifiedXmlFile, true);
					var CoverageReport = CoberturaUtil.ProcessCoberturaXmlFile(unifiedXmlFile, out var coverageLines);
					FCCEngine.CoverageLines = coverageLines;
					ReportGeneratorUtil.ProcessUnifiedHtmlFile(unifiedHtmlFile, true, out var coverageHtml);

					TestContainerDiscoverer.OnUpdateMarginTags(this, new UpdateMarginTagsEventArgs());
					TestContainerDiscoverer.OnUpdateOutputWindow(this, new UpdateOutputWindowEventArgs { HtmlContent = File.ReadAllText(coverageHtml) });
				}
				);
				_reloadCoverageThread.Start();

			}

		}

		private IEnumerable<string> GetSelectedProjectDirectories()
		{
			try
			{

#pragma warning disable VSTHRD002 // Avoid problematic synchronous waits
				var dte = package.GetServiceAsync(typeof(DTE)).GetAwaiter().GetResult() as DTE2;
#pragma warning restore VSTHRD002 // Avoid problematic synchronous waits

#pragma warning disable VSTHRD010 // Invoke single-threaded types on Main thread
				ThreadHelper.ThrowIfNotOnUIThread();
				var selectedItems = dte.ToolWindows.SolutionExplorer.SelectedItems as object[];
				return selectedItems.OfType<EnvDTE.UIHierarchyItem>().Select(i => {
					return i.Object;

                }).OfType<EnvDTE.Project>().Select(p => Path.GetDirectoryName(p.FullName));
#pragma warning restore VSTHRD010 // Invoke single-threaded types on Main thread

            }
			catch (Exception ex)
			{
				
			}
			return Enumerable.Empty<string>();
		}

		/*
			Of course this would need more work, including

			Would add for full soln coverage to Soln ( IDM_VS_CTXT_SOLNNODE ) 
			Test menu ( see https://jessehouwing.net/visual-studio-extensibility-id-and-guid-for-menu-item/ ), inside TestExplorer
			
			For project runs would need to add to M_VS_CTXT_PROJNODE IDM_VS_CTXT_XPROJ_MULTIPROJ and conditionally 
			for test projects

			For now to demo just adding to View/OtherWindows 
		*/
		public void ExecuteProjectVSTestIntegration(object sender, EventArgs e)
		{
			FCCEngine.CoverageLines.Clear();
			TestContainerDiscoverer.OnUpdateMarginTags(this, null);
			TestContainerDiscoverer.OnUpdateOutputWindow(this, null);

			ThreadHelper.ThrowIfNotOnUIThread();
#pragma warning disable VSTHRD002 // Avoid problematic synchronous waits
            var dte = this.package.GetServiceAsync(typeof(DTE)).GetAwaiter().GetResult() as DTE;
#pragma warning restore VSTHRD002 // Avoid problematic synchronous waits
            var solution = dte.Solution;
            var solutionDirectory = Path.GetDirectoryName(solution.FileName);
            var solutionResults = Path.Combine(solutionDirectory, "TestResults");

			

			var selectedProjectDirectories = GetSelectedProjectDirectories().ToList();

			
			

            
            
            var _reloadCoverageThread = new System.Threading.Thread(() =>
            {
				var fileSystemWatcher = new FileSystemWatcher();
				fileSystemWatcher.Path = Path.Combine(solutionDirectory, "TestResults");
				fileSystemWatcher.Filter = "coverage.cobertura.xml";
				fileSystemWatcher.IncludeSubdirectories = true;
				var coberturaFiles = new List<string>();
				fileSystemWatcher.Created += (object s, FileSystemEventArgs fseArgs) =>
				{
					coberturaFiles.Add(fseArgs.FullPath);
				};
				fileSystemWatcher.EnableRaisingEvents = true;

				var now = DateTime.Now;

				foreach(var selectedProjectDirectory in selectedProjectDirectories)
                {
					var processStartInfo = new ProcessStartInfo
					{
						FileName = "dotnet",
						CreateNoWindow = true,
						UseShellExecute = false,
						WindowStyle = ProcessWindowStyle.Hidden,
						WorkingDirectory = selectedProjectDirectory,
						Arguments = $"test --results-directory:\"{solutionResults}\" --collect:\"XPlat Code Coverage\"",
					};
					var process = System.Diagnostics.Process.Start(processStartInfo);
					process.WaitForExit();
				}
				
				ReportGeneratorUtil.RunReportGenerator(coberturaFiles, true, out var unifiedHtmlFile, out var unifiedXmlFile, true);
                var CoverageReport = CoberturaUtil.ProcessCoberturaXmlFile(unifiedXmlFile, out var coverageLines);
                FCCEngine.CoverageLines = coverageLines;
                ReportGeneratorUtil.ProcessUnifiedHtmlFile(unifiedHtmlFile, true, out var coverageHtml);

                TestContainerDiscoverer.OnUpdateMarginTags(this, new UpdateMarginTagsEventArgs());
                TestContainerDiscoverer.OnUpdateOutputWindow(this, new UpdateOutputWindowEventArgs { HtmlContent = File.ReadAllText(coverageHtml) });
			}
            );
            _reloadCoverageThread.Start();
        }

		/// <summary>
		/// Command menu group (command set GUID).
		/// </summary>
		public static readonly Guid CommandSet = new Guid("bedda1f3-0d8f-4f8d-a818-0b5523ee662d");

		/// <summary>
		/// VS Package that provides this command, not null.
		/// </summary>
		private readonly AsyncPackage package;

		/// <summary>
		/// Initializes a new instance of the <see cref="OutputToolWindowCommand"/> class.
		/// Adds our command handlers for menu (commands must exist in the command table file)
		/// </summary>
		/// <param name="package">Owner package, not null.</param>
		/// <param name="commandService">Command service to add command to, not null.</param>
		private OutputToolWindowCommand(AsyncPackage package, OleMenuCommandService commandService)
		{
			this.package = package ?? throw new ArgumentNullException(nameof(package));
			commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

			var menuCommandID = new CommandID(CommandSet, CommandId);
			var menuItem = new MenuCommand(this.Execute, menuCommandID);

			
			var projectCoverageCommandID = new CommandID(CommandSet, ProjectCoverageId);
			var projectCoverageCommand = new MenuCommand(ExecuteProjectVSTestIntegration, projectCoverageCommandID);
			commandService.AddCommand(projectCoverageCommand);
			commandService.AddCommand(menuItem);
		}

		/// <summary>
		/// Gets the instance of the command.
		/// </summary>
		public static OutputToolWindowCommand Instance
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets the service provider from the owner package.
		/// </summary>
		public IAsyncServiceProvider ServiceProvider
		{
			get
			{
				return package;
			}
		}

		/// <summary>
		/// Initializes the singleton instance of the command.
		/// </summary>
		/// <param name="package">Owner package, not null.</param>
		public static async Task InitializeAsync(AsyncPackage package)
		{
			// Switch to the main thread - the call to AddCommand in OutputToolWindowCommand's constructor requires
			// the UI thread.
			await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

			OleMenuCommandService commandService = await package.GetServiceAsync((typeof(IMenuCommandService))) as OleMenuCommandService;
			Instance = new OutputToolWindowCommand(package, commandService);
		}

		/// <summary>
		/// Shows the tool window when the menu item is clicked.
		/// </summary>
		/// <param name="sender">The event sender.</param>
		/// <param name="e">The event args.</param>
		public void Execute(object sender, EventArgs e)
		{
			ShowToolWindow();
		}

		public ToolWindowPane ShowToolWindow()
		{
			ToolWindowPane window = null;

			package.JoinableTaskFactory.RunAsync(async delegate
			{
				window = await package.ShowToolWindowAsync(typeof(OutputToolWindow), 0, true, package.DisposalToken);

				if ((null == window) || (null == window.Frame))
				{
					throw new NotSupportedException($"Cannot create '{Vsix.Name}' output window");
				}
			});

			return window;
		}

		public ToolWindowPane FindToolWindow()
		{
			ToolWindowPane window = null;

			package.JoinableTaskFactory.RunAsync(async delegate
			{
				window = await package.FindToolWindowAsync(typeof(OutputToolWindow), 0, true, package.DisposalToken);

				if ((null == window) || (null == window.Frame))
				{
					throw new NotSupportedException($"Cannot create '{Vsix.Name}' output window");
				}
			});

			return window;
		}
	}
}
