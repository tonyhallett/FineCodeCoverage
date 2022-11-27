import { customizable, IProgressIndicatorProps, ProgressIndicator } from "@fluentui/react";
import React from "react";

export interface IPercentageProps{
    percentage:number | null,
    styles:IProgressIndicatorProps['styles'],
}

export const vsStyledPercentageScope = "VsStyledPercentage";

@customizable(vsStyledPercentageScope, ['theme', 'styles'], true)
export class VsStyledPercentage extends React.Component<{
  percentage:number | null,
  styles?:IProgressIndicatorProps['styles']
  barHeight?:number
}, {}> {
  public render(): JSX.Element {
    const {percentage, styles,barHeight} = this.props;
    return <ProgressIndicator  barHeight={barHeight} percentComplete={percentage === null ? 1 : percentage / 100} styles={styles} />;
  }
  
}