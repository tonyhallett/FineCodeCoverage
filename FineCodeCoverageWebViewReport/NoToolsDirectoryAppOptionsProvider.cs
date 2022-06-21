using FineCodeCoverage.Options;
using System;

namespace FineCodeCoverageWebViewReport
{
    internal class NoToolsDirectoryAppOptionsProvider : IAppOptionsProvider
    {
        public event Action<IAppOptions> OptionsChanged;

        public IAppOptions Provide()
        {
            return new AppOptions();
        }
    }
}
