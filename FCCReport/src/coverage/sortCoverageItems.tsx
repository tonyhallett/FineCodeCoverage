import { ICoverageItemBase } from './ICoverageItemBase';

function numericSort(
  left:ICoverageItemBase, 
  right:ICoverageItemBase,
  fieldName:keyof ICoverageItemBase, 
  smaller:number,
  bigger:number
):number{
  return left[fieldName] === right[fieldName] ?
        0
        : (left[fieldName]! < right[fieldName]! ? smaller : bigger);
}


// todo type keyof to non function fields
function numericNullableSort(
  left:ICoverageItemBase, 
  right:ICoverageItemBase,
  fieldName:keyof ICoverageItemBase, 
  smaller:number,
  bigger:number
):number{
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

}

const nullableQuotaFields = ['coverageQuota','branchCoverageQuota','codeElementCoverageQuota'];//todo type so no !

export function sortCoverageItems(
  coverageItems: ICoverageItemBase[],
  fieldName: keyof ICoverageItemBase,
  ascending: boolean
) {
  const smaller: number = ascending ? -1 : 1;
  const bigger: number = ascending ? 1 : -1;
  let sortMethod = numericSort;
  if (nullableQuotaFields.some(nullableQuotaField => nullableQuotaField === fieldName)) {
    sortMethod = numericNullableSort;
  }
  coverageItems.sort((left, right) => {
    return sortMethod(left, right, fieldName, smaller, bigger);
  });
}
