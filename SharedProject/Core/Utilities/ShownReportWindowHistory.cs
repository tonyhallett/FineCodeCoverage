using FineCodeCoverage.Engine;
using System.ComponentModel.Composition;
using System.IO;

namespace FineCodeCoverage.Core.Utilities
{
    [Export(typeof(IShownReportWindowHistory))]
    internal class ShownReportWindowHistory : IShownReportWindowHistory
    {
        private readonly IFCCEngine fccEngine;
        private readonly IFileUtil fileUtil;
        private bool hasShown;
        private bool checkedFileExists;

        [ImportingConstructor]
        public ShownReportWindowHistory(IFCCEngine fccEngine, IFileUtil fileUtil)
        {
            this.fccEngine = fccEngine;
            this.fileUtil = fileUtil;
        }
        private string ShownReportWindowFilePath => Path.Combine(this.fccEngine.AppDataFolderPath, "outputWindowInitialized");
        public bool HasShown
        {
            get
            {
                if (!this.hasShown && !this.checkedFileExists)
                {
                    this.hasShown = this.fileUtil.Exists(this.ShownReportWindowFilePath);
                    this.checkedFileExists = true;
                }

                return this.hasShown;
            }
        }

        public void Showed()
        {
            if (!this.hasShown)
            {
                this.hasShown = true;
                this.fileUtil.WriteAllText(this.ShownReportWindowFilePath, string.Empty);
            }
        }
    }
}
