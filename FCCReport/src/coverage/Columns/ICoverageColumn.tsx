import { IColumn } from '@fluentui/react';
import { ICoverageItemBase } from '../ICoverageItemBase';

export interface ICoverageColumn extends IColumn {
  fieldName: keyof ICoverageItemBase;
}
