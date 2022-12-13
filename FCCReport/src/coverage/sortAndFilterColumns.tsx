import { IColumn } from '@fluentui/react';
import { nameColumn } from './Columns/columns';
import { ColumnSort } from "./ColumnSort";

export function sortAndFilterColumns(columns: IColumn[], filter: string, sortDetails: ColumnSort,grouping:number) {
  nameColumn.setFiltered(filter !== '');
  nameColumn.isGrouped = grouping > -1;
  columns.forEach(column => {
    column.isSorted = column.fieldName === sortDetails.fieldName;
    column.isSortedDescending = !sortDetails.ascending;
  });
}
