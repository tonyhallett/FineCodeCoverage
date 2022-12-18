import { useState } from "react";
import { Pivot, PivotItem } from "@fluentui/react";
import { Coverage } from "./coverage/Coverage";
import { Log } from "./Log";
import { Summary } from "./Summary";
import { RiskHotspots } from "./RiskHotspots/RiskHotspots";
import { LogMessage, Report, ReportOptions } from "./types";
import { Feedback } from "./Feedback";

export type ReportTabProps = {
    standalone: boolean;
    report: Report | undefined;
    reportOptions: ReportOptions;
    logMessages: LogMessage[];
    clearLogMessages: () => void;
};

const coverageKey = "CoverageTab";
const riskHotspotsKey = "RiskHotspotsTab";

export function ReportTab(props: ReportTabProps) {
    const [selectedTabKey, setSelectedTabKey] = useState(coverageKey);
    const { standalone, report, reportOptions, logMessages, clearLogMessages } =
        props;
    const { namespacedClasses, stickyCoverageTable } = reportOptions;
    const hasReport = !!report;

    const summaryComponent = hasReport ? (
        <Summary summaryResult={report.summaryResult} />
    ) : (
        <></>
    );

    const coverageTabActive = selectedTabKey == coverageKey;
    const riskHotspotsActive = selectedTabKey == riskHotspotsKey;
    const items: JSX.Element[] = [
        <PivotItem
            key={0}
            itemKey={coverageKey}
            headerText="Coverage"
            alwaysRender
        >
            {hasReport ? (
                <Coverage
                    active={coverageTabActive}
                    stickyCoverageTable={stickyCoverageTable}
                    standalone={standalone}
                    namespacedClasses={namespacedClasses}
                    summaryResult={report.summaryResult}
                    hideFullyCovered={reportOptions.hideFullyCovered}
                />
            ) : null}
        </PivotItem>,
        <PivotItem
            key={1}
            itemKey="SummaryTab"
            headerText="Summary"
            alwaysRender
        >
            {summaryComponent}
        </PivotItem>,
        <PivotItem
            key={2}
            itemKey={riskHotspotsKey}
            headerText="Risk Hotspots"
            alwaysRender
        >
            {hasReport ? (
                <RiskHotspots
                    namespacedClasses={namespacedClasses}
                    assemblies={report.summaryResult.assemblies}
                    riskHotspotAnalysisResult={report.riskHotspotAnalysisResult}
                    riskHotspotsAnalysisThresholds={
                        report.riskHotspotsAnalysisThresholds
                    }
                    standalone={standalone}
                    active={riskHotspotsActive}
                    stickyCoverageTable={stickyCoverageTable}
                />
            ) : null}
        </PivotItem>,
    ];

    if (!props.standalone) {
        items.push(
            <PivotItem
                key={3}
                itemKey="LogTab"
                headerText="Log"
                alwaysRender
            >
                <Log
                    logMessages={logMessages}
                    clearLogMessages={clearLogMessages}
                />
            </PivotItem>
        );
        items.push(
            <PivotItem
                key={4}
                itemKey="Feedback"
                headerText="Feedback"
                alwaysRender
            >
                <Feedback />
            </PivotItem>
        );
    }

    return (
        <Pivot
            linkFormat="tabs"
            styles={{
                itemContainer: {
                    marginTop: "5px", // necessary or the focus indicator is truncated
                },
            }}
            getTabId={(itemKey) => itemKey}
            focusZoneProps={{
                isCircularNavigation: true,
                onFocus: (evt) => {
                    setSelectedTabKey(evt.target.id);
                },
            }}
            selectedKey={selectedTabKey}
            aria-label={standalone ? "Coverage Report" : "Live Coverage Report"}
        >
            {items}
        </Pivot>
    );
}
