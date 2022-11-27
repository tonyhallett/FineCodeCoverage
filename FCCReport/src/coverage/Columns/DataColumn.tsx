import { VsStyledDetailsListCellText } from '../../vs styling/VsStyledDetailsListCellText';
import { ICoverageItem } from '../ICoverageItem';
import { ICoverageItemBase } from '../ICoverageItemBase';
import { focusingCells } from '../common';
import { CoverageColumn } from './CoverageColumn';
import { CopyToClipboard } from '../../helper components/CopyToCliboard';

export class DataColumn{
  // because fluentui creates a new IColumn with object spread
  public static create(fieldName: keyof ICoverageItemBase, name: string){
    const coverageColumn = CoverageColumn.create(fieldName,name);
    coverageColumn.onRender = (item: ICoverageItem) => {
      return <CopyToClipboard>
        <VsStyledDetailsListCellText data-is-focusable={focusingCells}>{item[coverageColumn.fieldName]}</VsStyledDetailsListCellText>
        </CopyToClipboard>
    }
    return coverageColumn;
  }
}
