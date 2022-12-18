import { LogMessage } from "./types";

export interface NewMessageAction {
    type: "newMessage";
    payload: LogMessage;
}

export interface ClearMessagesAction {
    type: "clear";
}

export function logMessagesReducer(
    logMessages: LogMessage[],
    action: NewMessageAction | ClearMessagesAction
): LogMessage[] {
    switch (action.type) {
        case "newMessage":
            return [action.payload, ...logMessages];
        default:
            return [];
    }
}
