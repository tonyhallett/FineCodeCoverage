import { IGroup } from '@fluentui/react';
import { ICoverageItemBase } from '../ICoverageItemBase';

export interface ICoverageGroup extends IGroup, ICoverageItemBase {
  classPaths: undefined;
  totalBranches: number;
  coveredBranches: number;
  totalLines: number;
  filter: (filter: string, hideFullyCovered: boolean) => void;
  sort: (fieldName: keyof ICoverageItemBase, ascending: boolean) => void;
  hideFullyCovered?: never;
}
