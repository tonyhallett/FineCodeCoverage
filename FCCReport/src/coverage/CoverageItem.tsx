import { Class, CoverageType } from '../types';
import { removeNamespaces } from '../common';
import { getQuota } from './getQuota';
import { ICoverageItem } from './ICoverageItem';

export class CoverageItem implements ICoverageItem {
  coveredLines: number;
  coverableLines: number;
  uncoveredLines: number;
  totalLines: number;
  coverageQuota: number | null = null;

  coveredBranches: number;
  totalBranches: number;
  branchCoverageQuota: number | null;

  coveredCodeElements: number;
  totalCodeElements: number;
  codeElementCoverageQuota: number | null;

  key: string;
  name: string;
  classPaths: string[];
  standalone: boolean;
  constructor(cls: Class, namespacedClasses: boolean, standalone: boolean) {
    this.standalone = standalone;
    this.name = namespacedClasses ? cls.displayName : removeNamespaces(cls.displayName);
    this.key = `${cls.assemblyIndex}${cls.name}`; // name or displayName
    this.classPaths = cls.files.map(f => f.path);

    this.coveredLines = cls.coveredLines;
    this.coverableLines = cls.coverableLines;
    this.uncoveredLines = cls.coverableLines - cls.coveredLines;
    this.totalLines = cls.totalLines === null ? 0 : cls.totalLines;
    if (this.coverableLines === 0) {
      if (cls.coverageType === CoverageType.MethodCoverage && cls.coverageQuota !== null) {
        this.coverageQuota = cls.coverageQuota;
      }
    } else {
      this.coverageQuota = getQuota(cls.coveredLines, cls.coverableLines);
    }

    this.totalBranches = cls.totalBranches === null ? 0 : cls.totalBranches;
    this.coveredBranches = cls.coveredBranches === null ? 0 : cls.coveredBranches;
    this.branchCoverageQuota = cls.branchCoverageQuota;

    this.coveredCodeElements = cls.coveredCodeElements;
    this.totalCodeElements = cls.totalCodeElements;
    this.codeElementCoverageQuota = cls.codeElementCoverageQuota;
  }
  filter(filter: string): boolean {
    return this.name.toLowerCase().indexOf(filter.toLowerCase()) > -1;
  }
  isFullyCovered(): boolean {
    return this.coverageQuota !== null && this.coverageQuota === 100 && this.branchCoverageQuota !== null && this.branchCoverageQuota === 100;
  }
}
