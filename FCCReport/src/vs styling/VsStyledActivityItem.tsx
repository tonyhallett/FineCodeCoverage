import { customizable, IActivityItemProps, ActivityItem } from "@fluentui/react";
import React from "react";

export const VsStyledActivityItemScope = 'VsStyledActivityItem';

@customizable(VsStyledActivityItemScope, ['theme', 'styles'], true)
export class VsStyledActivityItem extends React.Component<IActivityItemProps, {}> {
  public render(): JSX.Element {
    

    return <ActivityItem {...this.props}/>
  }
}