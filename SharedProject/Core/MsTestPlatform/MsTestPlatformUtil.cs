using System.IO;
using System.Linq;
using System.ComponentModel.Composition;
using FineCodeCoverage.Core.Initialization.ZippedTools;

namespace FineCodeCoverage.Engine.MsTestPlatform
{
    [Export(typeof(IMsTestPlatformUtil))]
	[Export(typeof(IRequireToolUnzipping))]
	internal class MsTestPlatformUtil : IMsTestPlatformUtil, IRequireToolUnzipping
	{
		public string MsTestPlatformExePath { get; private set; }
		public string ZipDirectoryName { get; } = "msTestPlatform";
		public string ZipPrefix { get; } = "microsoft.testplatform";
		
        public void SetZipDestination(string zipDestination)
        {
			MsTestPlatformExePath = Directory
				.GetFiles(zipDestination, "vstest.console.exe", SearchOption.AllDirectories)
				.FirstOrDefault();
		}
    }
}
