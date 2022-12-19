import { ICoverageItemBase } from "../ICoverageItemBase";
import { ICoverageColumn } from "./ICoverageColumn";

export class CoverageColumn {
    public static create(
        fieldName: keyof ICoverageItemBase,
        name: string,
        onRender: ICoverageColumn["onRender"]
    ): ICoverageColumn {
        return {
            minWidth: 100,
            key: fieldName,
            fieldName,
            name,
            onRender,
        };
    }
}
