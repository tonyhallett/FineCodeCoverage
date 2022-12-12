import { DetailsList, IColumn, IIconProps, SearchBox, IDropdownOption, Dropdown, SelectionMode, DetailsListLayoutMode, IDetailsHeaderProps, Sticky, Stack, Label, CheckboxVisibility } from '@fluentui/react';
import React, { useRef, useState } from 'react';
import { removeNamespaces } from './common';
import { OpenFile } from './OpenFile';
import { Assembly, ReportOptions, RiskHotpot, RiskHotspotsAnalysisThresholds } from './types';

export type RiskHotspotsResultProps = {
  riskHotspots:RiskHotpot[],
  assemblies:Assembly[],
  riskHotspotsAnalysisThresholds: RiskHotspotsAnalysisThresholds,
  standalone:boolean,
  active:boolean
} & Pick<ReportOptions,"namespacedClasses"|"stickyCoverageTable">

interface HotspotItem {
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

type MetricsWithStatusObject = {[prop:string]:MetricWithStatus}



interface MetricWithStatus{
  exceeded : boolean,
  name : string,
  value : number | null
}


interface ColumnSort{
  columnIdentifier:string,
  ascending:boolean
}

interface ISortableColumn extends IColumn{
  sortItems:(items:any[],ascending:boolean) => any[],
  columnIdentifier:string
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


  // to type
  function stringFieldSort(items:any[], ascending:boolean, fieldName:string):any[]{
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

  function sort(itemsToSort:HotspotItem[],columns:ISortableColumn[],columnSort:ColumnSort | undefined):HotspotItem[]{
    if(columnSort !== undefined){
      let sortColumn:ISortableColumn|undefined = undefined
      columns.forEach(column => {
        if(column.columnIdentifier === columnSort.columnIdentifier){
          column.isSorted = true,
          column.isSortedDescending = !columnSort.ascending
          sortColumn = column
        }else{
          column.isSorted = false
        }
      })
      return sortColumn!.sortItems(itemsToSort, !sortColumn!.isSortedDescending);
    }
    
    return itemsToSort;
  }

  // todo typescript mapping of key to be of string type
  
  function hotspotItemColumnFactory(fieldName:keyof HotspotItem, name:string):ISortableColumn{
    const column:ISortableColumn = {
      key:fieldName,
      name:name,
      columnIdentifier: fieldName,
      fieldName,
      minWidth:100,
      sortItems:(items:HotspotItem[], ascending : boolean) => {
        return stringFieldSort(items,ascending,fieldName);
      }
    }
    return column;
  }

  function renderingHotspotItemColumnFactory(
    fieldName:keyof HotspotItem,
    name:string,
    onRender:(item:HotspotItem)=> React.ReactElement
    ):ISortableColumn{
      const column = hotspotItemColumnFactory(fieldName,name);
      column.onRender = onRender;
      return column;
    }

  
  const assemblyColumn = hotspotItemColumnFactory("assemblyDisplay","Assembly");

  /* const assemblyColumn:ISortableColumn = {
    key:'assembly',
    name:'Assembly',
    columnIdentifier: 'assemblyDisplay',
    fieldName:'assemblyDisplay',
    minWidth:100,
    sortItems:(items:HotspotItem[], ascending : boolean) => {
      return stringFieldSort(items,ascending,assemblyFieldName);
    }
  } */


  const classColumn = renderingHotspotItemColumnFactory(
    "classDisplay",
    "Class",
    (item:HotspotItem) => {
      if(item.standalone){
        return <span>{item.classDisplay}</span>;
      }
      const toOpenAriaLabel = `class ${item.classDisplay}`;
      return <OpenFile type='class' toOpenAriaLabel={toOpenAriaLabel} filePaths={item.classPaths}  display={item.classDisplay}/>
    }
    )
  /* const classColumn : ISortableColumn = {
    key:'class',
    name:'Class',
    columnIdentifier:classFieldName,
    minWidth:100,
    onRender:(item:HotspotItem) => {
      if(item.standalone){
        return <span>{item.classDisplay}</span>;
      }
      const toOpenAriaLabel = `class ${item.classDisplay}`;
      return <OpenFile type='class' toOpenAriaLabel={toOpenAriaLabel} filePaths={item.classPaths}  display={item.classDisplay}/>
    },
    sortItems:(items:HotspotItem[], ascending : boolean) => {
      return stringFieldSort(items,ascending,classFieldName);
    }
  } */

  const methodColumn = renderingHotspotItemColumnFactory(
    "methodDisplay",
    "Method",
    (item:HotspotItem) => {
      if(item.standalone){
        return <span>{item.methodDisplay}</span>
      }
      const toOpenAriaLabel = `class ${item.classDisplay} at method ${item.methodDisplay}`;
      return <OpenFile toOpenAriaLabel={toOpenAriaLabel} type='hotspot' filePath={item.filePath} methodLine={item.methodLine} display={item.methodDisplay}/>
    }
    )
  /* const methodColumn : ISortableColumn = {
    key:'method',
    name:'Method',
    minWidth:100,
    columnIdentifier:methodFieldName,
    onRender:(item:HotspotItem) => {
      if(item.standalone){
        return <span>{item.methodDisplay}</span>
      }
      const toOpenAriaLabel = `class ${item.classDisplay} at method ${item.methodDisplay}`;
      return <OpenFile toOpenAriaLabel={toOpenAriaLabel} type='hotspot' filePath={item.filePath} methodLine={item.methodLine} display={item.methodDisplay}/>
    },
    sortItems:(items:HotspotItem[], ascending : boolean) => {
      return stringFieldSort(items,ascending,methodFieldName);
    }
  } */

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
        isResizable:true,
        columnIdentifier:metricColumnName,
        onRender:(item:HotspotItem) => {
          const metricWithStatus = item.metrics[metricColumnName]
          return <div>{metricWithStatus.value} {metricWithStatus.exceeded}</div> //todo
        },
        sortItems:(items:HotspotItem[], ascending : boolean) => {
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
    for(const col of columns){
      col.isResizable = true,
      col.calculatedWidth = 0;
      col.flexGrow = undefined;
    }

    columns[columns.length-1].flexGrow = 1;
    return columns;
  }

  function filterByAssembly(
    items:HotspotItem[],
    selectedAssemblyFilterOption:IDropdownOption<Assembly>|undefined,
    allAssembliesKey:string|number
  ):HotspotItem[]{
    let filteredByAssembly = items;
    assemblyColumn.isFiltered = false;
    if(selectedAssemblyFilterOption && selectedAssemblyFilterOption.key !== allAssembliesKey){
      filteredByAssembly = items.filter(item => item.assembly === selectedAssemblyFilterOption.data!);
      assemblyColumn.isFiltered = true;
    }
    return filteredByAssembly;
  }

  function filterByClass(
    items:HotspotItem[],
    classDisplayFilter:string|undefined, 
  ):HotspotItem[] {
    if(classDisplayFilter === undefined || classDisplayFilter === ''){
      classColumn.isFiltered = false;
      return items;
    }

    classColumn.isFiltered = true;
    return items.filter(item => {
      return item.classDisplay.toLowerCase().indexOf(classDisplayFilter.toLowerCase()) != -1
    })
    
  }

  function filterItems(
    items:HotspotItem[], 
    selectedAssemblyFilterOption:IDropdownOption<Assembly>|undefined,
    allAssembliesKey:string|number,
    classDisplayFilter:string|undefined, 
  ):any[]{
    const filteredByAssembly = filterByAssembly(items,selectedAssemblyFilterOption, allAssembliesKey);
    return filterByClass(filteredByAssembly, classDisplayFilter);
  }



const allAssembliesOption : IDropdownOption<Assembly> = {
  key:"All assemblies!",
  text:'All assemblies',
}

export function RiskHotspotsResult(props: RiskHotspotsResultProps) {
  const [columnSort, setColumnSort] = useState<ColumnSort|undefined>()
  const [classDisplayFilter, setClassDisplayFilter] = useState<string>();
  const [selectedAssemblyFilterOption,setSelectedAssemblyFilterOption] = useState<IDropdownOption<Assembly>>(allAssembliesOption);

  const { 
    riskHotspots,
    assemblies,
    namespacedClasses, 
    riskHotspotsAnalysisThresholds, 
    standalone, 
    active,
    stickyCoverageTable
  } = props;

  const onRenderDetailsHeader = React.useCallback((detailsHeaderProps: IDetailsHeaderProps | undefined, defaultRender: any) => {
    detailsHeaderProps!.styles={
      root:{
        paddingTop:'0px' 
      },
    }
    return active && stickyCoverageTable ? <Sticky>
      {defaultRender(detailsHeaderProps)}
    </Sticky> : defaultRender(detailsHeaderProps);
  },[active,stickyCoverageTable]);

  const [items, metricColumnNames,assemblyFilterDropDownOptions] = React.useMemo(() => {
    const items:HotspotItem[] = [];
    const metricColumnNames:string[] = [];
  
    const assemblyFilterDropDownOptions:IDropdownOption<Assembly>[] = [
      allAssembliesOption
    ];

    riskHotspots.forEach((riskHotspot) => {
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

      const key = `${assembly.name}${_class.name}${methodMetric.fullName}`;
      const hotspotView:HotspotItem = {
        key,
        assemblyDisplay:assembly.shortName,
        assembly,
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

    return [items,metricColumnNames,assemblyFilterDropDownOptions]
  },[riskHotspots, namespacedClasses])

  /* const columns = React.useMemo(() => {
    return getColumns(metricColumnNames,riskHotspotsAnalysisThresholds);
  },[metricColumnNames,riskHotspotsAnalysisThresholds]) */
  
  const columns = getColumns(metricColumnNames,riskHotspotsAnalysisThresholds);

  const filteredItems = React.useMemo(() => {
    return filterItems(
      items, 
      selectedAssemblyFilterOption,
      allAssembliesOption.key!,
      classDisplayFilter,
    );
  },[items,classDisplayFilter,selectedAssemblyFilterOption]);
  
  const filteredAndSortedItems = React.useMemo(() => {
    const filteredAndSortedItems = sort(filteredItems,columns,columnSort);
    return filteredAndSortedItems;
  },[filteredItems,columns,columnSort])
  
  return <div>
    <Stack style={{margin:'10px 0px'}} horizontal horizontalAlign='space-between' verticalAlign='center'>
        <Dropdown
          placeholder='All assemblies' 

          options={assemblyFilterDropDownOptions} 
          onChange={(_,option) => setSelectedAssemblyFilterOption(option!)} 
          selectedKey={selectedAssemblyFilterOption?.key}
          styles={
            {
              root:{
                width:"200px"
              }
            }
          }
          />
        <SearchBox styles={
          {
            root:{
              width:'200px'
            }
          }
        } iconProps={{iconName:'filter'}} 
          placeholder='Filter by class' 
          value={classDisplayFilter} 
          onChange={(_,newValue) => setClassDisplayFilter(newValue)}/>
    </Stack>
    <DetailsList 
      styles={
        {
          root:{
            clear:"both"
          }
        }
      }
       
      selectionMode={SelectionMode.single}
      checkboxVisibility={CheckboxVisibility.hidden} 
      items={filteredAndSortedItems} 
      columns={columns} 
      layoutMode={DetailsListLayoutMode.fixedColumns}
      onRenderDetailsHeader={onRenderDetailsHeader}
      onRenderRow={(rowProps, defaultRender) => {
        rowProps!.styles = {
          fields:{
            alignItems:"center"
          },
        }
        return defaultRender!(rowProps);
      }}
      onColumnHeaderClick={(_, column) => {
        const sortableColumn = column as ISortableColumn;
        setColumnSort((current) => {
          if(current && current.columnIdentifier === sortableColumn.columnIdentifier){
            return {
              columnIdentifier:sortableColumn.columnIdentifier,
              ascending: !current.ascending
            }
          }
          return {
            columnIdentifier:sortableColumn.columnIdentifier!,
            ascending:true
          }
        })
      
    }}/>
    </div>
}
