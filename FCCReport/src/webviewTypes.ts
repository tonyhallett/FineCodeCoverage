export interface Payload<T> {
    type: string;
    data: T;
}

export interface VsWebview {
    addEventListener: (
        type: string,
        listener: (msgEvent: MessageEvent<any>) => void
    ) => void;
    hostObjects:{
        fccResourcesNavigator:{
            buyMeACoffee:() => void;
            logIssueOrSuggestion:() => void;
            rateAndReview:() => void;
        }
    }
}

export type VsChromeWebViewWindow = typeof window & {
    chrome: {
        webview: VsWebview;
    };
};
