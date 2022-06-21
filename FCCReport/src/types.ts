export interface Assembly {
    name : string,
    shortName : string,
    classes : Class[]
    id: number
}

export interface ClassCoverage{
  coveredLines:number,
  coverableLines:number,
  totalLines:number | null,
  coverageQuota:number | null,

  coveredBranches:number | null,
  totalBranches:number | null,
  branchCoverageQuota : number | null

  coveredCodeElements:number,
  totalCodeElements : number
  codeElementCoverageQuota: number | null
}

export enum CoverageType{
  LineCoverage,
  MethodCoverage
}

export type Class = ClassCoverage & {
  name: string
  displayName: string
  files: CodeFile[]
  assemblyIndex:number,
  coverageType:CoverageType  
}

export interface CodeFile{
    path:string
}



export interface SummaryResult{
    assemblies : Assembly[],
    coveredLines:number,
    coverableLines : number,
    totalLines : number | null, //nullable
    coverageQuota: number | null, // nullable
    coveredBranches : number | null, //nullable
    totalBranches : number | null, // nullable
    branchCoverageQuota : number | null, //nullable
  
    coveredCodeElements : number,
    totalCodeElements : number,
    codeElementCoverageQuota : number | null,//nullable

    supportsBranchCoverage : boolean
  }

export interface Report{
    riskHotspotAnalysisResult:RiskHotspotAnalysisResult,
    riskHotspotsAnalysisThresholds:RiskHotspotsAnalysisThresholds,
    summaryResult:SummaryResult
}
  
export interface RiskHotspotAnalysisResult{
    /*
        display differently if false  
    */
    codeCodeQualityMetricsAvailable : boolean,
    riskHotspots : RiskHotpot[]
}
  
  /*
    a RiskHotspot occurs when a MethodMetric, the container for Metric objects that apply to a method, 
    has a code quality metric ( MetricType == MetricType.CodeQuality) that exceeds the threshold.
    It stores all code quality Metric in MetricStatus objects - check Exceeded for those that are an issue
  
  */
  
  // applies to a method
  export interface RiskHotpot{ 
    assemblyIndex: number,
    classIndex: number,
    methodMetric:MethodMetric,
    fileIndex:number,
  
    statusMetrics:MetricStatus[]// this is what concerned with for columns
  }
  
  export interface MetricStatus{
    metricIndex:number,
    exceeded:boolean
  }
  
  export interface MethodMetric{
    metrics:Metric[],
  
    fullName:string,
    shortName:string,
    line:number | null
  }
  
export enum MetricType {
  CoveragePercentual,
  CoverageAbsolute,
  CodeQuality
}

export enum MetricMergeOrder {
  HigherIsBetter,
  LowerIsBetter
}

  export interface Metric {
    metricType:MetricType
    mergeOrder:MetricMergeOrder
    explanationUrl:string
    name:string,
    value : number | null
  }
  
  
  export interface RiskHotspotsAnalysisThresholds{
    MetricThresholdForCyclomaticComplexity: number,
    MetricThresholdForCrapScore: number,
    MetricThresholdForNPathComplexity: number
  }

  export interface CategoryColours{
    EnvironmentColors:{
      ToolWindowText:string,
      ToolWindowBackground:string
    },
    CommonControlsColors:{
  
    }
  }
  
  export interface FontStyling{
    fontSize:string,
    fontName:string
  }

  export interface Styling extends FontStyling{
    categoryColours:CategoryColours
  }

  export interface ReportOptions{
    namespacedClasses:boolean,
    hideFullyCovered:boolean
  }

  
  export enum MessageContext {
    Info,
    Warning,
    Error,
    CoverageStart,
    CoverageCancelled,
    CoverageCompleted,
    CoverageToolStart,
    CoverageToolCompleted,
    ReportGeneratorStart,
    ReportGeneratorCompleted,
    TaskStarted,
    TaskCompleted // file synchronization
  }
  export interface LogMessage{
    context:MessageContext,
    message:(Emphasized | FCCLink)[]
  }

export enum Emphasis{
  None = 0,
  Bold = 1,
  Italic = 2,
  Underline = 4
}

  export interface Emphasized{
    message:string,
    emphasis : Emphasis,
    type:'emphasized'
  }
  export interface FCCLink{
    hostObject:string
    methodName:string
    arguments?:any[]
    title:string,
    ariaLabel: string,
    type:'fcclink'
  }
  
  