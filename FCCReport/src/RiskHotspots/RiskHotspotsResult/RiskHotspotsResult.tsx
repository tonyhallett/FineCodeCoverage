import { 
  DetailsList, 
  SearchBox, 
  IDropdownOption, 
  Dropdown, 
  SelectionMode, 
  DetailsListLayoutMode, 
  Stack, 
  CheckboxVisibility, 
  IDetailsListProps,
  ISearchBoxProps,
  IDropdownProps,
  IStackProps
} from '@fluentui/react';
import React, { useState } from 'react';
import { Assembly, ReportOptions, RiskHotspot, RiskHotspotsAnalysisThresholds } from '../../types';
import { useRenderDetailsHeaderSticky } from '../../utilities/hooks/useRenderDetailsHeaderSticky';
import { getColumns, ISortableColumn } from './columns';
import { allAssembliesOption, filterItems } from './filtering';
import { parseRiskHotspots } from './parseRiskHotspots';
import { ColumnSort, sort } from './sorting';

export type RiskHotspotsResultProps = {
  riskHotspots:RiskHotspot[],
  assemblies:Assembly[],
  riskHotspotsAnalysisThresholds: RiskHotspotsAnalysisThresholds,
  standalone:boolean,
  active:boolean
} & Pick<ReportOptions,"namespacedClasses"|"stickyCoverageTable">

const searchBoxStyles : ISearchBoxProps["styles"] = {
  root:{
    width:'200px'
  }
}

const searchBoxIconProps : ISearchBoxProps["iconProps"] = {iconName:'filter'}

const assemblyFilterDropDownStyles :IDropdownProps["styles"] = {
  root:{
    width:"200px"
  }
}

const stackStyles : IStackProps["styles"] = {
  root:{
    margin:'10px 10px'
  }
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

  const onRenderDetailsHeader = useRenderDetailsHeaderSticky(active, stickyCoverageTable);
  
  const searchBoxOnChange : ISearchBoxProps["onChange"] = React.useCallback((_,newValue) => setClassDisplayFilter(newValue),[]);

  const assemblyFilterDropDownOnChange : IDropdownProps["onChange"] = React.useCallback((_,option) => setSelectedAssemblyFilterOption(option!),[]);

  const detailsListOnRenderRow : IDetailsListProps["onRenderRow"] = React.useCallback(
    (rowProps, defaultRender) => {
      rowProps!.styles = {
        fields:{
          alignItems:"center"
        },
      }
      return defaultRender!(rowProps);
    },[]
  )

  const detailsListOnColumnHeaderClick : IDetailsListProps["onColumnHeaderClick"] = React.useCallback(
    (_, column) => {
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
    
    },[]
  )

  const [items, metricNames,assemblyFilterDropDownOptions] = React.useMemo(() => {
    return parseRiskHotspots(riskHotspots,assemblies,namespacedClasses,standalone);
  },[riskHotspots, assemblies, namespacedClasses, standalone])

  const columns = React.useMemo(() => {
    return getColumns(metricNames,riskHotspotsAnalysisThresholds);
  },[metricNames,riskHotspotsAnalysisThresholds])
  

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
    <Stack styles={stackStyles} horizontal horizontalAlign='space-between' verticalAlign='center'>
        <Dropdown
          placeholder='All assemblies' 
          options={assemblyFilterDropDownOptions} 
          onChange={assemblyFilterDropDownOnChange} 
          selectedKey={selectedAssemblyFilterOption?.key}
          styles={assemblyFilterDropDownStyles}
          />
        <SearchBox styles={searchBoxStyles} 
          iconProps={searchBoxIconProps} 
          placeholder='Filter by class' 
          value={classDisplayFilter} 
          onChange={searchBoxOnChange}/>
    </Stack>
    <DetailsList 
      selectionMode={SelectionMode.single}
      checkboxVisibility={CheckboxVisibility.hidden} 
      items={filteredAndSortedItems} 
      columns={columns} 
      layoutMode={DetailsListLayoutMode.fixedColumns}
      onRenderDetailsHeader={onRenderDetailsHeader}
      onRenderRow={detailsListOnRenderRow}
      onColumnHeaderClick={detailsListOnColumnHeaderClick}/>
    </div>
}


