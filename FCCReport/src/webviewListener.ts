import { VsChromeWebViewWindow, Payload } from "./webviewTypes";

type Listener<TPayload = unknown> = (payload: TPayload) => void;
type VsMessageEvent<T> = MessageEvent<Payload<T>>;

const chromeWebViewWindow = window as unknown as VsChromeWebViewWindow;

let webViewListening = false;
const listenerMap: Map<string, Listener[]> = new Map();

function addListener(payloadType: string, listener: Listener) {
    let listeners = listenerMap.get(payloadType);
    if (!listeners) {
        listeners = [];
        listenerMap.set(payloadType, listeners);
    }
    listeners.push(listener);
}

export function webviewPayloadTypeListen<TPayload>(
    payloadType: string,
    listener: Listener<TPayload>
) {
    addListener(payloadType, listener as Listener);

    if (!webViewListening) {
        listenForWebViewMessage();
        webViewListening = true;
    }
}

function listenForWebViewMessage() {
    chromeWebViewWindow.chrome.webview.addEventListener(
        "message",
        (msgEvent: VsMessageEvent<unknown>) => {
            const payload = msgEvent.data;
            const listeners = listenerMap.get(payload.type);
            if (listeners) {
                for (const listener of listeners) {
                    listener(payload.data);
                }
            }
        }
    );
}

export function webviewPayloadTypeUnlisten<TPayload>(
    payloadType: string,
    listener: (payload: TPayload) => void
) {
    const listeners = listenerMap.get(payloadType);
    if (listeners) {
        const newListeners = listeners.filter((l) => l !== listener);
        if (newListeners.length === 0) {
            listenerMap.delete(payloadType);
        } else {
            listenerMap.set(payloadType, newListeners);
        }
    }
}
