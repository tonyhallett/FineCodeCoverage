import { ICoverageColumn } from './ICoverageColumn';
import { ICoverageItemBase } from '../ICoverageItemBase';

export class CoverageColumn{
  public static create(fieldName: keyof ICoverageItemBase, name: string):ICoverageColumn{
    return {
      minWidth:100,
      key:fieldName,
      fieldName,
      name
    }
  }
}
