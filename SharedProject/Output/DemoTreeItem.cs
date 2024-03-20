using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.VisualStudio.Imaging.Interop;
using Microsoft.VisualStudio.Imaging;
using TreeGrid;

namespace FineCodeCoverage.Output
{
    public class TreeItem : VisualStudioTreeItemBase
    {
        private bool _isExpanded;
        private readonly ObservableCollection<ITreeItem> observableChildren = new ObservableCollection<ITreeItem>();
        public TreeItem(string name, IEnumerable<TreeItem> children = null)
        {
            this.Name = name;
            if (children != null)
            {
                foreach (TreeItem child in children)
                {
                    this.observableChildren.Add(child);
                    child.Parent = this;
                }
            }

            this.Children = this.observableChildren;
        }
        private string _name;
        public string Name
        {
            get => this._name;
            set => this.Set<string>(ref this._name, value, nameof(this.Name));
        }

        public ImageMoniker ImageMoniker => KnownMonikers.FolderClosed;

        public override bool IsExpanded
        {
            get => this._isExpanded;
            set
            {
                this.Set<bool>(ref this._isExpanded, value, nameof(this.IsExpanded));
                //if (this.ChildrenLoaded || !value)
                //    return;
                //this.Children.Clear();
                //this.LoadChildren();
                this.AdjustWidth(this._rootWidth);
                //this.ChildrenLoaded = true;
            }
        }

        // crisp image width and margin
        protected override double AdditionalAdjustment => 26;
    }
}
