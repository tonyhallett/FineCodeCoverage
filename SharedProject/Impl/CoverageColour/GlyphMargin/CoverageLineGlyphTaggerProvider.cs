using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Utilities;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text.Tagging;
using FineCodeCoverage.Core.Utilities;
using System.Collections.Generic;
using FineCodeCoverage.Engine.Model;
using FineCodeCoverage.Core.Utilities.VsThreading;

namespace FineCodeCoverage.Impl
{
    [ContentType("code")]
    [TagType(typeof(CoverageLineGlyphTag))]
    [Name(Vsix.TaggerProviderName)]
	[Export(typeof(ITaggerProvider))]
	internal class CoverageLineGlyphTaggerProvider : CoverageLineTaggerProviderBase<CoverageLineGlyphTagger, CoverageLineGlyphTag>
    {
        private readonly ICoverageColoursProvider coverageColoursProvider;
        private readonly RefreshCoverageGlyphsMessage refreshCoverageGlyphsMessage = new RefreshCoverageGlyphsMessage();
        [ImportingConstructor]
		public CoverageLineGlyphTaggerProvider(
            IEventAggregator eventAggregator, 
            ICoverageColoursProvider coverageColoursProvider,
            ICoverageColours coverageColours
        ) : base(eventAggregator)
        {
            this.coverageColoursProvider = coverageColoursProvider;
            coverageColours.ColoursChanged += CoverageColours_ColoursChanged;
        }

        internal IThreadHelper ThreadHelper = new VsThreadHelper();

        private void CoverageColours_ColoursChanged(object sender, System.EventArgs e)
        {
            eventAggregator.SendMessage(refreshCoverageGlyphsMessage);
        }

        protected override void NewCoverageLinesMessageReceived()
        {
            ThreadHelper.JoinableTaskFactory.Run(coverageColoursProvider.PrepareAsync);
        }

        protected override CoverageLineGlyphTagger CreateTagger(ITextBuffer textBuffer, List<CoverageLine> lastCoverageLines)
        {
            return new CoverageLineGlyphTagger(textBuffer, lastCoverageLines);
        }
    }
}