using System;
using System.Collections.Generic;
using System.Text;
using TreeGrid;

namespace FineCodeCoverage.Output
{
    public class ColumnManager : ColumnManagerBase
    {
        public ColumnData FirstColumn { get; } = new ColumnData("First", 0, true, 450);
        public ColumnData SecondColumn { get; } = new ColumnData("Second", 1, true, 120.0, 20);
        public ColumnData ThirdColumn { get; } = new ColumnData("Third", 2, true, 150.0);
        public ColumnManager()
        {
            Columns = new ColumnData[] { FirstColumn, SecondColumn, ThirdColumn };
        }

    }
}
