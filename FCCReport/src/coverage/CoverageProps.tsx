import { ReportOptions, SummaryResult } from "../types";

export type CoverageProps = {
    summaryResult: SummaryResult;
    hideFullyCovered: boolean;
    standalone: boolean;
    active: boolean;
} & Pick<ReportOptions, "stickyCoverageTable" | "namespacedClasses">;
