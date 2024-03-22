using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.Imaging.Interop;
using Microsoft.VisualStudio.Imaging;
using Palmmedia.ReportGenerator.Core.Parser.Analysis;

namespace FineCodeCoverage.Output
{
    public class CodeElementTreeItem : ReportTreeItemBase
    {
        public CodeElementTreeItem(
            string name, 
            string filePath,
            int startLine,
            CodeElementType codeElementType, 
            IEnumerable<LineVisitStatus> lineVisitStatuses)
        {
            this.Name = name;
            this.FilePath = filePath;
            this.FileLine = startLine;
            this.ImageMoniker = codeElementType == CodeElementType.Method ? KnownMonikers.Method : KnownMonikers.Property;
            this.CoverableLines = lineVisitStatuses.Count(lineVisitStatus => lineVisitStatus != LineVisitStatus.NotCoverable);
        }

        public override ImageMoniker ImageMoniker { get; }
        public int FileLine { get; internal set; }
        public string FilePath { get; internal set; }
    }
}
