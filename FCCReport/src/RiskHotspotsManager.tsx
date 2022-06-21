import { DetailsList, IColumn, IIconProps, SearchBox, IDropdownOption, Dropdown, SelectionMode } from '@fluentui/react';
import React, { useRef, useState } from 'react';
import { removeNamespaces } from './common';
import { OpenFileButton } from './OpenFileButton';
import { Assembly, RiskHotpot, RiskHotspotsAnalysisThresholds } from './types';

export interface RiskHotspotsManagerProps {
  riskHotspots:RiskHotpot[],
  assemblies:Assembly[],
  riskHotspotsAnalysisThresholds: RiskHotspotsAnalysisThresholds,
  namespacedClasses:boolean,
  standalone:boolean
}

interface HotspotView {
  key:string,
  assemblyDisplay:string,
  filePath:string,
  methodLine:number | null,
  methodDisplay:string,
  classDisplay:string,
  classPaths:string[],
  metrics:MetricsWithStatusObject,
  standalone:boolean
}

type MetricsWithStatusObject = {[prop:string]:MetricWithStatus}



interface MetricWithStatus{
  exceeded : boolean,
  name : string,
  value : number | null
}


interface ColumnSort{
  columnFieldName:string|undefined,
  ascending:boolean
}

interface ISortableColumn extends IColumn{
  sortItems:(items:any[],ascending:boolean) => any[]
}

function caseInsensitiveStringSort(item1:string,item2:string){
  const first = item1.toUpperCase();
  const second = item2.toUpperCase();
  if (first < second) {
    return -1;
  }
  if (first > second) {
    return 1;
  }
  return 0;
}

  // to type such that is a string - generic ?
  function stringFieldSort(items:any[], ascending:boolean, fieldName:any):any[]{
    return items.sort((item1,item2) => {
      const first = item1[fieldName]
      const second = item2[fieldName]
      let result = caseInsensitiveStringSort(first, second)
      if(!ascending){
        result = -result;
      }
      return result;
    })
  }

  function sort(itemsToSort:any[],columns:ISortableColumn[],sortDetails:ColumnSort):any[]{
    if(sortDetails.columnFieldName !== undefined){
      let sortColumn:ISortableColumn|undefined = undefined
      columns.forEach(column => {
        if(column.fieldName == sortDetails.columnFieldName){
          column.isSorted = true,
          column.isSortedDescending = !sortDetails.ascending
          sortColumn = column
        }else{
          column.isSorted = false
        }
      })
      return sortColumn!.sortItems(itemsToSort, !sortColumn!.isSortedDescending);
    }
    
    return itemsToSort;
  }

  // todo typescript mapping
  const assemblyFieldName = 'assemblyDisplay'
  const classFieldName = 'classDisplay';
  const methodFieldName = 'methodDisplay'


  const assemblyColumn:ISortableColumn = {
    key:'assembly',
    name:'Assembly',
    fieldName: assemblyFieldName,
    minWidth:100,
    sortItems:(items:HotspotView[], ascending : boolean) => {
      return stringFieldSort(items,ascending,assemblyFieldName);
    }
  }

  
  const classColumn : ISortableColumn = {
    key:'class',
    name:'Class',
    fieldName:classFieldName,
    minWidth:100,
    onRender:(item:HotspotView) => {
      if(item.standalone){
        return <span>{item.methodDisplay}</span>;
      }
      return <OpenFileButton type='class' filePaths={item.classPaths}  display={item.classDisplay}/>
    },
    sortItems:(items:HotspotView[], ascending : boolean) => {
      return stringFieldSort(items,ascending,classFieldName);
    }
  }

  const methodColumn : ISortableColumn = {
    key:'method',
    name:'Method',
    minWidth:100,
    fieldName:methodFieldName,
    onRender:(item:HotspotView) => {
      if(item.standalone){
        return <span>{item.methodDisplay}</span>
      }
      return <OpenFileButton type='hotspot' filePath={item.filePath} methodLine={item.methodLine} display={item.methodDisplay}/>
    },
    sortItems:(items:HotspotView[], ascending : boolean) => {
      return stringFieldSort(items,ascending,methodFieldName);
    }
  }

  function getColumns(metricColumnNames:string[],riskHotspotAnalysisThresholds:RiskHotspotsAnalysisThresholds):ISortableColumn[]{
    const columns:ISortableColumn[] = [assemblyColumn, classColumn, methodColumn];
    metricColumnNames.forEach(metricColumnName => {
      let threshold:number|undefined;
      switch(metricColumnName){
        case "Cyclomatic complexity":
          threshold = riskHotspotAnalysisThresholds.MetricThresholdForCyclomaticComplexity;
          break;
        case "NPath complexity":
          threshold = riskHotspotAnalysisThresholds.MetricThresholdForNPathComplexity;
          break;
        case "Crap Score":
          threshold = riskHotspotAnalysisThresholds.MetricThresholdForCrapScore;
      }

      const columnName = threshold === undefined ? metricColumnName : `${metricColumnName} (${threshold})`;

      const metricColumn:ISortableColumn = {
        key:metricColumnName,
        name:columnName,
        minWidth:100,
        fieldName:metricColumnName, // unnecessary ? - if using onRender
        onRender:(item:HotspotView) => {
          const metricWithStatus = item.metrics[metricColumnName]
          return <div>{metricWithStatus.value} {metricWithStatus.exceeded}</div> //todo
        },
        sortItems:(items:HotspotView[], ascending : boolean) => {
          return items.sort((item1,item2)=> {
            // for now assuming has a value
            const value1 = item1.metrics[metricColumnName].value!;
            const value2 = item2.metrics[metricColumnName].value!;
            let result = value1 - value2;
            if(!ascending){
              result = -result;
            }
            return result
          })
        }
      }
      columns.push(metricColumn);

    })
    return columns;
  }

  function filterItems(items:HotspotView[], filterByAssembly:IDropdownOption<Assembly>|undefined,filterText:string|undefined, allAssembliesKey:string):any[]{
    let filteredByAssembly = items;
    assemblyColumn.isFiltered = false;
    if(filterByAssembly && filterByAssembly.key !== allAssembliesKey){
      filteredByAssembly = items.filter(item => item.assemblyDisplay == filterByAssembly.data?.shortName);

      assemblyColumn.isFiltered = true;
    }

    if(filterText === undefined || filterText === ''){
      classColumn.isFiltered = false;
      return filteredByAssembly;
    }
    const filteredItems = filteredByAssembly.filter(item => {
      return item.classDisplay.toLowerCase().indexOf(filterText.toLowerCase()) != -1
    })
    classColumn.isFiltered = true;
    return filteredItems
  }



export function RiskHotspotsManager(props: RiskHotspotsManagerProps) {
  const [sortDetails, setSortDetails] = useState<ColumnSort>({columnFieldName:undefined,ascending:true})
  const [filterText, setFilterText] = useState<string>();
  const [filterByAssembly,setFilterByAssembly] = useState<IDropdownOption<Assembly>>();

  const { riskHotspots,assemblies,namespacedClasses, riskHotspotsAnalysisThresholds, standalone } = props;
  
  const items:HotspotView[] = [];
  const metricColumnNames:string[] = [];
  const allAssembliesKey = "All assemblies"
  const assemblyFilterDropDownOptions:IDropdownOption<Assembly>[] = [
    {
      key:allAssembliesKey,
      text:'All assemblies',
    }
  ];
  riskHotspots.forEach(riskHotspot => {
    const assembly = assemblies[riskHotspot.assemblyIndex];
    // set up assembly filtering
    if(!assemblyFilterDropDownOptions.find(ddo => {
      return ddo.data === assembly
    })){
      assemblyFilterDropDownOptions.push({
        key:assembly.name,
        text:assembly.shortName,
        data:assembly
      })
    }

    const assemblyDisplay = assembly.shortName;
    const _class = assembly.classes[riskHotspot.classIndex];
    const classPaths = _class.files.map(f => f.path);
    const filePath = _class.files[riskHotspot.fileIndex].path;
    const methodMetric = riskHotspot.methodMetric;
    const metrics = methodMetric.metrics
    const methodLine = methodMetric.line;
    const methodDisplay = methodMetric.shortName; // todo check the views - if necessary long form for method name
     
    const classDisplay = namespacedClasses ? _class.displayName : removeNamespaces(_class.displayName);
    
    const metricsWithStatusObject:MetricsWithStatusObject = {}
    riskHotspot.statusMetrics.forEach(statusMetric => {
      const metric = metrics[statusMetric.metricIndex];
      const metricName = metric.name;
      metricsWithStatusObject[metricName]={
        exceeded:statusMetric.exceeded,
        name:metricName,
        value:metric.value
      }
      if(!metricColumnNames.includes(metricName)){
        metricColumnNames.push(metricName);
      }
    });
    const hotspotView:HotspotView = {
      key:`${assemblyDisplay}${classDisplay}${methodDisplay}`,
      assemblyDisplay,
      filePath,
      methodLine,
      methodDisplay,
      classDisplay,
      classPaths,
      metrics:metricsWithStatusObject,
      standalone
    }
    items.push(hotspotView);
  });

  const columns = getColumns(metricColumnNames,riskHotspotsAnalysisThresholds);

  const filteredItems = filterItems(items, filterByAssembly,filterText,allAssembliesKey);

  let filteredAndSortedItems:any[] = sort(filteredItems,columns,sortDetails);
  
  
  return <div>
    <div>
    <Dropdown label='Filter by assembly' placeholder='All assemblies' options={assemblyFilterDropDownOptions} onChange={(_,option) => setFilterByAssembly(option)} selectedKey={filterByAssembly?.key}/>
    <SearchBox iconProps={{iconName:'filter'}} placeholder='Filter by class' value={filterText} onChange={(_,newValue) => setFilterText(newValue)}/>
    </div>
    <DetailsList selectionMode={SelectionMode.none} items={filteredAndSortedItems} columns={columns} onColumnHeaderClick={(_, column) => {
      setSortDetails((current) => {
        if(current.columnFieldName === column!.fieldName){
          return {
            columnFieldName:column!.fieldName,
            ascending: !current.ascending
          }
        }
        return {
          columnFieldName:column?.fieldName,
          ascending:true
        }
      })
    }}/>
    </div>
}
