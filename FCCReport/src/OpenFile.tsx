import { IIconProps } from '@fluentui/react';
import { DetailsListCellText } from './vs styling/DetailsListCellText';
import { StyledActionButton } from './vs styling/StyledActionButton';

export function openHotspotLine(filePath:string,line:number|null){
  (window as any).chrome.webview.hostObjects.sourceFileOpener.openAtLine(filePath, line);
}

export function openClassFiles(filePaths:string[]){
  (window as any).chrome.webview.hostObjects.sourceFileOpener.openFiles(filePaths);
}

// todo icon registration in other file and typed
export const openFileInVsIconProps: IIconProps = { iconName: 'OpenFile' };

interface OpenHotspot{
  filePath:string,
  methodLine:number|null,
  type:'hotspot'
}

interface OpenClass{
  filePaths:string[],
  type:'class'
}

export type OpenFileProps = {
  display:string
  toOpenAriaLabel:string
} & (OpenHotspot | OpenClass)

export function OpenFile(props: OpenFileProps) {
  const ariaLabel = `Open ${props.toOpenAriaLabel} in Visual Studio`;
    return <><StyledActionButton ariaLabel={ariaLabel} iconProps={openFileInVsIconProps} onClick={() => {
      if (props.type === 'hotspot') {
        openHotspotLine(props.filePath, props.methodLine);
      } else {
        openClassFiles(props.filePaths);
      }
    }}>
    
  </StyledActionButton>
  <DetailsListCellText style={{marginLeft:'5px'}}>{props.display}</DetailsListCellText>
  </>
}
