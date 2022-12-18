import {
    CheckboxVisibility,
    customizable,
    DetailsList,
    DetailsRow,
    IDetailsListProps,
    IDetailsRowProps,
    SelectionMode,
} from "@fluentui/react";
import React from "react";

@customizable("SimpleTableRow", ["theme", "styles"], true)
class SimpleTableRow extends React.Component<IDetailsRowProps> {
    public render(): JSX.Element {
        return <DetailsRow {...this.props} />;
    }
}

/*
    if this was a multi-use component
    isHeaderVisible would be a prop
    There would be other props removed
*/
type SimpleTableProps = Omit<
    IDetailsListProps,
    | "role"
    | "checkboxVisibility"
    | "isHeaderVisible"
    | "selectionMode"
    | "focusZoneProps"
    | "onRenderRow"
>;
export function SimpleTable(props: SimpleTableProps) {
    return (
        <DetailsList
            role="table"
            checkboxVisibility={CheckboxVisibility.hidden}
            isHeaderVisible={false}
            selectionMode={SelectionMode.none}
            onRenderRow={(props) => {
                return props ? <SimpleTableRow {...props} /> : null;
            }}
            focusZoneProps={{
                disabled: true,
            }}
            {...props}
        />
    );
}
