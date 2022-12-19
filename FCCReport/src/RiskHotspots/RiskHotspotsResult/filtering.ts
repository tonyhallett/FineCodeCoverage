import { IDropdownOption } from "@fluentui/react";
import { Assembly } from "../../types";
import { assemblyColumn, classColumn } from "./columns";
import { HotspotItem } from "./hotspotItem";

export const allAssembliesOption: IDropdownOption<Assembly> = {
    key: "All assemblies!",
    text: "All assemblies",
};

function filterByAssembly(
    items: HotspotItem[],
    selectedAssemblyFilterOption: IDropdownOption<Assembly>,
    allAssembliesKey: string | number
): HotspotItem[] {
    let filteredByAssembly = items;
    assemblyColumn.isFiltered = false;
    if (selectedAssemblyFilterOption.key !== allAssembliesKey) {
        filteredByAssembly = items.filter(
            (item) => item.assembly === selectedAssemblyFilterOption.data
        );
        assemblyColumn.isFiltered = true;
    }
    return filteredByAssembly;
}

function filterByClass(
    items: HotspotItem[],
    classDisplayFilter: string | undefined
): HotspotItem[] {
    if (classDisplayFilter === undefined || classDisplayFilter === "") {
        classColumn.isFiltered = false;
        return items;
    }

    classColumn.isFiltered = true;
    return items.filter((item) => {
        return (
            item.classDisplay
                .toLowerCase()
                .indexOf(classDisplayFilter.toLowerCase()) != -1
        );
    });
}

export function filterItems(
    items: HotspotItem[],
    selectedAssemblyFilterOption: IDropdownOption<Assembly>,
    allAssembliesKey: string | number,
    classDisplayFilter: string | undefined
): HotspotItem[] {
    const filteredByAssembly = filterByAssembly(
        items,
        selectedAssemblyFilterOption,
        allAssembliesKey
    );
    return filterByClass(filteredByAssembly, classDisplayFilter);
}
