import { Assembly } from '../../types';
import { ClassesGroup } from './ClassesGroup';

export class AssemblyGroup extends ClassesGroup {
  constructor(assembly: Assembly, namespacedClasses: boolean, standalone: boolean) {
    super(assembly.classes, namespacedClasses, assembly.shortName, standalone, `Assembly ${assembly.name} class coverage`);
  }
}
