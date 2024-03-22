using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.IO;
using FineCodeCoverage.Core.Utilities;
using FineCodeCoverage.Engine;
using FineCodeCoverage.Engine.Messages;
using Microsoft.VisualStudio.Shell;
using Palmmedia.ReportGenerator.Core.Parser.Analysis;
using TreeGrid;

namespace FineCodeCoverage.Output
{
    class ReportColumnManager : ColumnManagerBase
    {
        public ColumnData Name { get; } = new ColumnData("Name", 0, true, 450);
        public ColumnData CoverableLines { get; } = new ColumnData("Coverable Lines", 1, true, 120.0, 20);
        public ReportColumnManager() => this.Columns = new ColumnData[] { this.Name, this.CoverableLines };
        internal void ShowRelevantHotspotColumns(string usedParser)
        {

        }
    }

    [Export(typeof(ReportViewModel))]
    internal class ReportViewModel : TreeGridViewModelBase<ReportTreeItemBase, ReportColumnManager>, IListener<NewReportMessage>
    {
        //ReportColumnManager to be injected by interface
        // Factory for the specific tree items
        [ImportingConstructor]
        public ReportViewModel(
            IEventAggregator eventAggregator,
            ISourceFileOpener2 sourceFileOpener
        )
        {
            _ = eventAggregator.AddListener(this);
            this.SetItems(this._items);
            this.sourceFileOpener = sourceFileOpener;
        }
        private readonly ObservableCollection<AssemblyTreeItem> _items = new ObservableCollection<AssemblyTreeItem>();
        private readonly ISourceFileOpener2 sourceFileOpener;

        protected override ReportColumnManager ColumnManagerImpl { get; set; } = new ReportColumnManager();

        public void Handle(NewReportMessage message)
        {
            if(message.SummaryResult != null)
            {
                this.ColumnManagerImpl.ShowRelevantHotspotColumns(message.SummaryResult.UsedParser);
                IReadOnlyCollection<Assembly> assemblies = message.SummaryResult.Assemblies;

                _ = ThreadHelper.JoinableTaskFactory.RunAsync(async () =>
                {
                    double firstColumnWidth = this.ColumnManagerImpl.Columns[0].Width.Value;
                    await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                    this._items.Clear();
                    foreach (Assembly assembly in assemblies)
                    {
                        var assemblyTreeItem = new AssemblyTreeItem(assembly);
                        assemblyTreeItem.AdjustWidth(firstColumnWidth);
                        this._items.Add(assemblyTreeItem);
                    }
                });
                
            }
            else
            {
                this._items.Clear();
            }
        }

        public override void Sort(int displayIndex)
        {

        }
        protected override void LeafTreeItemDoubleClick(ReportTreeItemBase treeItem)
        {
            var codeElementTreeItem = treeItem as CodeElementTreeItem;
            if (!IsRelativePath(codeElementTreeItem.FilePath))
            {
                _ = this.sourceFileOpener.OpenAsync(codeElementTreeItem.FilePath, codeElementTreeItem.FileLine);
            }
        }

        public static bool IsRelativePath(string path) => Uri.IsWellFormedUriString(path, UriKind.Relative);
    }
}
