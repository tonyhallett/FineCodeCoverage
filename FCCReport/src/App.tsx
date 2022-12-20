import React, { useEffect, useReducer, useState } from "react";
import { webviewPayloadTypeListen } from "./webviewListener";
import {
    CustomizerContext,
    ProgressIndicator,
    registerIcons,
    ScrollablePane,
} from "@fluentui/react";
import {
    LogMessage,
    MessageContext,
    Report,
    ReportOptions,
    Styling,
} from "./types";
import { ReportTab } from "./ReportTab";
import {
    SortDownIcon,
    SortUpIcon,
    ClearFilterIcon,
    FilterIcon,
    ChevronDownIcon,
    createSvgIcon,
    ChevronRightMedIcon,
    TagIcon,
    BeerMugIcon,
    GitHubLogoIcon,
    ReviewSolidIcon,
    InfoIcon,
    WarningIcon,
    CompletedIcon,
    TableIcon,
    ProcessingIcon,
    OpenPaneIcon,
    NavigateExternalInlineIcon,
    ErrorBadgeIcon,
    RunningIcon,
    DeveloperToolsIcon,
    ProcessingCancelIcon,
    LogRemoveIcon,
    GroupedDescendingIcon,
} from "@fluentui/react-icons-mdl2";
import { useRefInitOnce } from "./utilities/hooks/useRefInitOnce";
import {
    addScrollBarStyles,
    addVsHighContrastBlocker,
    getBodyStyles,
    VsCustomizerContext,
} from "./vs-styling/themeStyles";
import { useBodyStyling } from "./utilities/hooks/useBody";
import { mockWebViewIfRequired } from "./mocking";
import { logMessagesReducer } from "./logMessagesReducer";

//https://github.com/microsoft/fluentui/issues/22895
const VisualStudioIDELogo32Icon = createSvgIcon({
    svg: ({ classes }) => (
        <svg
            xmlns="http://www.w3.org/2000/svg"
            viewBox="0 0 2048 2048"
            className={classes.svg}
        >
            <path d="M2048 213v1622l-512 213L0 1536l1536 223V0l512 213zM245 1199l-117-39V590l117-39 283 213 470-465 282 119v913l-282 120-470-466-283 214zm430-324l323 244V631L675 875zm-430 169l171-169-171-170v339z" />
        </svg>
    ),
    displayName: "VisualStudioIDELogo32Icon",
});

/* const VisualStudioLogoIcon = createSvgIcon({
    svg: ({ classes }) => (
        <svg
            xmlns="http://www.w3.org/2000/svg"
            viewBox="0 0 2048 2048"
            className={classes.svg}
        >
            <path d="M1472 128l448 171v1429l-448 192-704-704-469 363-171-107V577l171-108 469 363 704-704zM320 1280l256-256-256-256v512zm1152 128V640l-448 384 448 384z" />
        </svg>
    ),
    displayName: "VisualStudioLogoIcon",
}); */

registerIcons({
    icons: {
        OpenFile: <VisualStudioIDELogo32Icon />,
        sortDown: <SortDownIcon />,
        sortUp: <SortUpIcon />,
        clear: <ClearFilterIcon />,
        filter: <FilterIcon />,
        chevrondown: <ChevronDownIcon />,
        chevronrightmed: <ChevronRightMedIcon />,
        tag: <TagIcon />,
        beerMug: <BeerMugIcon />,
        github: <GitHubLogoIcon />,
        review: <ReviewSolidIcon />,
        info: <InfoIcon />,
        warning: <WarningIcon />,
        error: <ErrorBadgeIcon />,
        completed: <CompletedIcon />,
        table: <TableIcon />,
        processing: <ProcessingIcon />,
        processingCancelled: <ProcessingCancelIcon />,
        openPane: <OpenPaneIcon />,
        navigate: <NavigateExternalInlineIcon />,
        running: <RunningIcon />,
        tool: <DeveloperToolsIcon />,
        logRemove: <LogRemoveIcon />,
        groupeddescending: <GroupedDescendingIcon />,
    },
});

mockWebViewIfRequired();

type PossiblyStandaloneWindow = typeof window & {
    styling: Styling | undefined;
    report: Report | undefined;
    reportOptions: ReportOptions | undefined;
};

const possiblyStandaloneWindow = window as unknown as PossiblyStandaloneWindow;

function App() {
    const standalone = !!possiblyStandaloneWindow.styling;
    const [logMessages, logMessagesDispatch] = useReducer(
        logMessagesReducer,
        []
    );
    const [stylingState, setStyling] = useState<Styling | undefined>(
        possiblyStandaloneWindow.styling
    );
    const [reportState, setReport] = useState<Report | undefined>(
        possiblyStandaloneWindow.report
    );
    const [coverageRunning, setCoverageRunning] = useState(false);
    const [reportOptionsState, setReportOptions] = useState<ReportOptions>(
        possiblyStandaloneWindow.reportOptions ?? {
            hideFullyCovered: false,
            namespacedClasses: true,
            stickyCoverageTable: false,
        }
    );

    const clearLogMessages = React.useCallback(() => {
        logMessagesDispatch({
            type: "clear",
        });
    }, []);

    useEffect(() => {
        function stylingListener(styling: Styling) {
            addScrollBarStyles(styling.categoryColours);
            addVsHighContrastBlocker(styling.themeIsHighContrast);
            setStyling(styling);
        }

        function reportOptionsListener(reportOptions: ReportOptions) {
            setReportOptions(reportOptions);
        }

        function reportListener(report: Report) {
            setReport(report);
        }

        function messageListener(logMessage: LogMessage) {
            if (logMessage.context === MessageContext.CoverageStart) {
                setCoverageRunning(true);
            }

            logMessagesDispatch({ type: "newMessage", payload: logMessage });
        }

        function coverageStoppedListener() {
            setCoverageRunning(false);
        }

        if (!standalone) {
            webviewPayloadTypeListen("report", reportListener);
            webviewPayloadTypeListen("styling", stylingListener);
            webviewPayloadTypeListen("reportOptions", reportOptionsListener);
            webviewPayloadTypeListen("message", messageListener);
            webviewPayloadTypeListen(
                "coverageStopped",
                coverageStoppedListener
            );
        }
    }, [standalone]);

    const customizationStyling = useRefInitOnce(new VsCustomizerContext());

    useBodyStyling(
        stylingState ? getBodyStyles(stylingState.categoryColours) : {}
    );

    if (!stylingState) {
        return null;
    }

    customizationStyling.current =
        customizationStyling.current.getNext(stylingState);

    return (
        <CustomizerContext.Provider value={customizationStyling.current}>
            <ScrollablePane>
                {standalone ? null : (
                    <ProgressIndicator
                        percentComplete={coverageRunning ? undefined : 0}
                    />
                )}
                <ReportTab
                    standalone={standalone}
                    report={reportState}
                    reportOptions={reportOptionsState}
                    logMessages={logMessages}
                    clearLogMessages={clearLogMessages}
                />
            </ScrollablePane>
        </CustomizerContext.Provider>
    );
}

export default App;
