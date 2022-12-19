import { ICoverageItemBase } from "../ICoverageItemBase";
import { VsStyledPercentage } from "../../vs-styling/VsStyledPercentage";
import { CoverageColumn } from "./CoverageColumn";
import { focusingCells } from "../common";
import { VsStyledDetailsListCellText } from "../../vs-styling/VsStyledDetailsListCellText";
import { Stack } from "@fluentui/react";
import { CopyToClipboard } from "../../helper-components/CopyToCliboard";
import { KeysOfType } from "../../utilities/types";

export class PercentageColumn {
    // because fluentui creates a new IColumn with object spread cannot create a class
    public static create(
        fieldName: KeysOfType<ICoverageItemBase, number | null>,
        name: string
    ) {
        return CoverageColumn.create(fieldName, name, (item) => {
            const quota = item[fieldName];
            return (
                <Stack
                    horizontal
                    verticalAlign="center"
                >
                    <VsStyledPercentage
                        styles={{
                            root: { display: "inline-block", width: "50px" },
                        }}
                        barHeight={2}
                        percentage={quota}
                    />
                    <CopyToClipboard>
                        <VsStyledDetailsListCellText
                            styles={{ root: { marginLeft: "5px" } }}
                            data-is-focusable={focusingCells}
                        >
                            {quota}
                        </VsStyledDetailsListCellText>
                    </CopyToClipboard>
                </Stack>
            );
        });
    }
}
