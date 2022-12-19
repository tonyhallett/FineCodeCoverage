import { Assembly, Class } from '../../types';
import { getQuota } from '../getQuota';
import { ICoverageItemBase } from '../ICoverageItemBase';
import { ICoverageGroup } from './ICoverageGroup';
import { ClassesGroup } from './ClassesGroup';
import { sortCoverageItems } from "../sortCoverageItems";

// even if have no behaviour a base group for presence of fields useful
export class NamespacedGroup implements ICoverageGroup {
  name: string;
  key: string;
  classPaths: undefined;
  // will be set from outside
  count = 0;
  startIndex = 0;

  // todo - this is common typing 
  coveredLines = 0;
  coverableLines = 0;
  uncoveredLines = 0;
  totalLines = 0;
  coverageQuota: number | null;

  coveredBranches = 0;
  totalBranches = 0;
  branchCoverageQuota: number | null;

  coveredCodeElements = 0;
  totalCodeElements = 0;
  codeElementCoverageQuota: number | null;

  children: ClassesGroup[] = [];
  readonly level = 0;
  ariaLabel: string;
  constructor(assembly: Assembly, namespacedClasses: boolean, grouping: number, isStandalone: boolean) {
    this.name = assembly.shortName;
    this.key = assembly.name; // will need a different key ?
    this.ariaLabel = `${assembly.name} class coverage`;
    const map: Map<string, Class[]> = new Map();
    assembly.classes.forEach(cls => {
      const namespaceGroupingName = this.getGrouping(cls.displayName, grouping);
      let classes = map.get(namespaceGroupingName)
      if (classes === undefined) {
        classes = [];
        map.set(namespaceGroupingName, classes);
      }
      classes.push(cls);
    });
    map.forEach((classes, groupingName) => {
      // todo if just add the assembly to the class will be much better than iterating again
      this.children.push(new ClassesGroup(classes, namespacedClasses, groupingName, isStandalone, `Namespace ${groupingName} class coverage`, 1));
    });
    this.children.forEach(childGroup => {
      this.coveredLines += childGroup.coveredLines;
      this.coverableLines += childGroup.coverableLines;
      this.uncoveredLines += childGroup.uncoveredLines;
      this.totalLines += childGroup.totalLines;

      this.coveredBranches += childGroup.coveredBranches;
      this.totalBranches += childGroup.totalBranches;

      this.coveredCodeElements += childGroup.coveredCodeElements;
      this.totalCodeElements += childGroup.totalCodeElements;
    });
    this.coverageQuota = getQuota(this.coveredLines, this.coverableLines);
    this.branchCoverageQuota = getQuota(this.coveredBranches, this.totalBranches);
    this.codeElementCoverageQuota = getQuota(this.coveredCodeElements, this.totalCodeElements);
  }

  private getGrouping(namespacedClass: string, level: number): string {
    const parts = namespacedClass.split('.');
    const namespaceParts = parts.length - 1;
    if (namespaceParts === 0) {
      return "( Global )";
    }
    const takeParts = namespaceParts < level ? namespaceParts : level;
    return parts.slice(0, takeParts).join('.');
  }

  filter(filter: string, hideFullyCovered: boolean): void {
    this.children.forEach(classesGroup => classesGroup.filter(filter, hideFullyCovered));
  }

  sort(fieldName: keyof ICoverageItemBase, ascending: boolean) {
    sortCoverageItems(this.children, fieldName, ascending);
    this.children.forEach(classesGroup => classesGroup.sort(fieldName, ascending));
  }
}
