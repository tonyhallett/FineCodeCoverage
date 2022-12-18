import { Payload } from "./webviewTypes";

type MockWebViewWindow = typeof window & {
    invokeMessageEvent: (msgEvent: MessageEvent<Payload<unknown>>) => void;
    chrome: {
        webview: {
            addEventListener: (
                _: string,
                listener: (msgEvent: MessageEvent<Payload<unknown>>) => void
            ) => void;
        };
    };
};

export function mockWebViewIfRequired() {
    if (process.env.MOCK_WEBVIEW) {
        const mockWebViewWindow = window as unknown as MockWebViewWindow;
        let reactListener: (msgEvent: MessageEvent<Payload<unknown>>) => void;
        mockWebViewWindow.invokeMessageEvent = (
            msgEvent: MessageEvent<Payload<unknown>>
        ) => {
            reactListener(msgEvent);
        };
        mockWebViewWindow.chrome.webview = {
            addEventListener: (
                _: string,
                listener: (msgEvent: MessageEvent<Payload<unknown>>) => void
            ) => {
                reactListener = listener;
            },
        };
    }
}
