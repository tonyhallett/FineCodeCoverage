export interface Payload<T>{
    type:string,
    data:T
}

interface Webview{
    addEventListener:(type: string, listener: (msgEvent: MessageEvent<any>) => void) => void;
    
}

type Listener<TPayload = any> = (payload:TPayload) => void;

const anyWindow = window as any;

let webViewListening = false;
const listenerMap:Map<string,((data:any)=> void)[]> = new Map();

function addListener(payloadType:string,listener:Listener){
    if(!listenerMap.has(payloadType)){
        listenerMap.set(payloadType,[]);
    }
    const listeners = listenerMap.get(payloadType)!;
    listeners.push(listener);
}



export function webviewPayloadTypeListen<TPayload>(payloadType:string,listener:(payload:TPayload) => void){
    // undefined or null ?
    const webview: Webview | undefined = anyWindow.chrome?.webview;
    if (webview){
        addListener(payloadType, listener);

        if (!webViewListening){
            
            listenForWebViewMessage(webview);
            webViewListening = true;
        }
    }
}

function listenForWebViewMessage(webview:Webview){
    webview.addEventListener('message',(msgEvent:MessageEvent<Payload<unknown>>) => {
        const payload = msgEvent.data;
        if(listenerMap.has(payload.type)){
            const listeners = listenerMap.get(payload.type)!;
            for(let listener of listeners){
                listener(payload.data);
            }
        }
    });
}

export function webviewPayloadTypeUnlisten<TPayload>(payloadType:string,listener:(payload:TPayload) => void){
    if(listenerMap.has(payloadType)){
        // todo remove the chromewebview listener
        const listeners = listenerMap.get(payloadType)!;
        const newListeners = listeners.filter(l => l !== listener);
        if(newListeners.length === 0){
            listenerMap.delete(payloadType);
        }else{
            listenerMap.set(payloadType, newListeners);
        }
    }
}