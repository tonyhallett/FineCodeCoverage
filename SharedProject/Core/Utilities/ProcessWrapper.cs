using System.ComponentModel.Composition;
using System.Diagnostics.CodeAnalysis;

namespace FineCodeCoverage.Core.Utilities
{
    [Export(typeof(IProcess))]
    [ExcludeFromCodeCoverage]
    internal class ProcessWrapper : IProcess
    {
        public void Start(string fileName)
        {
            System.Diagnostics.Process.Start(fileName);
        }
    }
}