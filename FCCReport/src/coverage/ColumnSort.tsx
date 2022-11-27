import { ICoverageItemBase } from './ICoverageItemBase';


export interface ColumnSort {
  fieldName: keyof ICoverageItemBase | undefined;
  ascending: boolean;
}
