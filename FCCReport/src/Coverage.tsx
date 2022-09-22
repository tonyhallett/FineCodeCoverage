import React, { useState } from 'react';
import { DetailsList, DetailsListLayoutMode, DetailsRow, GroupHeader, IColumn, IDetailsHeaderProps, IFocusZoneProps, IGroup, IGroupHeaderProps, IRenderFunction, ISearchBoxStyles, ISliderStyles, ProgressIndicator, ScrollablePane, SearchBox, SelectionMode, Slider, Stack, Sticky, TextField } from '@fluentui/react';
import { Assembly, Class, ClassCoverage, CoverageType, SummaryResult } from './types';
import { OpenFileButton } from './OpenFileButton';
import { removeNamespaces } from './common';

export interface CoverageProps{
  summaryResult:SummaryResult,
  namespacedClasses:boolean,
  hideFullyCovered:boolean,
  standalone:boolean,
  active:boolean
}

export interface ICoverageItemBase extends ClassCoverage{
  key:string,
  name:string,
  uncoveredLines:number,
  classPaths:string[] | undefined,
  
}

export interface ICoverageItem extends ICoverageItemBase{
  isFullyCovered:() => boolean
  totalLines:number
  coveredBranches:number;
  totalBranches:number;
  standalone:boolean
}

//#region columns

interface ICoverageColumn extends IColumn{
  fieldName:keyof ICoverageItemBase
}

interface NameColumn extends ICoverageColumn{
  setFiltered:(filtered:boolean) => void
}

const nameColumn:NameColumn = {
  key:'name',
  name:'Name',
  fieldName: "name",
  minWidth:100,
  onRender:(item:ICoverageItem) => {
    if(!item.standalone && item.classPaths){
      const toOpenAriaLabel = `class ${item.name}`;
      return <OpenFileButton toOpenAriaLabel={toOpenAriaLabel} type='class' filePaths={item.classPaths}  display={item.name}/>
    }
    return <span>{item.name}</span>
  },
  setFiltered(filtered:boolean){
    this.isFiltered = filtered;
    //this.name = filtered ? "Name ( class )" : "Name";
  }
}

const coveredColumnDisplay = "Covered";
const totalColumnDisplay = "Total";

const coveredLinesColumn:ICoverageColumn = {
  key:'coveredLines',
  name:coveredColumnDisplay,
  fieldName: "coveredLines",
  minWidth:100,
}

const coverableLinesColumn:ICoverageColumn = {
  key:'coverableLines',
  name:"Coverable",
  fieldName: "coverableLines",
  minWidth:100,
}

const uncoveredLinesColumn:ICoverageColumn = {
  key:'uncoveredLines',
  name:"Uncovered",
  fieldName: "uncoveredLines",
  minWidth:100,
}

const totalLinesColumn:ICoverageColumn = {
  key:'totalLines',
  name:totalColumnDisplay,
  fieldName: "totalLines",
  minWidth:100,
}

function renderPercentage(percentage:number | null){
  if(percentage === null){
    return null;
  }
  //todo type
  const progressBarColor = percentage === null ? "gray" : "green";
  return <span><ProgressIndicator percentComplete={percentage === null ? 1 : percentage/100} styles={
    {
      progressBar:{
        backgroundColor:progressBarColor
      },
      root:{
        display:'inline-block',
        width:'50px'
      }
      /* progressTrack:{
        backgroundColor:'red'
      } */
    }
  }/><span style={{marginLeft:'15px'}}>{percentage === null ? '' : percentage}</span>
  </span> 
  
  
}

const lineCoverageQuotaColumn:ICoverageColumn = {
  key:'coverageQuota',
  name:"Line Coverage",
  fieldName: "coverageQuota",
  minWidth:100,
  onRender(item:ICoverageItem){
    return renderPercentage(item.coverageQuota);
  }
}

const coveredBranchesColumn:ICoverageColumn = {
  key:'coveredBranches',
  name:coveredColumnDisplay,
  fieldName: "coveredBranches",
  minWidth:100,
}

const totalBranchesColumn:ICoverageColumn = {
  key:'totalBranches',
  name:totalColumnDisplay,
  fieldName: "totalBranches",
  minWidth:100,
}

const branchCoverageQuotaColumn:ICoverageColumn = {
  key:'branchCoverageQuota',
  name:"Branch Coverage",
  fieldName: "branchCoverageQuota",
  minWidth:100,
  onRender(item:ICoverageItem){
    return renderPercentage(item.branchCoverageQuota);
  }
}

const coveredCodeElementsColumn:ICoverageColumn = {
  key:'coveredCodeElements',
  name:coveredColumnDisplay,
  fieldName: "coveredCodeElements",
  minWidth:100,
}

const totalCodeElementsColumn:ICoverageColumn = {
  key:'totalCodeElements',
  name:totalColumnDisplay,
  fieldName: "totalCodeElements",
  minWidth:100,
}


const codeElementCoverageQuotaColumn:ICoverageColumn = {
  key:'codeElementCoverageQuota',
  name:"Element Coverage",
  fieldName: "codeElementCoverageQuota",
  minWidth:100,
  onRender(item:ICoverageItem){
    return renderPercentage(item.codeElementCoverageQuota);
  }
}

//#endregion

export function getGroupingMax(assemblies:Assembly[]): number{
  let groupingMax = 0;
    assemblies.forEach(assembly => {
      assembly.classes.forEach(cls => {
        // is 1 with a1.c1
        const classGroupingMax = cls.displayName.split('.').length-1;
        if(classGroupingMax > groupingMax){
          groupingMax = classGroupingMax;
        }
      })
    })
    return groupingMax;
}

const useProFeature = false;

interface ICoverageGroup extends IGroup, ICoverageItemBase{
  classPaths:undefined
  totalBranches:number
  coveredBranches:number
  totalLines:number,
  filter:(filter:string, hideFullyCovered:boolean) => void
  sort:(fieldName:keyof ICoverageItemBase,ascending:boolean) => void
  hideFullyCovered?:never
}


class CoverageItem implements ICoverageItem{
  coveredLines:number;
  coverableLines:number;
  uncoveredLines:number;
  totalLines:number 
  coverageQuota:number | null = null;

  coveredBranches:number;
  totalBranches:number;
  branchCoverageQuota : number | null;

  coveredCodeElements:number;
  totalCodeElements : number;
  codeElementCoverageQuota: number | null;

  key:string
  name:string
  classPaths: string[];
  standalone:boolean;
  constructor(cls:Class, namespacedClasses:boolean, standalone:boolean){
    this.standalone = standalone;
    this.name = namespacedClasses ? cls.displayName : removeNamespaces(cls.displayName);
    this.key = `${cls.assemblyIndex}${cls.name}`; // name or displayName
    this.classPaths = cls.files.map(f => f.path)

    this.coveredLines = cls.coveredLines;
    this.coverableLines = cls.coverableLines;
    this.uncoveredLines = cls.coverableLines - cls.coveredLines;
    this.totalLines = cls.totalLines === null ? 0 : cls.totalLines;
    if (this.coverableLines === 0){
      if (cls.coverageType === CoverageType.MethodCoverage && cls.coverageQuota !== null){
        this.coverageQuota = cls.coverageQuota;
      }
    } else {
      this.coverageQuota = getQuota(cls.coveredLines, cls.coverableLines);
    }
    
    this.totalBranches = cls.totalBranches === null ? 0 : cls.totalBranches;
    this.coveredBranches = cls.coveredBranches === null ? 0 : cls.coveredBranches;
    this.branchCoverageQuota = cls.branchCoverageQuota;

    this.coveredCodeElements = cls.coveredCodeElements;
    this.totalCodeElements = cls.totalCodeElements;
    this.codeElementCoverageQuota = cls.codeElementCoverageQuota;
  }
  filter(filter:string):boolean{
    return this.name.toLowerCase().indexOf(filter.toLowerCase()) > -1;
  }
  isFullyCovered():boolean{
    return this.coverageQuota !== null && this.coverageQuota === 100 && this.branchCoverageQuota !== null && this.branchCoverageQuota === 100;
  }
}

function getQuota(numerator:number, denominator:number){
  if(denominator === 0){
    return null;
  }
  return roundNumber(100 * numerator / denominator, 2);
}

function roundNumber(number: number, precision: number): number {
  return Math.floor(number * Math.pow(10, precision)) / Math.pow(10, precision);
}



class ClassesGroup implements ICoverageGroup{
  name:string
  key:string
  classPaths: undefined;
  // will be set from outside
  count:number = 0;
  startIndex:number = 0;

  // todo - this is common typing 
  coveredLines:number = 0
  coverableLines:number = 0;
  uncoveredLines: number = 0;
  totalLines:number = 0; 
  coverageQuota:number | null

  coveredBranches:number = 0;
  totalBranches:number= 0;
  branchCoverageQuota : number | null;

  coveredCodeElements:number = 0;
  totalCodeElements : number = 0;
  codeElementCoverageQuota: number | null;
  private _items:ICoverageItem[] = []
  level:number;
  items:ICoverageItem[] = [];
  ariaLabel:string

  constructor(classes:Class[],namespacedClasses:boolean,name:string,standalone:boolean,ariaLabel:string,level:number = 0){
    this.level = level;
    this.name = name;
    this.key = name;
    this.ariaLabel = ariaLabel;
    classes.forEach(cls => {
      const coverageItem = new CoverageItem(cls,namespacedClasses, standalone);
      this._items.push(coverageItem);
      
      this.coveredLines += coverageItem.coveredLines;
      this.coverableLines += coverageItem.coverableLines;
      this.uncoveredLines += coverageItem.uncoveredLines;
      this.totalLines += coverageItem.totalLines;
      
      this.coveredBranches += coverageItem.coveredBranches;
      this.totalBranches += coverageItem.totalBranches;

      this.coveredCodeElements += coverageItem.coveredCodeElements;
      this.totalCodeElements += coverageItem.totalCodeElements;

    });
    this.coverageQuota = getQuota(this.coveredLines, this.coverableLines);
    this.branchCoverageQuota = getQuota(this.coveredBranches, this.totalBranches);
    this.codeElementCoverageQuota = getQuota(this.coveredCodeElements, this.totalCodeElements);
  }

  filter(filter:string, hideFullyCoverered:boolean):void{
    
    this.items = this._items.filter(coverageItem => {
      if(hideFullyCoverered && coverageItem.isFullyCovered()){
        return false;
      }
      if(filter === ''){
        return true;
      }
      return coverageItem.name.toLowerCase().indexOf(filter.toLowerCase()) > -1;
    })
  }

  sort(fieldName:keyof ICoverageItemBase, ascending:boolean){
    sortCoverageItems(this.items,fieldName,ascending);
  }
}

class AssemblyGroup extends ClassesGroup{
  constructor(assembly:Assembly,namespacedClasses:boolean, standalone:boolean){
    super(assembly.classes,namespacedClasses,assembly.shortName,standalone, `Assembly ${assembly.name} class coverage`)
  }
}

class AllGroup extends ClassesGroup{
  
  constructor(assemblies:Assembly[],namespacedClasses:boolean, standalone:boolean){
    super(AllGroup.getClasses(assemblies),namespacedClasses,"All",  standalone, "All class coverage",);
  }
  static getClasses(assemblies:Assembly[]){
    const classes:any = []; // todo typing
    assemblies.forEach(assembly => {
      assembly.classes.forEach(cls => {
        classes.push(cls)
      })
    });
    return classes;
  }
}

function getGrouping(namespacedClass:string,level:number):string{
  const parts = namespacedClass.split('.');
  const namespaceParts = parts.length - 1;
  if(namespaceParts === 0){
    return "( Global )";
  }
  const takeParts = namespaceParts < level ? namespaceParts : level;
  return parts.slice(0,takeParts).join('.');
}

const nullableQuotaFields = ['coverageQuota','branchCoverageQuota','codeElementCoverageQuota'];//todo type so no !
function numericSort(left:ICoverageItemBase, right:ICoverageItemBase,fieldName:keyof ICoverageItemBase, smaller:number,bigger:number):number{
  return left[fieldName] === right[fieldName] ?
        0
        : (left[fieldName]! < right[fieldName]! ? smaller : bigger);
}


// todo type keyof to non function fields
function numericNullableSort(left:ICoverageItemBase, right:ICoverageItemBase,fieldName:keyof ICoverageItemBase, smaller:number,bigger:number):number{
  const leftValue = left[fieldName];
  const rightValue = right[fieldName];
  if (leftValue === rightValue) {
    return 0;
} else if (leftValue === null || leftValue === undefined) {
    return smaller;
} else if (rightValue === null || rightValue === undefined) {
    return bigger;
} else {
    return leftValue < rightValue ? smaller : bigger;
}

}


function sortCoverageItems<TCoverageItem extends ICoverageItemBase>(coverageItems:TCoverageItem[], fieldName:keyof ICoverageItemBase ,ascending:boolean) {
  let smaller: number = ascending ? -1 : 1;
  let bigger: number = ascending ? 1 : -1;
  let sortMethod = numericSort;
  if(nullableQuotaFields.some(nullableQuotaField => nullableQuotaField === fieldName)){
    sortMethod = numericNullableSort;
  }
  coverageItems.sort((left, right) =>  {
    return sortMethod(left,right,fieldName,smaller,bigger);
  })


}

// even if have no behaviour a base group for presence of fields useful
class NamespacedGroup implements ICoverageGroup{
  name:string
  key:string
  classPaths: undefined;
  // will be set from outside
  count:number = 0;
  startIndex:number = 0;

  // todo - this is common typing 
  coveredLines:number = 0
  coverableLines:number = 0;
  uncoveredLines: number = 0;
  totalLines:number = 0; 
  coverageQuota:number | null

  coveredBranches:number = 0;
  totalBranches:number= 0;
  branchCoverageQuota : number | null;

  coveredCodeElements:number = 0;
  totalCodeElements : number = 0;
  codeElementCoverageQuota: number | null;
  
  children:ClassesGroup[] = [];
  level:0 = 0
  ariaLabel:string
  constructor(assembly:Assembly,namespacedClasses:boolean, grouping:number, isStandalone:boolean){
    this.name = assembly.shortName;
    this.key = assembly.name;// will need a different key ?
    this.ariaLabel = `${assembly.name} class coverage`;
    const map:Map<string,Class[]> = new Map();
    assembly.classes.forEach(cls => {
      const namespaceGroupingName = getGrouping(cls.displayName,grouping);
      if(!map.has(namespaceGroupingName)){
        map.set(namespaceGroupingName,[]);
      }
      const classes = map.get(namespaceGroupingName)!;
      classes.push(cls);
    });
    map.forEach((classes,groupingName) => {
      // todo if just add the assembly to the class will be much better than iterating again
      this.children.push(new ClassesGroup(classes,namespacedClasses,groupingName,isStandalone,`Namespace ${groupingName} class coverage`,1));
    })
    this.children.forEach(childGroup => {
      this.coveredLines += childGroup.coveredLines;
      this.coverableLines += childGroup.coverableLines;
      this.uncoveredLines += childGroup.uncoveredLines;
      this.totalLines += childGroup.totalLines;
      
      this.coveredBranches += childGroup.coveredBranches;
      this.totalBranches += childGroup.totalBranches;

      this.coveredCodeElements += childGroup.coveredCodeElements;
      this.totalCodeElements += childGroup.totalCodeElements;
    });
    this.coverageQuota = getQuota(this.coveredLines, this.coverableLines);
    this.branchCoverageQuota = getQuota(this.coveredBranches, this.totalBranches);
    this.codeElementCoverageQuota = getQuota(this.coveredCodeElements, this.totalCodeElements);
  }

  filter(filter:string,hideFullyCovered:boolean):void{
    this.children.forEach(classesGroup => classesGroup.filter(filter,hideFullyCovered));
  }

  sort(fieldName:keyof ICoverageItemBase, ascending:boolean){
    sortCoverageItems(this.children,fieldName, ascending);
    this.children.forEach(classesGroup => classesGroup.sort(fieldName,ascending));
  }
}

let _columns:any;

const searchBoxStyles: Partial<ISearchBoxStyles> = { root: { width: 200, marginRight:10} };
const sliderStyles : Partial<ISliderStyles> = {root:{width:200}}

interface ColumnSort{
  fieldName:keyof ICoverageItemBase|undefined,
  ascending:boolean
}

export function Coverage(props:CoverageProps) {
  const [sortDetails, setSortDetails] = useState<ColumnSort>({fieldName:undefined,ascending:true})
  const [filter, setFilter] = useState('');
  const [grouping,setGrouping] = useState(0);
  const {summaryResult, namespacedClasses, standalone, active} = props;
  const {assemblies, supportsBranchCoverage} = summaryResult;
  
  const groupingMax = React.useMemo(() => {
    console.log('getting grouping max');
    return getGroupingMax(assemblies);
  },[assemblies]);

  const columns = React.useMemo(() => {
    console.log('getting columns')
    const columns: IColumn[] = [
      nameColumn,
      coveredLinesColumn,
      coverableLinesColumn,
      uncoveredLinesColumn,
      totalLinesColumn,
      lineCoverageQuotaColumn
    ]

    if(supportsBranchCoverage){
      columns.push(coveredBranchesColumn);
      columns.push(totalBranchesColumn);
      columns.push(branchCoverageQuotaColumn);
    }

    if(useProFeature){
      columns.push(coveredCodeElementsColumn);
      columns.push(totalCodeElementsColumn);
      columns.push(codeElementCoverageQuotaColumn);
    }
    for(const col of columns){
      col.isResizable = true
    }
    return columns
  },[supportsBranchCoverage])
  
  
  const groups = React.useMemo(():ICoverageGroup[] => {
    switch(grouping){
      case -1:
        return  [ new AllGroup(assemblies,namespacedClasses, standalone)];
      case 0:
        return assemblies.map(assembly => new AssemblyGroup(assembly,namespacedClasses,standalone));
      default:
        return assemblies.map(assembly => new NamespacedGroup(assembly, namespacedClasses,grouping,standalone));
    }
  },[assemblies, namespacedClasses,grouping])



  
  if(groups.length > 1){
    const rootGroupsSort = sortDetails.fieldName ? sortDetails.fieldName : 'name';
    const rootGroupAscending = sortDetails.fieldName ? sortDetails.ascending : true;
    sortCoverageItems(groups,rootGroupsSort,rootGroupAscending)
  }
  
  nameColumn.isGrouped = grouping > -1;
  
  const items:ICoverageItem[] = [];
  function applyClassesGroup(group:ClassesGroup){
    group.count = group.items.length;
    group.startIndex = items.length;
    items.push(...group.items);
  }
  const workaroundIssueGroups:ICoverageGroup[] = []; // https://github.com/microsoft/fluentui/issues/23169
  groups.forEach(group => {
    group.filter(filter,props.hideFullyCovered);
    if(sortDetails.fieldName){
      group.sort(sortDetails.fieldName, sortDetails.ascending);
    }
    if(group instanceof ClassesGroup && group.items.length > 0){
      applyClassesGroup(group);
      workaroundIssueGroups.push(group);
    }else if(group instanceof NamespacedGroup){
      let groupCount = 0;
      const classesGroupsWithItems:ClassesGroup[] = [];
      group.children.forEach(classesGroup => {
        if(classesGroup.items.length > 0){
          applyClassesGroup(classesGroup);
          groupCount+=classesGroup.count;
          classesGroupsWithItems.push(classesGroup);
        }
      });
      
      if(groupCount > 0){
        const workaroundGroup:ICoverageGroup = {
          ...group,
          children:classesGroupsWithItems,
          count:groupCount,
          filter:() => {},
          sort:() => {}
        }
        workaroundIssueGroups.push(workaroundGroup);
      }
    }
  })

  nameColumn.setFiltered(filter !== '');
  columns.forEach(column => {
    column.isSorted = column.fieldName === sortDetails.fieldName;
    column.isSortedDescending = !sortDetails.ascending;
  })
  
  const groupNestingDepth = grouping > 0 ? 2 : 1;
  
  return <div>
    <Stack horizontal horizontalAlign='space-between' verticalAlign='center'>
    
    <Slider 
      styles={sliderStyles}
      //label='Set grouping level' 
      showValue 
      min={-1} 
      max={groupingMax} 
      value={grouping} 
      onChange={newGrouping => setGrouping(newGrouping)  }
      valueFormat={grouping => {
        switch(grouping){
          case -1:
            return "No grouping";
          case 0:
            return "Assembly";
          default:
            return `By namespace, Level: ${grouping}`;
        }
      }
      }/>
      <SearchBox styles={searchBoxStyles} iconProps={{iconName:'filter'}} value={filter} onChange={(_,newFilter) => setFilter(newFilter!)}/>
    </Stack>
      <DetailsList 
        onShouldVirtualize={() => false} //https://github.com/microsoft/fluentui/issues/21367 https://github.com/microsoft/fluentui/issues/20825
        layoutMode={DetailsListLayoutMode.fixedColumns}
        selectionMode={SelectionMode.none} 
        items={items} 
        groups={workaroundIssueGroups} 
        columns={columns}
        getRowAriaLabel={ (item:ICoverageItem) => `${item.name} coverage`}  
        groupProps={{
          showEmptyGroups:false,
          headerProps:{
            onRenderTitle:(props:IGroupHeaderProps|undefined) => {
              // groupNestingDepth used for aria
              
              const groupLevel = props!.groupLevel === undefined ? 0 : props!.groupLevel;
              const headerGroupNestingDepth = groupNestingDepth- groupLevel - 1;
              const focusZoneProps:IFocusZoneProps = {
                disabled:true
              }
              return <DetailsRow {...props} 
                focusZoneProps={focusZoneProps} 
                groupNestingDepth={headerGroupNestingDepth} 
                item={props!.group} 
                columns={_columns} 
                selectionMode={SelectionMode.none} 
                itemIndex={props!.groupIndex!}
                />
            }
          },
          onRenderHeader: (props:IGroupHeaderProps|undefined) => {
            _columns = (props as any).columns; // ****************************** any cast
            return <GroupHeader {...props}/>
          }
        }}
        onRenderDetailsHeader={
          //todo resolve typing any
          (detailsHeaderProps: IDetailsHeaderProps | undefined, defaultRender: any) => {
            if(active){
              return <Sticky>
              {defaultRender(detailsHeaderProps)}
            </Sticky>
            }

            return defaultRender(detailsHeaderProps)
          }
        }

        onColumnHeaderClick={(_, column) => {
          const coverageColumn:ICoverageColumn = column as ICoverageColumn;
          setSortDetails((current) => {
            if(current.fieldName === coverageColumn!.fieldName){
              return {
                fieldName:coverageColumn!.fieldName,
                ascending: !current.ascending
              }
            }
            return {
              fieldName:coverageColumn?.fieldName,
              ascending:true
            }
          })
          }
        }
        
        />
    </div>
}
