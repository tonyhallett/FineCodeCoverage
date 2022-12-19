import { ICoverageItemBase } from "./ICoverageItemBase";

export interface ICoverageItem extends ICoverageItemBase {
    isFullyCovered: () => boolean;
    totalLines: number;
    coveredBranches: number;
    totalBranches: number;
    standalone: boolean;
}
