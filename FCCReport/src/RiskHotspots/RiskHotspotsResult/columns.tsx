import { IColumn } from "@fluentui/react";
import { OpenFile } from "../../OpenFile";
import { RiskHotspotsAnalysisThresholds } from "../../types";
import { HotspotItem } from "./hotspotItem";
import { stringFieldSort } from "./sorting";

export interface ISortableColumn extends IColumn{
  sortItems:(items:any[],ascending:boolean) => any[],
  columnIdentifier:string
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

  export const assemblyColumn = hotspotItemColumnFactory("assemblyDisplay","Assembly");

  export const classColumn = renderingHotspotItemColumnFactory(
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
  
  export const methodColumn = renderingHotspotItemColumnFactory(
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

  export function getColumns(
    metricColumnNames:string[],
    riskHotspotAnalysisThresholds:RiskHotspotsAnalysisThresholds
  ):ISortableColumn[]{
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