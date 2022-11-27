import { RiskHotspotsManager, RiskHotspotsManagerProps } from "./RiskHotspotsManager";
import { INoRiskHotspotsResultProps as NoRiskHotspotsResultProps, NoRiskHotspotsResult } from "./NoRiskHotspotsResult";
import { Report } from './types';
import { VsSTyledToolWindowText } from './vs styling/VsStyledToolWindowText';

export type RiskHotspotProps = 
  NoRiskHotspotsResultProps & Omit<RiskHotspotsManagerProps,"riskHotspots"> & Pick<Report,'riskHotspotAnalysisResult'>
  /* Pick<Report,'riskHotspotAnalysisResult'> & 
  Pick<Report['summaryResult'],'assemblies'> & {standalone:boolean, active:boolean} &
  Pick<ReportOptions,"namespacedClasses"|"stickyCoverageTable"> */

export function RiskHotspots(props: RiskHotspotProps) {
  const { 
    riskHotspotAnalysisResult, 
    riskHotspotsAnalysisThresholds, 
    assemblies, 
    namespacedClasses, 
    standalone, 
    active,
    stickyCoverageTable
  } = props;
  if (!riskHotspotAnalysisResult) {
    return null;
  }
  if (!riskHotspotAnalysisResult.codeCodeQualityMetricsAvailable) {
    return <VsSTyledToolWindowText>No code quality metrics available</VsSTyledToolWindowText>;
  }
  if (riskHotspotAnalysisResult.riskHotspots.length === 0) {
    return <NoRiskHotspotsResult riskHotspotsAnalysisThresholds={riskHotspotsAnalysisThresholds} />;
  }
  return <RiskHotspotsManager 
          stickyCoverageTable={stickyCoverageTable}
          assemblies={assemblies} 
          riskHotspots={riskHotspotAnalysisResult.riskHotspots} 
          riskHotspotsAnalysisThresholds={riskHotspotsAnalysisThresholds} 
          namespacedClasses={namespacedClasses}
          standalone={standalone}
          active={active}
          />;
}
