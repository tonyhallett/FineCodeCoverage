import { ISortableColumn } from "./columns";
import { HotspotItem } from "./hotspotItem";

// to type
export function stringFieldSort(
    items: any[],
    ascending: boolean,
    fieldName: string
): any[] {
    return items.sort((item1, item2) => {
        const first = item1[fieldName];
        const second = item2[fieldName];
        let result = caseInsensitiveStringSort(first, second);
        if (!ascending) {
            result = -result;
        }
        return result;
    });
}

function caseInsensitiveStringSort(item1: string, item2: string) {
    const first = item1.toUpperCase();
    const second = item2.toUpperCase();
    if (first < second) {
        return -1;
    }
    if (first > second) {
        return 1;
    }
    return 0;
}

export interface ColumnSort {
    columnIdentifier: string;
    ascending: boolean;
}

export function sort(
    itemsToSort: HotspotItem[],
    columns: ISortableColumn[],
    columnSort: ColumnSort | undefined
): HotspotItem[] {
    if (columnSort !== undefined) {
        itemsToSort = [...itemsToSort];
        let sortColumn: ISortableColumn | undefined = undefined;
        columns.forEach((column) => {
            if (column.columnIdentifier === columnSort.columnIdentifier) {
                (column.isSorted = true),
                    (column.isSortedDescending = !columnSort.ascending);
                sortColumn = column;
            } else {
                column.isSorted = false;
            }
        });
        return sortColumn!.sortItems(
            itemsToSort,
            !sortColumn!.isSortedDescending
        );
    }

    return itemsToSort;
}
