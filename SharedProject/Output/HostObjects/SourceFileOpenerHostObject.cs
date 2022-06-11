using EnvDTE;
using EnvDTE80;
using Microsoft;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Runtime.InteropServices;

namespace FineCodeCoverage.Output.HostObjects
{
    [ClassInterface(ClassInterfaceType.AutoDual)]
    [ComVisible(true)]
    public class SourceFileOpenerHostObject
    {
        private DTE2 dte;
        private readonly IServiceProvider serviceProvider;
        private readonly ILogger logger;

        public SourceFileOpenerHostObject(IServiceProvider serviceProvider, ILogger logger)
        {
            this.serviceProvider = serviceProvider;
            this.logger = logger;
        }

        private DTE2 Dte
        {
            get
            {
                if(dte == null)
                {
                    dte = (DTE2)serviceProvider.GetService(typeof(SDTE));
                    Assumes.Present(dte);

                }
                return dte;
            }
        }

#pragma warning disable IDE1006 // Naming Styles
        public void openAtLine(string filePath, int line)
        {
            var ok = TryOpenFiles(new object[] { filePath });
            if (ok && line != 0)
            {
                GotoLine(line);
            }
        }

        public void openFiles(object[] filePaths) // important - string[] does not work
        {
            TryOpenFiles(filePaths);
        }
#pragma warning restore IDE1006 // Naming Styles

        private bool TryOpenFiles(object[] filePaths)
        {
            ActivateMainWindow();

            var ok = false;
            foreach (var filePath in filePaths)
            {
                ok = TryOpenFile(filePath as string);
            }

            return ok;
        }

        // https://docs.microsoft.com/en-us/microsoft-edge/webview2/concepts/threading-model
        // The WebView2 must be created on a UI thread that uses a message pump. All callbacks occur on that thread

        private bool TryOpenFile(string filePath)
        {
            var ok = true;
            try
            {
                OpenFile(filePath);
            }
            catch
            {
                logger.Log($"Unable to open file - {filePath}");
                ok = false;
            }
            return ok;
        }

#pragma warning disable VSTHRD010 // Invoke single-threaded types on Main thread
        private void GotoLine(int line)
        {
            ((TextSelection)Dte.ActiveDocument.Selection).GotoLine(line, false);
        }

        private void ActivateMainWindow()
        {
            Dte.MainWindow.Activate();
        }

        private void OpenFile(string filePath)
        {
#pragma warning disable VSTHRD010 // Invoke single-threaded types on Main thread
            Dte.ItemOperations.OpenFile(filePath, EnvDTE.Constants.vsViewKindCode);

        }
#pragma warning restore VSTHRD010 // Invoke single-threaded types on Main thread

    }
}
