import { CheckboxVisibility, DetailsList, IDetailsListProps, SelectionMode } from "@fluentui/react";

/*
    if this was a multi-use component
    isHeaderVisible would be a prop
    There would be other props removed
*/
type SimpleTableProps = Omit<IDetailsListProps,'role'|'checkboxVisibility'|'isHeaderVisible'|'selectionMode'|'focusZoneProps'|'onRenderRow'>
export function SimpleTable(props:SimpleTableProps){
    
    return <DetailsList
    role='table'
    checkboxVisibility={CheckboxVisibility.hidden}
    isHeaderVisible={false}
    selectionMode={SelectionMode.none}
    onRenderRow={(props, defaultRender) => {
      props!.styles = {
        root: {
          '&:hover': {
            backgroundColor: 'transparent'
          }
        }
      };
      return defaultRender!(props);
    }}
    focusZoneProps={{
      disabled: true
    }}
    {...props} 
    />
}