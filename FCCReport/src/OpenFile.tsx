import { IIconProps, Link } from "@fluentui/react";
import { VsChromeWebViewWindow } from "./webviewTypes";

const vsWindow = window as unknown as VsChromeWebViewWindow;

export function openHotspotLine(filePath: string, line: number | null) {
    vsWindow.chrome.webview.hostObjects.sourceFileOpener.openAtLine(
        filePath,
        line
    );
}

export function openClassFiles(filePaths: string[]) {
    vsWindow.chrome.webview.hostObjects.sourceFileOpener.openFiles(
        filePaths
    );
}

export const openFileInVsIconProps: IIconProps = { iconName: "OpenFile" };

interface OpenHotspot {
    filePath: string;
    methodLine: number | null;
    type: "hotspot";
}

interface OpenClass {
    filePaths: string[];
    type: "class";
}

export type OpenFileProps = {
    display: string;
    toOpenAriaLabel: string;
} & (OpenHotspot | OpenClass);

export function OpenFile(props: OpenFileProps) {
    const ariaLabel = `Open ${props.toOpenAriaLabel} in Visual Studio`;
    return (
            <Link
                aria-label={ariaLabel}
                onClick={() => {
                    if (props.type === "hotspot") {
                        openHotspotLine(props.filePath, props.methodLine);
                    } else {
                        openClassFiles(props.filePaths);
                    }
                }}
            >
                {props.display}
            </Link>
    );
}
