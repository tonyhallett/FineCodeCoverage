using Microsoft.VisualStudio.Imaging.Interop;
using Microsoft.VisualStudio.Imaging;
using Palmmedia.ReportGenerator.Core.Parser.Analysis;
using System.Linq;
using System.Collections.Generic;

namespace FineCodeCoverage.Output
{
    public class ClassTreeItem : ReportTreeItemBase
    {
        public ClassTreeItem(Class clss)
        {
            this.Name = clss.DisplayName.Split('.').Last();
            IEnumerable<CodeElementTreeItem> codeElements = clss.Files.SelectMany(file => 
                file.CodeElements.Select(codeElement =>
                {
                    IEnumerable<LineVisitStatus> lineVisitStatuses = file.LineVisitStatus.Skip(codeElement.FirstLine)
                        .Take(codeElement.LastLine - codeElement.FirstLine + 1);
                    return new CodeElementTreeItem(codeElement.Name,file.Path,codeElement.FirstLine, codeElement.CodeElementType, lineVisitStatuses)
                    {
                        Parent = this
                    };
                }));

            foreach (CodeElementTreeItem codeElement in codeElements)
            {
                this.observableChildren.Add(codeElement);
                this.CoverableLines += codeElement.CoverableLines;
            }
        }

        public override ImageMoniker ImageMoniker => KnownMonikers.Class;
    }
}
