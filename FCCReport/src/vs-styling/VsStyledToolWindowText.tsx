import { customizable, ITextProps, Text } from "@fluentui/react";
import React from "react";

export const vsStyledToolWindowTextScope = "VsStyledToolWindowText";

@customizable(vsStyledToolWindowTextScope, ["theme", "styles"], true)
export class VsSTyledToolWindowText extends React.Component<ITextProps> {
    public render(): JSX.Element {
        return <Text styles={this.props.styles}>{this.props.children}</Text>;
    }
}
