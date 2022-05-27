using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using System.Runtime.InteropServices;
using Task = System.Threading.Tasks.Task;

namespace FineCodeCoverage.Output.HostObjects
{
    [ClassInterface(ClassInterfaceType.AutoDual)]
    [ComVisible(true)]

    public class SourceFileOpener
    {
        private readonly DTE2 dte;
        private readonly ILogger logger;

        public SourceFileOpener(DTE2 dte, ILogger logger)
        {
            this.dte = dte;
            this.logger = logger;
        }
#pragma warning disable VSTHRD200 // Use "Async" suffix for async methods
        public async Task openAtLine(string filePath, int line)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            dte.MainWindow.Activate();
            var ok = TryOpenFile(filePath);
            if (ok && line != 0)
            {
                ((TextSelection)dte.ActiveDocument.Selection).GotoLine(line, false);
            }
        }

        public async Task openFiles(object[] filePaths) // important - string[] does not work
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            dte.MainWindow.Activate();

            foreach (var filePath in filePaths)
            {
                bool ok = TryOpenFile(filePath as string);
                if (!ok)
                {
                    break;
                }
            }
        }

#pragma warning restore VSTHRD200 // Use "Async" suffix for async methods

        private bool TryOpenFile(string filePath)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var ok = true;
            try
            {

                dte.ItemOperations.OpenFile(filePath, EnvDTE.Constants.vsViewKindCode);
            }
            catch
            {
                logger.Log($"Unable to open file - {filePath}");
                ok = false;
            }
            return ok;
        }

    }
}
