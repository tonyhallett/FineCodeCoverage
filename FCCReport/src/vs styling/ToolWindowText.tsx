import { customizable, ITextProps, Text } from "@fluentui/react";
import React from "react";

@customizable('ToolWindowText', ['theme', 'styles'], true)
export class ToolWindowText extends React.Component<ITextProps, {}> {
  public render(): JSX.Element {
    return <Text styles={this.props.styles}>{this.props.children}</Text>
  }
  
}

