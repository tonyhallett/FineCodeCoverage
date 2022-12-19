import { ClassCoverage } from "../types";

export interface ICoverageItemBase extends ClassCoverage {
    key: string;

    name: string;

    uncoveredLines: number;

    classPaths: string[] | undefined;
    standalone: boolean;
}
