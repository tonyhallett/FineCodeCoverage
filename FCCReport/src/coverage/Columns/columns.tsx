import { IColumn } from "@fluentui/react";
import { CopyToClipboard } from "../../helper-components/CopyToCliboard";
import { OpenFile } from "../../OpenFile";
import { VsStyledDetailsListCellText } from "../../vs-styling/VsStyledDetailsListCellText";
import { focusingCells } from "../common";
import { ICoverageItemBase } from "../ICoverageItemBase";
import { DataColumn } from "./DataColumn";
import { INameColumn } from "./INameColumn";
import { PercentageColumn } from "./PercentageColumn";

const useProFeature = false;

const coveredColumnDisplay = "Covered";
const totalColumnDisplay = "Total";

export const nameColumn: INameColumn = {
    key: "name",
    name: "Name",
    fieldName: "name",
    minWidth: 100,
    onRender: (item: ICoverageItemBase) => {
        if (!item.standalone && item.classPaths) {
            const toOpenAriaLabel = `class ${item.name}`;
            return (
                <OpenFile
                    toOpenAriaLabel={toOpenAriaLabel}
                    type="class"
                    filePaths={item.classPaths}
                    display={item.name}
                />
            );
        }
        return (
            <CopyToClipboard>
                <VsStyledDetailsListCellText data-is-focusable={focusingCells}>
                    {item.name}
                </VsStyledDetailsListCellText>
            </CopyToClipboard>
        );
    },
    setFiltered(filtered: boolean) {
        this.isFiltered = filtered;
        //this.name = filtered ? "Name ( class )" : "Name";
    },
};

export const coveredLinesColumn = DataColumn.create(
    "coveredLines",
    coveredColumnDisplay
);
export const coverableLinesColumn = DataColumn.create(
    "coverableLines",
    "Coverable"
);
export const uncoveredLinesColumn = DataColumn.create(
    "uncoveredLines",
    "Uncovered"
);
export const totalLinesColumn = DataColumn.create(
    "totalLines",
    totalColumnDisplay
);
export const lineCoverageQuotaColumn = PercentageColumn.create(
    "coverageQuota",
    "Line Coverage"
);

export const coveredBranchesColumn = DataColumn.create(
    "coveredBranches",
    coveredColumnDisplay
);
export const totalBranchesColumn = DataColumn.create(
    "totalBranches",
    totalColumnDisplay
);
export const branchCoverageQuotaColumn = PercentageColumn.create(
    "branchCoverageQuota",
    "Branch Coverage"
);
export const coveredCodeElementsColumn = DataColumn.create(
    "coveredCodeElements",
    coveredColumnDisplay
);
export const totalCodeElementsColumn = DataColumn.create(
    "totalCodeElements",
    totalColumnDisplay
);
export const codeElementCoverageQuotaColumn = PercentageColumn.create(
    "codeElementCoverageQuota",
    "Element Coverage"
);

export function getColumns(supportsBranchCoverage: boolean) {
    const columns: IColumn[] = [
        nameColumn,
        coveredLinesColumn,
        coverableLinesColumn,
        uncoveredLinesColumn,
        totalLinesColumn,
        lineCoverageQuotaColumn,
    ];

    if (supportsBranchCoverage) {
        columns.push(coveredBranchesColumn);
        columns.push(totalBranchesColumn);
        columns.push(branchCoverageQuotaColumn);
    }

    if (useProFeature) {
        columns.push(coveredCodeElementsColumn);
        columns.push(totalCodeElementsColumn);
        columns.push(codeElementCoverageQuotaColumn);
    }
    for (const col of columns) {
        (col.isResizable = true), (col.calculatedWidth = 0);
        col.flexGrow = undefined;
    }

    columns[columns.length - 1].flexGrow = 1;

    return columns;
}
