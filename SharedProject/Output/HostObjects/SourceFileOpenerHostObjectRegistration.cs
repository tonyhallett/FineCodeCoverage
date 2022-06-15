using Microsoft.VisualStudio.Shell;
using System;
using System.ComponentModel.Composition;
using FineCodeCoverage.Logging;

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
            HostObject = new SourceFileOpenerHostObject(serviceProvider, logger);
        }

        public const string HostObjectName = "sourceFileOpener";
        public string Name => HostObjectName;

        public object HostObject { get; }
    }

    
}
