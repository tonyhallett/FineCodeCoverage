import { Class } from '../../types';
import { getQuota } from './../getQuota';
import { ICoverageItemBase } from '../ICoverageItemBase';
import { ICoverageItem } from '../ICoverageItem';
import { ICoverageGroup } from './ICoverageGroup';
import { CoverageItem } from '../CoverageItem';
import { sortCoverageItems } from "../sortCoverageItems";

export class ClassesGroup implements ICoverageGroup {
  name: string;
  key: string;
  classPaths: undefined;
  // will be set from outside
  count: number = 0;
  startIndex: number = 0;

  // todo - this is common typing 
  coveredLines: number = 0;
  coverableLines: number = 0;
  uncoveredLines: number = 0;
  totalLines: number = 0;
  coverageQuota: number | null;

  coveredBranches: number = 0;
  totalBranches: number = 0;
  branchCoverageQuota: number | null;

  coveredCodeElements: number = 0;
  totalCodeElements: number = 0;
  codeElementCoverageQuota: number | null;
  private _items: ICoverageItem[] = [];
  level: number;
  items: ICoverageItem[] = [];
  ariaLabel: string;

  constructor(classes: Class[], namespacedClasses: boolean, name: string, standalone: boolean, ariaLabel: string, level: number = 0) {
    this.level = level;
    this.name = name;
    this.key = name;
    this.ariaLabel = ariaLabel;
    classes.forEach(cls => {
      const coverageItem = new CoverageItem(cls, namespacedClasses, standalone);
      this._items.push(coverageItem);

      this.coveredLines += coverageItem.coveredLines;
      this.coverableLines += coverageItem.coverableLines;
      this.uncoveredLines += coverageItem.uncoveredLines;
      this.totalLines += coverageItem.totalLines;

      this.coveredBranches += coverageItem.coveredBranches;
      this.totalBranches += coverageItem.totalBranches;

      this.coveredCodeElements += coverageItem.coveredCodeElements;
      this.totalCodeElements += coverageItem.totalCodeElements;

    });
    this.coverageQuota = getQuota(this.coveredLines, this.coverableLines);
    this.branchCoverageQuota = getQuota(this.coveredBranches, this.totalBranches);
    this.codeElementCoverageQuota = getQuota(this.coveredCodeElements, this.totalCodeElements);
  }

  filter(filter: string, hideFullyCoverered: boolean): void {

    this.items = this._items.filter(coverageItem => {
      if (hideFullyCoverered && coverageItem.isFullyCovered()) {
        return false;
      }
      if (filter === '') {
        return true;
      }
      return coverageItem.name.toLowerCase().indexOf(filter.toLowerCase()) > -1;
    });
  }

  sort(fieldName: keyof ICoverageItemBase, ascending: boolean) {
    sortCoverageItems(this.items, fieldName, ascending);
  }
}
