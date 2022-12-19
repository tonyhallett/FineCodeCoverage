import { KeysOfType } from "./types";

export function caseInsensitiveStringFieldSort<
    T,
    K extends KeysOfType<T, string>
>(items: T[], ascending: boolean, fieldName: K): T[] {
    return stringFieldSort(
        items,
        ascending,
        fieldName,
        caseInsensitiveStringSort
    );
}

function stringFieldSort<T, K extends KeysOfType<T, string>>(
    items: T[],
    ascending: boolean,
    fieldName: K,
    sortFunction: (first: string, second: string) => number
): T[] {
    return items.sort((item1, item2) => {
        const first = item1[fieldName] as unknown as string;
        const second = item2[fieldName] as unknown as string;
        let result = sortFunction(first, second);
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
