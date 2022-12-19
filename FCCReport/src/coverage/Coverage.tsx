import React, { useRef, useState } from 'react';
import { CheckboxVisibility, DetailsList, DetailsListLayoutMode, DetailsRow, IColumn, IDetailsGroupDividerProps, IDetailsRowProps, IFocusZoneProps, IGroup, IGroupHeaderProps, SearchBox, SelectionMode, Slider, Stack } from '@fluentui/react';
import { getGroupingMax } from './getGroupingMax';
import { sortAndFilterColumns as sortFilterGroupColumns } from './sortAndFilterColumns';
import { ICoverageItem } from './ICoverageItem';
import { ICoverageGroup } from './Groups/ICoverageGroup';
import { ClassesGroup } from './Groups/ClassesGroup';
import { AssemblyGroup } from './Groups/AssemblyGroup';
import { AllGroup } from './Groups/AllGroup';
import { NamespacedGroup } from './Groups/NamespacedGroup';
import { CoverageProps } from './CoverageProps';
import { sortCoverageItems } from './sortCoverageItems';
import { ColumnSort } from './ColumnSort';
import { ICoverageColumn } from './Columns/ICoverageColumn';
import { GroupsItemsSelection } from '../utilities/GroupsItemsSelection';
import { useConst } from '@fluentui/react-hooks';
import { focusingCells } from './common';
import { useRenderDetailsHeaderSticky } from '../utilities/hooks/useRenderDetailsHeaderSticky';
import { getColumns } from './Columns/columns';

const groupHeaderRowClassName = "groupHeaderRow";
const dataIsFocusable = {
  "data-is-focusable":true,
}


export function Coverage(props:CoverageProps) {
  const [columnSort, setColumnSort] = useState<ColumnSort>({fieldName:undefined,ascending:true})
  const [filter, setFilter] = useState<string>();
  const [grouping,setGrouping] = useState(0);
  const adjustedColumns = useRef<IColumn[]>()
  const selection = useConst(() => new GroupsItemsSelection());
  const {summaryResult, namespacedClasses, standalone, active, stickyCoverageTable, hideFullyCovered} = props;
  const {assemblies, supportsBranchCoverage} = summaryResult;
  
  const onRenderDetailsHeader = useRenderDetailsHeaderSticky(active, stickyCoverageTable);

  const groupingMax = React.useMemo(() => {
    return getGroupingMax(assemblies);
  },[assemblies]);

  const columns = React.useMemo(() => {
    return getColumns(supportsBranchCoverage);
  },[supportsBranchCoverage])

  sortFilterGroupColumns(columns,filter ?? '',columnSort,grouping);
  
  
  const groups = React.useMemo(():ICoverageGroup[] => {
    switch(grouping){
      case -1:
        return  [ new AllGroup(assemblies,namespacedClasses, standalone)];
      case 0:
        return assemblies.map(assembly => new AssemblyGroup(assembly,namespacedClasses,standalone));
      default:
        return assemblies.map(assembly => new NamespacedGroup(assembly, namespacedClasses,grouping,standalone));
    }
  },[assemblies, namespacedClasses,grouping, standalone])

  React.useMemo(() => {
    if(groups.length > 1){
      const rootGroupsSort = columnSort.fieldName ? columnSort.fieldName : 'name';
      const rootGroupAscending = columnSort.fieldName ? columnSort.ascending : true;
      sortCoverageItems(groups,rootGroupsSort,rootGroupAscending)
    }
  },[groups,columnSort])

  const [items, workaroundIssueGroups] = React.useMemo(() => {
    const items:ICoverageItem[] = [];
    const workaroundIssueGroups:ICoverageGroup[] = []; // https://github.com/microsoft/fluentui/issues/23169

    function takeItems(group:ClassesGroup){
      group.count = group.items.length;
      group.startIndex = items.length;
      items.push(...group.items);
    }

    groups.forEach(group => {
      group.filter(filter ?? '',hideFullyCovered);
  
      if(columnSort.fieldName){
        group.sort(columnSort.fieldName, columnSort.ascending);
      }
      
      if(group instanceof ClassesGroup && group.items.length > 0){
        takeItems(group);
        workaroundIssueGroups.push(group);
      }else if(group instanceof NamespacedGroup){
        let groupCount = 0;
        const classesGroupsWithItems:ClassesGroup[] = [];
        group.children.forEach(classesGroup => {
          if(classesGroup.items.length > 0){
            takeItems(classesGroup);
            groupCount+=classesGroup.count;
            classesGroupsWithItems.push(classesGroup);
          }
        });
        
        if(groupCount > 0){
          const workaroundGroup:ICoverageGroup = {
            ...group,
            children:classesGroupsWithItems,
            count:groupCount,
            filter:() => {
              //ok
            }, 
            sort:() => {
              //ok
            }
          }
          workaroundIssueGroups.push(workaroundGroup);
        }
      }
    });

    selection.initialize(workaroundIssueGroups, items);
    return [items,workaroundIssueGroups]
  },[groups,filter,hideFullyCovered,columnSort, selection]);
  
  const groupNestingDepth = grouping > 0 ? 2 : 1;
  
  return <>
    <Stack horizontal horizontalAlign='space-between' verticalAlign='center'>
      <Slider 
        styles={{root:{width:200}}}
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
      <SearchBox styles={{ root: { width: 200, marginRight:10} }} 
        iconProps={{iconName:'filter'}} 
        value={filter} 
        onChange={(_,newFilter) => setFilter(newFilter)}/>
    </Stack>
      <DetailsList 
        styles={
          {
            root:{
              marginTop:'10px'
            }
          }
        }
        onShouldVirtualize={() => false} //https://github.com/microsoft/fluentui/issues/21367 https://github.com/microsoft/fluentui/issues/20825
        layoutMode={DetailsListLayoutMode.fixedColumns} // justified always flashes unless have set all the column sizes
        selectionMode={SelectionMode.single} // due to defaultProps ! does not take from selection !
        selection={selection}
        checkboxVisibility={CheckboxVisibility.hidden}
        items={items} 
        groups={workaroundIssueGroups} 
        columns={columns}
        getRowAriaLabel={ (item:ICoverageItem) => `${item.name} coverage`}  
        groupProps={{
          showEmptyGroups:false,
          headerProps:{
            onRenderTitle:(props) => {
              const definedProps = props as IGroupHeaderProps;
              const group = definedProps.group as IGroup;

              const expandButtonProps:IGroupHeaderProps["expandButtonProps"] = {
                'aria-label': 'expand collapse group',
              }
              
              const unsafeExpandButtonProps = {
                ...expandButtonProps,                
                ...dataIsFocusable
              }
              // eslint-disable-next-line @typescript-eslint/no-explicit-any, @typescript-eslint/no-unsafe-assignment
              definedProps.expandButtonProps = unsafeExpandButtonProps as any;
              
              // groupNestingDepth used for aria
              const groupLevel = definedProps.groupLevel === undefined ? 0 : definedProps.groupLevel;

              const headerGroupNestingDepth = groupNestingDepth- groupLevel - 1;
              const notFocusingCellsFocusZoneProps:IFocusZoneProps = {
                disabled:true
              }
              const focusZoneProps = focusingCells ? dataIsFocusable: notFocusingCellsFocusZoneProps;

              const groupIndex = selection.getGroupIndex(group);

              return <DetailsRow 
                styles = {{
                  fields:{
                    alignItems:"center"
                  },
                }}
                className={groupHeaderRowClassName}
                selection={selection}
                selectionMode={SelectionMode.single} 
                checkboxVisibility={CheckboxVisibility.hidden}
                // eslint-disable-next-line @typescript-eslint/no-explicit-any, @typescript-eslint/no-unsafe-assignment
                focusZoneProps={focusZoneProps as any} 
                groupNestingDepth={headerGroupNestingDepth} 
                item={group} 
                columns={adjustedColumns.current} 
                itemIndex={groupIndex}
                />
            }
          },
          onRenderHeader: (props,defaultRender) => {
            const definedProps = props as IDetailsGroupDividerProps;
            const definedDefaultRender = defaultRender as NonNullable<typeof defaultRender>;
            adjustedColumns.current = definedProps.columns;
            if(focusingCells){
              definedProps.onGroupHeaderKeyUp = ev => {
                const leftOrRightArrow = ev.code === 'ArrowRight' || ev.code === 'ArrowLeft';
                if(leftOrRightArrow){
                  const groupHeaderRow = (ev.target as Element).closest(`.${groupHeaderRowClassName}`);
                  if(groupHeaderRow){
                    ev.preventDefault()
                  }
                }
                
  
              }
              
              
            }
            return definedDefaultRender(definedProps);
          }
        }}
        onRenderRow={(rowProps, defaultRender) => {
          const definedProps = rowProps as IDetailsRowProps;
          const definedDefaultRender = defaultRender as NonNullable<typeof defaultRender>;
          //definedProps!.groupNestingDepth = // todo calculate
          definedProps.styles = {
            fields:{
              alignItems:"center"
            },
          }
          return definedDefaultRender(rowProps);
        }}
        onRenderDetailsHeader={onRenderDetailsHeader}

        onColumnHeaderClick={(_, column) => {
          const coverageColumn = column as ICoverageColumn;
          setColumnSort((current) => {
            if(current.fieldName === coverageColumn.fieldName){
              return {
                fieldName:coverageColumn.fieldName,
                ascending: !current.ascending
              }
            }
            return {
              fieldName:coverageColumn.fieldName,
              ascending:true
            }
          })
          }
        }
        
        />
    </>
}

