import { ISortableColumn } from "./columns";
import { HotspotItem } from "./hotspotItem";

export interface ColumnSort {
    columnIdentifier: string;
    ascending: boolean;
}

export function sort(
    itemsToSort: HotspotItem[],
    columns: ISortableColumn<HotspotItem>[],
    columnSort: ColumnSort | undefined
): HotspotItem[] {
    if (columnSort !== undefined) {
        itemsToSort = [...itemsToSort];
        let sortColumn = columns[0];
        let isSortedDescending = false;
        columns.forEach((column) => {
            if (column.columnIdentifier === columnSort.columnIdentifier) {
                column.isSorted = true;
                isSortedDescending = !columnSort.ascending;
                column.isSortedDescending = isSortedDescending;
                sortColumn = column;
            } else {
                column.isSorted = false;
            }
        });
        return sortColumn.sortItems(itemsToSort, isSortedDescending);
    }

    return itemsToSort;
}
