using System.Collections.ObjectModel;
using TreeGrid;

namespace FineCodeCoverage.Output
{
    public class TreeGridViewModel : TreeGridViewModelBase<VisualStudioTreeItemBase, ColumnManager>
    {
        public TreeGridViewModel()
        {
            this.columnManagerImpl = new ColumnManager();
            this._items[0].AdjustWidth(this.columnManagerImpl.FirstColumn.Width.Value);
            this.TreeViewAutomationName = "DemoTreeView";
            this.SetItems(this._items);
        }
        private readonly ObservableCollection<TreeItem> _items = new ObservableCollection<TreeItem>
        {
            new TreeItem(
            "Root",
               new TreeItem[]{
                   new TreeItem(
                   "Child",
                   new TreeItem[]{new TreeItem("GC") })})
        };

        public override void Sort(int columnIndex) => this.columnManagerImpl.SortColumns(columnIndex);
    }
}
