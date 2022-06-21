import React from 'react';
import { Label } from '@fluentui/react';
import { RiskHotspotsManager } from "./RiskHotspotsManager";
import { NoRiskHotspotsResult } from "./NoRiskHotspotsResult";
import { Report } from './types';

export type RiskHotspotProps = Pick<Report,'riskHotspotAnalysisResult'|'riskHotspotsAnalysisThresholds'> & Pick<Report['summaryResult'],'assemblies'> & {namespacedClasses:boolean, standalone:boolean}

export function RiskHotspots(props: RiskHotspotProps) {
  const { riskHotspotAnalysisResult, riskHotspotsAnalysisThresholds, assemblies, namespacedClasses, standalone } = props;
  if (!riskHotspotAnalysisResult) {
    return null;
  }
  if (!riskHotspotAnalysisResult.codeCodeQualityMetricsAvailable) {
    return <Label>No code quality metrics available</Label>;
  }
  if (riskHotspotAnalysisResult.riskHotspots.length === 0) {
    return <NoRiskHotspotsResult riskHotspotsAnalysisThresholds={riskHotspotsAnalysisThresholds} />;
  }
  return <RiskHotspotsManager 
          assemblies={assemblies} 
          riskHotspots={riskHotspotAnalysisResult.riskHotspots} 
          riskHotspotsAnalysisThresholds={riskHotspotsAnalysisThresholds} 
          namespacedClasses={namespacedClasses}
          standalone={standalone}
          />;
}
