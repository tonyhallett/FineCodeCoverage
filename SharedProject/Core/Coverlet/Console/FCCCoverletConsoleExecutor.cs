using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using FineCodeCoverage.Core.Initialization.ZippedTools;
using FineCodeCoverage.Core.Utilities;
using FineCodeCoverage.Engine.Model;

namespace FineCodeCoverage.Engine.Coverlet
{
    public interface ICoverletConsoleExeFinder
    {
        string FindInFolder(string folder, SearchOption searchOption);

    }
    public class CoverletConsoleExeFinder
    {
        public string FindInFolder(string folder, SearchOption searchOption)
        {
            return Directory.GetFiles(folder, "coverlet.exe", searchOption).FirstOrDefault()
                           ?? Directory.GetFiles(folder, "*coverlet*.exe", searchOption).FirstOrDefault();
        }
    }

    [Export(typeof(IFCCCoverletConsoleExecutor))]
    [Export(typeof(IRequireToolUnzipping))]
    internal class FCCCoverletConsoleExecutor : IFCCCoverletConsoleExecutor, IRequireToolUnzipping
    {
        private string coverletExePath;

        public string ZipDirectoryName => "coverlet";

        public string ZipPrefix => "coverlet.console";

        public ExecuteRequest GetRequest(ICoverageProject coverageProject, string coverletSettings)
        {
			return new ExecuteRequest
			{
				FilePath = coverletExePath,
				Arguments = coverletSettings,
				WorkingDirectory = coverageProject.ProjectOutputFolder
			};

		}

        public void SetZipDestination(string zipDestination)
        {
            coverletExePath = Directory.GetFiles(zipDestination, "coverlet.exe", SearchOption.AllDirectories).FirstOrDefault()
                           ?? Directory.GetFiles(zipDestination, "*coverlet*.exe", SearchOption.AllDirectories).FirstOrDefault();
        }
    }
}
