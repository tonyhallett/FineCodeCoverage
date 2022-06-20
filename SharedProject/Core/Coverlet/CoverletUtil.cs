using System.ComponentModel.Composition;
using System.Threading;
using System.Threading.Tasks;
using FineCodeCoverage.Engine.Model;

namespace FineCodeCoverage.Engine.Coverlet
{

    [Export(typeof(ICoverletUtil))]
    internal class CoverletUtil:ICoverletUtil
	{
        private readonly ICoverletDataCollectorUtil coverletDataCollectorUtil;
        private readonly ICoverletConsoleUtil coverletConsoleUtil;

        [ImportingConstructor]
		public CoverletUtil(ICoverletDataCollectorUtil coverletDataCollectorUtil, ICoverletConsoleUtil coverletConsoleUtil)
        {
            this.coverletDataCollectorUtil = coverletDataCollectorUtil;
            this.coverletConsoleUtil = coverletConsoleUtil;
        }
		//public void Initialize(string appDataFolder,CancellationToken cancellationToken)
		//{
		//	coverletGlobalUtil.Initialize(appDataFolder, cancellationToken);
		//	coverletDataCollectorUtil.Initialize(appDataFolder, cancellationToken);
		//}

		
		public Task RunCoverletAsync(ICoverageProject project, CancellationToken cancellationToken)
		{
            if (coverletDataCollectorUtil.CanUseDataCollector(project))
            {
				return coverletDataCollectorUtil.RunAsync(cancellationToken);
            }
			return coverletConsoleUtil.RunAsync(project, cancellationToken);
		}
	}
}
