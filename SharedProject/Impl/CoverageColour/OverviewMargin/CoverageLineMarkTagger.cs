using FineCodeCoverage.Core.Utilities;
using FineCodeCoverage.Engine.Model;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;
using System.Collections.Generic;

namespace FineCodeCoverage.Impl
{
	internal class CoverageLineMarkTagger : CoverageLineTaggerBase<OverviewMarkTag>, IListener<CoverageMarginOptionsChangedMessage>
	{
		internal ICoverageMarginOptions _coverageMarginOptions;
		public CoverageLineMarkTagger(ITextBuffer textBuffer, List<CoverageLine> lastCoverageLines, ICoverageMarginOptions coverageMarginOptions) : 
			base(textBuffer, lastCoverageLines)
		{
			this._coverageMarginOptions = coverageMarginOptions;
		}

        public void Handle(CoverageMarginOptionsChangedMessage message)
        {
			_coverageMarginOptions = message.Options;
			RaiseTagsChanged();
        }

        protected override TagSpan<OverviewMarkTag> GetTagSpan(CoverageLine coverageLine, SnapshotSpan span)
		{
			var coverageType = coverageLine.GetCoverageType();
			var shouldShow = _coverageMarginOptions.Show(coverageType);
			if (!shouldShow) return null;

			var newSnapshotSpan = GetLineSnapshotSpan(coverageLine.Line.Number, span);
			return new TagSpan<OverviewMarkTag>(newSnapshotSpan, new OverviewMarkTag(GetMarkKindName(coverageLine)));
		}

		private SnapshotSpan GetLineSnapshotSpan(int lineNumber, SnapshotSpan originalSpan)
		{
			var line = originalSpan.Snapshot.GetLineFromLineNumber(lineNumber - 1);

			var startPoint = line.Start;
			var endPoint = line.End;

			return new SnapshotSpan(startPoint, endPoint);
		}

		private string GetMarkKindName(CoverageLine coverageLine)
		{
			var line = coverageLine.Line;
			var lineHitCount = line?.Hits ?? 0;
			var lineConditionCoverage = line?.ConditionCoverage?.Trim();

			var markKindName = NotCoveredEditorFormatDefinition.ResourceName;

			if (lineHitCount > 0)
			{
				markKindName = CoveredEditorFormatDefinition.ResourceName;

				if (!string.IsNullOrWhiteSpace(lineConditionCoverage) && !lineConditionCoverage.StartsWith("100"))
				{
					markKindName = PartiallyCoveredEditorFormatDefinition.ResourceName;
				}
			}
			return markKindName;
		}
	}
}
