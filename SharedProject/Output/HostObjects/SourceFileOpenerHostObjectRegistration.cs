using EnvDTE80;
using Microsoft;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.ComponentModel.Composition;

namespace FineCodeCoverage.Output.HostObjects
{
    [Export(typeof(IWebViewHostObjectRegistration))]
    public class SourceFileOpenerHostObjectRegistration : IWebViewHostObjectRegistration
    {
        [ImportingConstructor]
        public SourceFileOpenerHostObjectRegistration(
            [Import(typeof(SVsServiceProvider))]
            IServiceProvider serviceProvider,
            ILogger logger
        )
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var dte = (DTE2)serviceProvider.GetService(typeof(SDTE));
            Assumes.Present(dte);

            HostObject = new SourceFileOpener(dte, logger);
        }

        public string Name => "sourceFileOpener";

        public object HostObject { get; }
    }

    
}
