import { customizable, IProgressIndicatorProps, ProgressIndicator } from "@fluentui/react";
import React from "react";

export interface IPercentageProps{
    percentage:number | null,
    styles:IProgressIndicatorProps['styles'],
}

@customizable('Percentage', ['theme', 'styles'], true)
export class Percentage extends React.Component<{percentage:number | null,styles:IProgressIndicatorProps['styles']}, {}> {
  public render(): JSX.Element {
    const {percentage, styles} = this.props;
    return <ProgressIndicator  barHeight={5} percentComplete={percentage === null ? 1 : percentage / 100} styles={styles} />;
  }
  
}