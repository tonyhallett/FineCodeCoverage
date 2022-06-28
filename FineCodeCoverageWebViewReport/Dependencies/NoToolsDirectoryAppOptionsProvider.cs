using FineCodeCoverage.Options;
using System;

namespace FineCodeCoverageWebViewReport
{
    internal class NoToolsDirectoryAppOptionsProvider : IAppOptionsProvider
    {
#pragma warning disable 67
        public event Action<IAppOptions> OptionsChanged;
#pragma warning restore 67

        public IAppOptions Provide()
        {
            return new AppOptions();
        }
    }
}
