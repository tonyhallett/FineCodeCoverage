import { Assembly } from '../../types';
import { ClassesGroup } from './ClassesGroup';

export class AllGroup extends ClassesGroup {

  constructor(assemblies: Assembly[], namespacedClasses: boolean, standalone: boolean) {
    super(AllGroup.getClasses(assemblies), namespacedClasses, "All", standalone, "All class coverage");
  }
  static getClasses(assemblies: Assembly[]) {
    const classes: any = []; // todo typing
    assemblies.forEach(assembly => {
      assembly.classes.forEach(cls => {
        classes.push(cls);
      });
    });
    return classes;
  }
}
