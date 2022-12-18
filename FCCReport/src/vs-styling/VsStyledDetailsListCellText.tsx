import { customizable, ITextProps, Text } from "@fluentui/react";
import React from "react";

export const vsStyledDetailsListCellTextScope = "VsStyledDetailsListCellText";
@customizable(vsStyledDetailsListCellTextScope, ["theme", "styles"], true)
export class VsStyledDetailsListCellText extends React.Component<
    ITextProps
> {
    public render(): JSX.Element {
        return <Text {...this.props} />;
    }
}
