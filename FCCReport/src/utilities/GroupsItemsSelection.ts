import {
    Selection,
    IGroup,
    SelectionMode,
    IObjectWithKey,
} from "@fluentui/react";

export class GroupsItemsSelection<
    TItem extends IObjectWithKey
> extends Selection {
    private itemsLength = 0;
    private groups: IGroup[] = [];
    constructor() {
        super({ selectionMode: SelectionMode.single });
    }

    public initialize(groups: IGroup[], items: TItem[]) {
        this.groups = [];
        groups.forEach((group) => this.addGroup(group));
        this.setItemsPrivate(items);
    }

    private addGroup(group: IGroup) {
        this.groups.push(group);
        if (group.children) {
            group.children.forEach((g) => this.addGroup(g));
        }
    }

    public setItems(): void {
        //ok
    }

    private setItemsPrivate(items: IObjectWithKey[]) {
        this.itemsLength = items.length;
        const groups = this.groups as IObjectWithKey[];
        super.setItems(items.concat(groups), false);
    }

    public getGroupIndex(group: IGroup): number {
        const items = this.getItems();
        const groups = items.slice(this.itemsLength);
        const groupIndex = groups.findIndex((g) => g.key === group.key);
        return groupIndex + this.itemsLength;
    }
}
