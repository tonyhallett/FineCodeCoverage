import { ICoverageItemBase } from "./ICoverageItemBase";

export function sortCoverageItems(
    coverageItems: ICoverageItemBase[],
    fieldName: keyof ICoverageItemBase,
    ascending: boolean
) {
    const smaller: number = ascending ? -1 : 1;
    const bigger: number = ascending ? 1 : -1;

    coverageItems.sort((left, right) => {
        const leftValue = left[fieldName];
        const rightValue = right[fieldName];
        if (leftValue === rightValue) {
            return 0;
        } else if (leftValue === null || leftValue === undefined) {
            return smaller;
        } else if (rightValue === null || rightValue === undefined) {
            return bigger;
        } else {
            return leftValue < rightValue ? smaller : bigger;
        }
    });
}
