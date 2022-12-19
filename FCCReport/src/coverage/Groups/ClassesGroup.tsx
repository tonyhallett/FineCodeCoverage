import { Class } from "../../types";
import { getQuota } from "./../getQuota";
import { ICoverageItemBase } from "../ICoverageItemBase";
import { ICoverageItem } from "../ICoverageItem";
import { ICoverageGroup } from "./ICoverageGroup";
import { CoverageItem } from "../CoverageItem";
import { sortCoverageItems } from "../sortCoverageItems";

export class ClassesGroup implements ICoverageGroup {
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
    private _items: ICoverageItem[] = [];
    level: number;
    items: ICoverageItem[] = [];
    ariaLabel: string;
    standalone: boolean;

    constructor(
        classes: Class[],
        namespacedClasses: boolean,
        name: string,
        standalone: boolean,
        ariaLabel: string,
        level = 0
    ) {
        this.standalone = standalone;
        this.level = level;
        this.name = name;
        this.key = name;
        this.ariaLabel = ariaLabel;
        classes.forEach((cls) => {
            const coverageItem = new CoverageItem(
                cls,
                namespacedClasses,
                standalone
            );
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
        this.branchCoverageQuota = getQuota(
            this.coveredBranches,
            this.totalBranches
        );
        this.codeElementCoverageQuota = getQuota(
            this.coveredCodeElements,
            this.totalCodeElements
        );
    }

    filter(filter: string, hideFullyCoverered: boolean): void {
        this.items = this._items.filter((coverageItem) => {
            if (hideFullyCoverered && coverageItem.isFullyCovered()) {
                return false;
            }
            if (filter === "") {
                return true;
            }
            return (
                coverageItem.name.toLowerCase().indexOf(filter.toLowerCase()) >
                -1
            );
        });
    }

    sort(fieldName: keyof ICoverageItemBase, ascending: boolean) {
        sortCoverageItems(this.items, fieldName, ascending);
    }
}
