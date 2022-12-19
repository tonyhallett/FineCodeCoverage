export interface Payload<T> {
    type: string;
    data: T;
}

type HostObject = Record<string, (...args: unknown[]) => void>;

type HostObjects = Record<string, HostObject>;

type VsHostObjects = HostObjects & {
    fccResourcesNavigator: {
        buyMeACoffee: () => void;
        logIssueOrSuggestion: () => void;
        rateAndReview: () => void;
    };
    sourceFileOpener: {
        openAtLine: (filePath: string, line: number | null) => void;
        openFiles: (filePaths: string[]) => void;
    };
};

export type VsMessageEvent<T> = MessageEvent<Payload<T>>;

export interface VsWebview {
    addEventListener: (
        type: string,
        listener: (msgEvent: VsMessageEvent<unknown>) => void
    ) => void;
    hostObjects: VsHostObjects;
}

export type VsChromeWebViewWindow = typeof window & {
    chrome: {
        webview: VsWebview;
    };
};
