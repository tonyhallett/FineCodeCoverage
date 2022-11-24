import { customizable, ITextProps, Text } from "@fluentui/react";
import React from "react";


@customizable('DetailsListCellText', ['theme', 'styles'], true)
export class DetailsListCellText extends React.Component<ITextProps, {}> {
    public render(): JSX.Element {
        return <Text {...this.props}/>
    }

}
