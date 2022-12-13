import { Assembly } from "../../types";

export type MetricsWithStatusObject = {[prop:string]:MetricWithStatus}

interface MetricWithStatus{
    exceeded : boolean,
    name : string,
    value : number | null
}

export interface HotspotItem {
    key:string,
    assembly:Assembly,
    assemblyDisplay:string,
    filePath:string,
    methodLine:number | null,
    methodDisplay:string,
    classDisplay:string,
    classPaths:string[],
    metrics:MetricsWithStatusObject,
    standalone:boolean
  }