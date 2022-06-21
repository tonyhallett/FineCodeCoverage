import { CheckboxVisibility, DetailsHeader, DetailsList, DetailsRow, GroupHeader, IColumn, IDetailsHeaderProps, IDetailsList, IDetailsListProps, IDetailsRowProps, IFocusZoneProps, IGroup, IGroupDividerProps, IGroupHeaderProps, IRenderFunction, SelectionMode } from '@fluentui/react';
import React, {  } from 'react';

interface MyItem{
    first:string,
    second:string
  }
  
  interface MyGroup extends Omit<IGroup, 'startIndex'|'count'|'children'>, MyItem {
    startIndex?:number,
    count?:number,
    children?:MyGroup[]
  }



export default class DetailsListCustomGroupHeadersExample extends React.Component<{}, {}> {
    private _items: MyItem[];
    private _groups: MyGroup[];
    private _columns:IColumn[]|undefined;
    private _groupNestingLevel!:number
    constructor(props: {}) {
      super(props);
      this._items = [
        {first:"1First",second:"1Second"},
        {first:"2First",second:"2Second"},
        {first:"3First",second:"3Second"},
        {first:"4First",second:"4Second"},
        {first:'5First', second:'5Second'}
      ]
      const myGroups:MyGroup[] = [
        {
          first:"G2First",
          second:"G2Second",
          key:'g2',
          startIndex:2,
          count:2,
          level:0,
          name:'g2'
        },
        {
          first:"GFirst",
          second:"GSecond",
          key:'g1',
          name:'g1',
          level:0,
          count:2,
          children:[
            {
              first:"GChildFirst",
              second:"GChildSecond",
              key:'gGhild',
              name:'gGChild',
              level:1, 
              //startIndex:1, - seems that this is irrelevant
              count:1, // this is necessary but with normal rendering just that it is more than 0 ! normal header rendering renders this count
              children:[
                {
                  first:"GGCFirst",
                  second:"GGCSecond",
                  key:'gGC',
                  startIndex:1,
                  count:1,
                  level:2,
                  name:'gGC'
                },
                {
                  first:"GGC2First",
                  second:"GGC2Second",
                  key:'gGC2',
                  startIndex:1,
                  count:1,
                  level:2,
                  name:'gGC2'
                }
              ]
            },
            {
              first:"GChildEndFirst",
              second:"GChildEndSecond",
              key:'gChildEnd',
              name:'gChildEnd',
              level:1,
              startIndex:0,
              count:1
            },
          ]
        },
      ]
      this._groups = myGroups;
      this._groupNestingLevel = this._getGroupsNestinglevel(myGroups);
    }

    private _getGroupsNestinglevel(groups:MyGroup[]):number{
      let level = 0;
      for(let group of groups){
        const groupLevel = this._getGroupNestingLevel(1,group);
        if(groupLevel > level){
          level = groupLevel;
        }
      }
      return level;
    }

    private _getGroupNestingLevel(start:number,group:MyGroup):number{
      if(group.children && group.children.length > 0){
        const childLevels = group.children.map(g => this._getGroupNestingLevel(start + 1, g));
        return Math.max(...childLevels);
      }
      return start;
    }
  
    private _onRenderTitle = (props:IGroupHeaderProps|undefined) => {
      // groupNestingDepth used for aria
      const groupLevel = props!.groupLevel === undefined ? 0 : props!.groupLevel;
      const groupNestingDepth = this._groupNestingLevel - groupLevel - 1;
      const focusZoneProps:IFocusZoneProps = {
        disabled:true
      }
      return <DetailsRow {...props} focusZoneProps={focusZoneProps} groupNestingDepth={groupNestingDepth} item={props!.group} columns={this._columns} selectionMode={SelectionMode.none} itemIndex={props!.groupIndex!}/>
    }
      
    private _onRenderGroupHeader = (props:IGroupHeaderProps|undefined) => {
      this._columns = (props as any).columns; // ****************************** any cast
      return <GroupHeader {...props}/>
    }
      
    private _onRenderRow:any = (props:IDetailsRowProps,defaultRender:(props:IDetailsRowProps)=>JSX.Element | null) => {
      props.groupNestingDepth= this._groupNestingLevel;
      return defaultRender(props);
    }
    
    private _onRenderDetailsHeader:any = (props:IDetailsHeaderProps, defaultRender:(props:IDetailsHeaderProps) => JSX.Element |null) => {
      props.groupNestingDepth = this._groupNestingLevel;
      return defaultRender(props);
    }

    private correctScrolling = (detailsList:IDetailsList|null) => {
      if(detailsList){
        (detailsList as any)._getGroupNestingDepth = () => {
          return this._groupNestingLevel;
        }
        detailsList.forceUpdate();
      }
    }

    public render(): JSX.Element {
      return (
        <>
          <DetailsList
            componentRef={
              detailsList => this.correctScrolling(detailsList)
            }
            items={this._items}
            groups={this._groups as any}
            onRenderDetailsHeader={this._onRenderDetailsHeader}
            onRenderRow={this._onRenderRow}
            
            groupProps={{
              headerProps:{
                onRenderTitle:this._onRenderTitle,
              },
              onRenderHeader: this._onRenderGroupHeader,
            }}

            ariaLabelForSelectionColumn="Toggle selection"
            ariaLabelForSelectAllCheckbox="Toggle selection for all items"
            checkButtonAriaLabel="select row"
            checkboxVisibility={CheckboxVisibility.hidden}
          />
        </>
      );
    }

  }