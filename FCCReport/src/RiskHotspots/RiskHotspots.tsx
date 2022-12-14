import {
  RiskHotspotsResult,
  RiskHotspotsResultProps,
} from "./RiskHotspotsResult/RiskHotspotsResult";
import {
  INoRiskHotspotsResultProps as NoRiskHotspotsResultProps,
  NoRiskHotspotsResult,
} from "./NoRiskHotspotsResult";
import { Report } from "../types";
import { VsSTyledToolWindowText } from "../vs-styling/VsStyledToolWindowText";

export type RiskHotspotProps = NoRiskHotspotsResultProps &
  Omit<RiskHotspotsResultProps, "riskHotspots"> &
  Pick<Report, "riskHotspotAnalysisResult">;

export function RiskHotspots(props: RiskHotspotProps) {
  const {
    riskHotspotAnalysisResult,
    riskHotspotsAnalysisThresholds,
    assemblies,
    namespacedClasses,
    standalone,
    active,
    stickyCoverageTable,
  } = props;
  if (!riskHotspotAnalysisResult) {
    return null;
  }
  if (!riskHotspotAnalysisResult.codeCodeQualityMetricsAvailable) {
    return (
      <VsSTyledToolWindowText>
        No code quality metrics available
      </VsSTyledToolWindowText>
    );
  }
  if (riskHotspotAnalysisResult.riskHotspots.length === 0) {
    return (
      <NoRiskHotspotsResult
        riskHotspotsAnalysisThresholds={riskHotspotsAnalysisThresholds}
      />
    );
  }
  return (
    <RiskHotspotsResult
      stickyCoverageTable={stickyCoverageTable}
      assemblies={assemblies}
      riskHotspots={riskHotspotAnalysisResult.riskHotspots}
      riskHotspotsAnalysisThresholds={riskHotspotsAnalysisThresholds}
      namespacedClasses={namespacedClasses}
      standalone={standalone}
      active={active}
    />
  );
}
