import { Selection, IGroup, SelectionMode } from "@fluentui/react";

export class GroupsItemsSelection extends Selection {
  private itemsLength: number | undefined;
  private groups: IGroup[] = [];
  constructor() {
    super({ selectionMode: SelectionMode.single });
  }
  public initialize(groups: IGroup[], items: any[]) {
    this.groups = [];
    groups.forEach(group => this.addGroup(group));
    this.setItemsPrivate(items);
  }

  private addGroup(group: IGroup) {
    this.groups.push(group);
    if (group.children) {
      group.children.forEach(g => this.addGroup(g));
    }
  }

  public setItems(): void {
  }

  private setItemsPrivate(items: any[]) {
    this.itemsLength = items.length;
    super.setItems(items.concat(this.groups), false);
  }

  public getGroupIndex(group: IGroup): number {
    const items = this.getItems();
    const groups = items.slice(this.itemsLength);
    let groupIndex = groups.findIndex(g => g.key === group.key);
    return groupIndex + this.itemsLength!;
  }

  setKeySelected(key: string, isSelected: boolean, shouldAnchor: boolean): void {
    super.setKeySelected(key, isSelected, shouldAnchor);
  }

  setIndexSelected(index: number, isSelected: boolean, shouldAnchor: boolean): void {
    super.setIndexSelected(index, isSelected, shouldAnchor);
  }
}
