using EnvDTE80;
using Microsoft;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.ComponentModel.Composition;
using Task = System.Threading.Tasks.Task;

namespace FineCodeCoverage.Logging
{
    [Export(typeof(IFCCOutputPane))]
    public class FCCOutputPane : IFCCOutputPane
    {
        private Guid fccPaneGuid = Guid.Parse("3B3C775A-0050-445D-9022-0230957805B2");
        private IVsOutputWindowPane _pane;
        private IVsOutputWindow _outputWindow;
        private DTE2 dte;
        private readonly IServiceProvider _serviceProvider;

        [ImportingConstructor]
        public FCCOutputPane(
            [Import(typeof(SVsServiceProvider))]
            IServiceProvider serviceProvider
        )
        {
            this._serviceProvider = serviceProvider;
        }

        public async Task ActivateAsync()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            await TrySetPaneAsync();

            if (_pane != null)
            {
                EnvDTE.Window window = dte.Windows.Item(EnvDTE.Constants.vsWindowKindOutput);
                window.Activate();
                _pane.Activate();
            }
        }
        

        public void OutputString(string outputString)
        {
            ThreadHelper.JoinableTaskFactory.Run(
                () => OutputStringAsync(outputString)
            );
        }

        public async Task OutputStringAsync(string outputString)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            await TrySetPaneAsync();
            if (_pane == null)
            {
                return;
            }
            _pane.OutputStringThreadSafe(outputString);
        }

        private async Task TrySetPaneAsync()
        {
            if (_pane != null)
            {
                return;
            }
            try
            {
                await SetPaneAsync();
            }
            catch { }
        }

        private async Task SetPaneAsync()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            _outputWindow = (IVsOutputWindow)_serviceProvider.GetService(typeof(SVsOutputWindow));
            Assumes.Present(_outputWindow);
            dte = (DTE2)_serviceProvider.GetService(typeof(EnvDTE.DTE));
            Assumes.Present(dte);

            // Create a new pane.
            _outputWindow.CreatePane(
                ref fccPaneGuid,
                "FCC",
                Convert.ToInt32(true),
                Convert.ToInt32(false)); // do not clear with solution otherwise will not get initialize methods

            // Retrieve the new pane.
            _outputWindow.GetPane(ref fccPaneGuid, out IVsOutputWindowPane pane);
            _pane = pane;
        }
    }
}
