import { ICoverageItem } from '../ICoverageItem';
import { ICoverageItemBase } from '../ICoverageItemBase';
import { VsStyledPercentage } from '../../vs styling/VsStyledPercentage';
import { CoverageColumn } from './CoverageColumn';
import { focusingCells } from '../common';
import { VsStyledDetailsListCellText } from '../../vs styling/VsStyledDetailsListCellText';
import { Stack } from '@fluentui/react';
import { CopyToClipboard } from '../../helper components/CopyToCliboard';

export class PercentageColumn {
  // because fluentui creates a new IColumn with object spread
  public static create(fieldName: keyof ICoverageItemBase, name: string){
    const coverageColumn = CoverageColumn.create(fieldName,name);
    coverageColumn.onRender = (item: ICoverageItem) => {
      const quota: number | null = item[coverageColumn.fieldName] as any;//todo
      // <Stack horizontal horizontalAlign='space-between' verticalAlign='center'>
      return <Stack horizontal verticalAlign='center'>
          <VsStyledPercentage styles={{root:{display:"inline-block", width:"50px"}}} barHeight={2} percentage={quota} />
          <div>
            <CopyToClipboard>
              <VsStyledDetailsListCellText styles={{root:{marginLeft:"5px"}}} data-is-focusable={focusingCells}>{quota}</VsStyledDetailsListCellText>
            </CopyToClipboard>
            </div>
      </Stack>
    }
    return coverageColumn;
  }
}
