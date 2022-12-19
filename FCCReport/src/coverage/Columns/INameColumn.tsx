import { ICoverageColumn } from "./ICoverageColumn";

export interface INameColumn extends ICoverageColumn {
    setFiltered: (filtered: boolean) => void;
}
