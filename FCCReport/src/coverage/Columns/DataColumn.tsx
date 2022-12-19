import { VsStyledDetailsListCellText } from "../../vs-styling/VsStyledDetailsListCellText";
import { ICoverageItemBase } from "../ICoverageItemBase";
import { focusingCells } from "../common";
import { CoverageColumn } from "./CoverageColumn";
import { CopyToClipboard } from "../../helper-components/CopyToCliboard";
import { ICoverageColumn } from "./ICoverageColumn";

export class DataColumn {
    // because fluentui creates a new IColumn with object spread cannot use a class
    public static create(
        fieldName: keyof ICoverageItemBase,
        name: string
    ): ICoverageColumn {
        return CoverageColumn.create(fieldName, name, (item) => {
            if (!item) {
                return null;
            }
            return (
                <CopyToClipboard>
                    <VsStyledDetailsListCellText
                        data-is-focusable={focusingCells}
                    >
                        {item[fieldName]}
                    </VsStyledDetailsListCellText>
                </CopyToClipboard>
            );
        });
    }
}
