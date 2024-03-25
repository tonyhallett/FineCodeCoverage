using FineCodeCoverage.Core.Utilities.FCCVersioning;
using FineCodeCoverage.Core.Utilities;
using FineCodeCoverage.Output.Pane;
using System.ComponentModel.Composition;

namespace FineCodeCoverage.Github
{
    [Export(typeof(IFCCGithubService))]
    internal class FCCGithubService : IFCCGithubService
    {
        private readonly IFCCOutputWindowPaneCreator paneCreator;
        private readonly IVsVersion vsVersion;
        private readonly IFCCVersion fccVersion;

        [ImportingConstructor]
        public FCCGithubService(
            IFCCOutputWindowPaneCreator paneCreator,
            IVsVersion vsVersion,
            IFCCVersion fccVersion
        )
        {
            this.paneCreator = paneCreator;
            this.vsVersion = vsVersion;
            this.fccVersion = fccVersion;
        }

        public void Execute()
        {
            new GithubDialogWindow().ShowDialog();
            //ThreadHelper.JoinableTaskFactory.Run(async () =>
            //{
            //    var pane = await paneCreator.GetOrCreateAsync();
            //    var text = await pane.GetTextAsync();
            //    var semanticVersion = vsVersion.GetSemanticVersion();
            //    var releaseVersion = vsVersion.GetReleaseVersion();
            //    var displayVersion = vsVersion.GetDisplayVersion();
            //    var editionName = vsVersion.GetEditionName();
            //    var fccVersion = this.fccVersion.GetVersion();
            //    var st = "";
            //});
        }
    }
}
