using System;
using System.ComponentModel.Composition;

namespace FineCodeCoverage.Core.Utilities
{
    [Export(typeof(IEnvironment))]
    internal class EnvironmentWrapper : IEnvironment
    {
        public bool Is64BitOperatingSystem => Environment.Is64BitOperatingSystem;
    }
}
