import { Assembly } from '../types';

//#endregion

export function getGroupingMax(assemblies: Assembly[]): number {
  let groupingMax = 0;
  assemblies.forEach(assembly => {
    assembly.classes.forEach(cls => {
      // is 1 with a1.c1
      const classGroupingMax = cls.displayName.split('.').length - 1;
      if (classGroupingMax > groupingMax) {
        groupingMax = classGroupingMax;
      }
    });
  });
  return groupingMax;
}
