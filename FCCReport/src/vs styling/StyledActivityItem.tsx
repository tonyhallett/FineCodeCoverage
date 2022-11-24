import { customizable, IActivityItemProps, ActivityItem } from "@fluentui/react";
import React from "react";

@customizable('VsStyledActivityItem', ['theme', 'styles'], true)
export class VsStyledActivityItem extends React.Component<IActivityItemProps, {}> {
  public render(): JSX.Element {
    

    return <ActivityItem {...this.props}/>
  }
}