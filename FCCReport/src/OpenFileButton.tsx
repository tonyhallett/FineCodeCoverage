import { ActionButton, IIconProps } from '@fluentui/react';
import React from 'react';

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
} & (OpenHotspot | OpenClass)

export function OpenFileButton(props: OpenFileProps) {
    const toOpen = props.type === 'class' ? 'class' : 'method';
    let ariaDescription = `Open ${toOpen} in Visual Studio`;
  
    return <ActionButton ariaLabel='Open in Visual Studio' ariaDescription={ariaDescription}  iconProps={openFileInVsIconProps} onClick={() => {
      if (props.type === 'hotspot') {
        openHotspotLine(props.filePath, props.methodLine);
      } else {
        openClassFiles(props.filePaths);
      }
    }}>
    {props.display}
  </ActionButton>;
}
