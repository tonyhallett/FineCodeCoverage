using System.ComponentModel.Composition;

namespace FineCodeCoverage.Core.Utilities
{
    [Export(typeof(IVsVersion))]
    internal class VsVersion : IVsVersion
    {
        public VsVersion()
        {
#if VS2022
            Is2022 = true;
#endif
        }

        public bool Is2022 { get; }
    }
}
