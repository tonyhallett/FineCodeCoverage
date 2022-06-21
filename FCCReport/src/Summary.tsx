import React from 'react';
import { SummaryResult } from './types';
import { SimpleTable } from './SimpleTable';

export interface SummaryProps{
    summaryResult: SummaryResult
  }
  
  export function possiblyNullDisplay(num:number | null):string{
    if(num === null){
      return "n/a";
    }
    return num.toString();
  }
  
  function possiblyNullDisplayCallback(num:number | null,cb:(n:number)=>string):string{
    if(num === null){
      return "n/a";
    }
    return cb(num);
  }
  
  export function possiblyNullPercentage(percentage:number | null, numerator:number, denominator:number){
    return possiblyNullDisplayCallback(percentage,p => `${p}% (${numerator} of ${denominator})` )
  }

export function Summary(props: SummaryProps) {
  const summaryRows: { key: string; display: string; }[] = [];
  const summaryResult = props.summaryResult;
  const assemblies = summaryResult.assemblies;
  summaryRows.push({ key: 'Assemblies :', display: assemblies.length.toString() });
  let numClasses = 0;
  let numFiles = 0;
  assemblies.forEach(assembly => {
    if (!assembly.classes) {
      alert("no classes");
    }
    assembly.classes.forEach(cl => {
      numClasses++;
      if (!cl.files) {
        alert('no files');
      }
      cl.files.forEach(_file => {
        numFiles++;
      });
    });
  });

  summaryRows.push({ key: 'Classes :', display: numClasses.toString() });
  summaryRows.push({ key: 'Files :', display: numFiles.toString() });
  summaryRows.push({ key: 'Covered lines :', display: summaryResult.coveredLines.toString() });
  const uncoveredLines = summaryResult.coverableLines - summaryResult.coveredLines;
  summaryRows.push({ key: 'Uncovered lines :', display: uncoveredLines.toString() });
  summaryRows.push({ key: 'Coverable lines :', display: summaryResult.coverableLines.toString() });
  summaryRows.push({ key: 'Total lines :', display: possiblyNullDisplay(summaryResult.totalLines) });
  summaryRows.push({ key: 'Line coverage :', display: possiblyNullPercentage(summaryResult.coverageQuota, summaryResult.coveredLines, summaryResult.coverableLines) });
  summaryRows.push({ key: 'Covered branches :', display: possiblyNullDisplay(summaryResult.coveredBranches) });
  summaryRows.push({ key: 'Total branches :', display: possiblyNullDisplay(summaryResult.totalBranches) });
  summaryRows.push({ key: 'Branch coverage :', display: possiblyNullPercentage(summaryResult.branchCoverageQuota, summaryResult.coveredBranches!, summaryResult.totalBranches!) });

  return <SimpleTable
    items={summaryRows}
    columns={[
      { key: 'key', fieldName: 'key', isRowHeader: true, name: "Key", minWidth: 200, maxWidth: 200 },
      { key: 'display', fieldName: 'display', name: "Display", minWidth: 100 }
    ]}
  />;
}
