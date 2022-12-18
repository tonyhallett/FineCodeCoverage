import { IActivityItemProps, Icon, IStyle, Stack } from "@fluentui/react";
import { Emphasis, LogMessage, MessageContext } from "./types";
import { VsStyledActionButton } from "./vs-styling/VsStyledActionButton";
import { VsStyledActivityItem } from "./vs-styling/VsStyledActivityItem";
import { VsSTyledToolWindowText } from "./vs-styling/VsStyledToolWindowText";
import { VsChromeWebViewWindow } from "./webviewTypes";

// todo use a map
function getIconNameForContext(messageContext: MessageContext) {
    switch (messageContext) {
        case MessageContext.Info:
            return "info";
        case MessageContext.Warning:
            return "warning";
        case MessageContext.Error:
            return "error";
        case MessageContext.CoverageStart:
            return "processing";
        case MessageContext.CoverageCancelled:
            return "processingCancelled";
        case MessageContext.TaskCompleted:
        case MessageContext.ReportGeneratorCompleted:
        case MessageContext.CoverageCompleted:
        case MessageContext.CoverageToolCompleted:
            return "completed";
        case MessageContext.ReportGeneratorStart:
            return "table";
        case MessageContext.CoverageToolStart:
            return "tool";
    }
}

function getIconNameForHostObjectMethod(hostObject: string) {
    if (hostObject === "fccOutputPane") {
        return "openPane";
    }
    return "navigate";
}

// todo is this necessary ?
function getActivityIconAriaLabelFromContext(messageContext: MessageContext) {
    switch (messageContext) {
        case MessageContext.CoverageCancelled:
            return "Coverage Cancelled";
        case MessageContext.CoverageCompleted:
            return "Coverage Completed";
        case MessageContext.CoverageStart:
            return "Coverage Start";
        case MessageContext.CoverageToolCompleted:
            return "Coverage Tool Completed";
        case MessageContext.CoverageToolStart:
            return "Coverage Tool Start";
        case MessageContext.Error:
            return "Error";
        case MessageContext.Info:
            return "Info";
        case MessageContext.ReportGeneratorCompleted:
            return "Report Completed";
        case MessageContext.ReportGeneratorStart:
            return "Report Start";
        case MessageContext.TaskCompleted:
            return "Task Completed";
        case MessageContext.TaskStarted:
            return "Task Started";
        case MessageContext.Warning:
            return "Warning";
    }
}

export function Log(props: {
    logMessages: LogMessage[];
    clearLogMessages: () => void;
}) {
    const { logMessages, clearLogMessages } = props;
    
    const activityItems: React.ReactElement[] = [];
    logMessages.forEach((logMessage, i) => {
        const activityDescription: React.ReactNode[] = logMessage.message.map(
            (msgPart, j) => {
                if (msgPart.type === "emphasized") {
                    const root: IStyle = {};
                    if (msgPart.emphasis & Emphasis.Bold) {
                        root.fontWeight = "bold";
                    }
                    if (msgPart.emphasis & Emphasis.Italic) {
                        root.fontStyle = "italic";
                    }
                    if (msgPart.emphasis & Emphasis.Underline) {
                        root.textDecoration = "underline";
                    }
                    return (
                        <VsSTyledToolWindowText
                            key={j}
                            styles={{ root }}
                        >
                            {msgPart.message}
                        </VsSTyledToolWindowText>
                    );
                } else {
                    const actionButton = (
                        <VsStyledActionButton
                            key={j}
                            style={{ marginLeft: "10px" }}
                            ariaLabel={msgPart.ariaLabel}
                            iconProps={{
                                iconName: getIconNameForHostObjectMethod(
                                    msgPart.hostObject,
                                ),
                            }}
                            onClick={() => {
                                const hostObject = (window as unknown as VsChromeWebViewWindow).chrome
                                    .webview.hostObjects[msgPart.hostObject];
                                const hostMethod =
                                    hostObject[msgPart.methodName];
                                const hostArguments = msgPart.arguments ?? [];
                                hostMethod(...hostArguments)
                            }}
                        >
                            {msgPart.title}
                        </VsStyledActionButton>
                    );
                    return actionButton;
                }
            }
        );

        const activityItemProps: Partial<IActivityItemProps> = {
            activityDescription,
            activityIcon: (
                <Icon
                    aria-label={getActivityIconAriaLabelFromContext(
                        logMessage.context
                    )}
                    styles={{ root: { marginLeft: "10px" } }}
                    iconName={getIconNameForContext(logMessage.context)}
                />
            ),
            styles: {
                root: {
                    alignItems: "center",
                },
                activityTypeIcon: {
                    height: "16px",
                },
            },

            isCompact: false,
        };

        activityItems.push(
            <VsStyledActivityItem
                {...activityItemProps}
                key={i}
            />
        );
    });
    return (
        <Stack
            horizontal
            verticalAlign="start"
        >
            <VsStyledActionButton
                ariaLabel="Clear log messages"
                iconProps={{ iconName: "logRemove" }}
                onClick={clearLogMessages}
            />
            <div>{activityItems}</div>
        </Stack>
    );
}
