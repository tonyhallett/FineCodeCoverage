import { RiskHotspotsAnalysisThresholds } from './types';
import { SimpleTable } from './SimpleTable';
import { ToolWindowText } from './vs styling/ToolWindowText';

export interface INoRiskHotspotsResultProps { 
  riskHotspotsAnalysisThresholds: RiskHotspotsAnalysisThresholds 
}

export function NoRiskHotspotsResult(props: INoRiskHotspotsResultProps) {
  const thresholds = [];
  thresholds.push({ threshold: 'Cyclomatic complexity :', value: props.riskHotspotsAnalysisThresholds.MetricThresholdForCyclomaticComplexity});
  thresholds.push({ threshold: 'Crap score :', value: props.riskHotspotsAnalysisThresholds.MetricThresholdForCrapScore});
  thresholds.push({ threshold: 'NPath complexity :', value: props.riskHotspotsAnalysisThresholds.MetricThresholdForCrapScore});

  return <div>
    <ToolWindowText styles={{root:{marginLeft:10}}}>No risk hotspots for thresholds :</ToolWindowText>
    <SimpleTable
      items={thresholds}
      columns={[
        { key: 'threshold', fieldName: 'threshold', isRowHeader: true, name: "Threshold", minWidth: 200, maxWidth: 200 },
        { key: 'value', fieldName: 'value', name: "Value", minWidth: 100 }
      ]}
      />
  </div>
}

