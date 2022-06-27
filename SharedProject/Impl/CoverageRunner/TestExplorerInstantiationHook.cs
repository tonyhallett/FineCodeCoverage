using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.VisualStudio.TestWindow.Extensibility;
using Microsoft.VisualStudio.Utilities;

namespace FineCodeCoverage.Impl
{
    [Name(Vsix.TestContainerDiscovererName)]
    [Export(typeof(ITestContainerDiscoverer))]
    internal class TestExplorerInstantiationHook : ITestContainerDiscoverer
    {
#pragma warning disable 67
        public event EventHandler TestContainersUpdated;
#pragma warning restore 67

        [ExcludeFromCodeCoverage]
        public Uri ExecutorUri => new Uri($"executor://{Vsix.Code}.Executor/v1");
        [ExcludeFromCodeCoverage]
        public IEnumerable<ITestContainer> TestContainers => Enumerable.Empty<ITestContainer>();

        [ImportingConstructor]
        public TestExplorerInstantiationHook
        (
            [ImportMany]
            IEnumerable<ITestInstantiationPathAware> testInstantionPathAwares
        )
        {
            testInstantionPathAwares.ToList()
                .ForEach(testInstantionPathAware => testInstantionPathAware.TestPathInstantion());
        }
    }
}
