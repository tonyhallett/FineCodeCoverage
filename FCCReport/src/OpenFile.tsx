import { IIconProps, Link } from '@fluentui/react';
import { VsStyledDetailsListCellText } from './vs styling/VsStyledDetailsListCellText';
import { VsStyledActionButton } from './vs styling/VsStyledActionButton';

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
    return <>
    <Link aria-label={ariaLabel} onClick={() => {
      if (props.type === 'hotspot') {
        openHotspotLine(props.filePath, props.methodLine);
      } else {
        openClassFiles(props.filePaths);
      }
    }}>{props.display}</Link>
    {/* <VsStyledDetailsListCellText style={{marginRight:'5px'}}>{props.display}</VsStyledDetailsListCellText>
    <VsStyledActionButton ariaLabel={ariaLabel} iconProps={openFileInVsIconProps} onClick={() => {
      if (props.type === 'hotspot') {
        openHotspotLine(props.filePath, props.methodLine);
      } else {
        openClassFiles(props.filePaths);
      }
    }}>
    </VsStyledActionButton> */}
  </>
}
