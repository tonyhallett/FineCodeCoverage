import { createTheme, FontClassNames, getFocusStyle, getInputFocusStyle, HighContrastSelector, ICheckboxStyleProps, ICheckboxStyles, ICustomizations, ICustomizerContext, IDetailsColumnStyleProps, IDetailsHeaderStyleProps, IDetailsRowStyleProps, IDropdownStyleProps, IGroupHeaderStyleProps, ILabelStyleProps, ILinkStyleProps, IModalStyleProps, IPivotStyleProps, IProgressIndicatorStyleProps, IProgressIndicatorStyles, IRawStyle, isDark, ISearchBoxStyleProps, IsFocusVisibleClassName, ISliderStyleProps, ZIndexes } from "@fluentui/react";
import { cbGlobalClassNames, dropDownClassNames, sliderClassNames } from "./globalClassNames";
import { getScrollbarStyle } from "./getScrollbarStyle";
import { colorRGBA, getColor, lightenOrDarken } from "./colorHelpers";
import { CategoryColours, Styling } from "../types";

export const buttonHighContrastFocus = {
    left: -2,
    top: -2,
    bottom: -2,
    right: -2,
    outlineColor: 'ButtonText', 
};

function getVsFocusStyle(vsColors:CategoryColours) {
  return getFocusStyle(null as any, {borderColor:"transparent", outlineColor:vsColors.CommonControlsColors.FocusVisualText})
}

export function getActionButtonStyles(vsColors:CategoryColours){
    const {CommonControlsColors} = vsColors;
    const actionButtonStyles = {
      root:[
          getFocusStyle(null as any, { inset: 1, highContrastStyle: buttonHighContrastFocus, borderColor: 'transparent',outlineColor:CommonControlsColors.FocusVisualText }),
          {
              color:CommonControlsColors.ButtonText,
              backgroundColor:CommonControlsColors.Button,
              border:`1px solid ${CommonControlsColors.ButtonBorder}`,
              // todo high contrast
              ":focus":{
                  color:CommonControlsColors.ButtonFocusedText,
                  backgroundColor:CommonControlsColors.ButtonFocused,
                  border:`1px solid ${CommonControlsColors.ButtonBorderFocused}`
              }
          },
      ],
      rootHovered:{
          color:CommonControlsColors.ButtonHoverText, // affects the text
          backgroundColor:CommonControlsColors.ButtonHover,
          border:`1px solid ${CommonControlsColors.ButtonBorderHover}`
      },
      rootDisabled:{
          color:CommonControlsColors.ButtonDisabledText,
          backgroundColor:CommonControlsColors.ButtonDisabled,
          border:`1px solid ${CommonControlsColors.ButtonBorderDisabled}`
      },
      rootPressed:{
          color:CommonControlsColors.ButtonPressedText,
          backgroundColor:CommonControlsColors.ButtonPressed,
          border:`1px solid ${CommonControlsColors.ButtonBorderPressed}`
      },
      
      icon:{
          color:"inherit"
      },
      iconHovered:{
          color:"inherit"
      },
      //iconDisabled inherits
      iconPressed:{
          color:"inherit"
      }
    }
    return actionButtonStyles
  }

  export function getLinkStyle(props:ILinkStyleProps,vsColors:CategoryColours){
    const {isDisabled} = props;
    const {EnvironmentColors: environmentColors, CommonControlsColors} = vsColors;
    const focusColor = CommonControlsColors.FocusVisualText;
  
    return {
      root:[{
        color:environmentColors.PanelHyperlink,
        selectors: {
          '.ms-Fabric--isFocusVisible &:focus': {
            // Can't use getFocusStyle because it doesn't support wrapping links
            // https://github.com/microsoft/fluentui/issues/4883#issuecomment-406743543
            // Using box-shadow and outline allows the focus rect to wrap links that span multiple lines
            // and helps the focus rect avoid getting clipped.
            boxShadow: `0 0 0 1px ${focusColor} inset`,
            outline: `none`,
          },
        },
  
      },
      !isDisabled && {
        '&:active:hover':{
          color:environmentColors.PanelHyperlinkPressed,
        },
        '&:hover':{
          color:environmentColors.PanelHyperlinkHover,
        },
        '&:focus': {
          color: environmentColors.PanelHyperlink,
        },
  
      }
    ]
    }}

        // ignoring indetermintate as vs styling does not provide and not having that state
    // visual studio does not differentiate with checked state
    export const vsCbStylesFn = (props:ICheckboxStyleProps,vsColors:CategoryColours) : ICheckboxStyles => {
      const {CommonControlsColors:commonControlsColors} = vsColors;
      return {
          root:[
              {
                  [`:hover .${cbGlobalClassNames.checkbox}`]: {
                    background: commonControlsColors.CheckBoxBackgroundHover,
                    borderColor: commonControlsColors.CheckBoxBorderHover,
                    [HighContrastSelector]: {
                      borderColor: 'Highlight',
                      background: 'Highlight',
                    },
                  },
                  [`:focus .${cbGlobalClassNames.checkbox}`]: { 
                      background: commonControlsColors.CheckBoxBackgroundFocused,
                      borderColor: commonControlsColors.CheckBoxBorderFocused 
                  },
                  
                  [`:hover .${cbGlobalClassNames.checkmark}`]: { // perhaps should prevent the opacity changing to 1 ?
                    color: commonControlsColors.CheckBoxGlyphHover,
                    [HighContrastSelector]: {
                      color: 'Highlight',
                    },
                  },

                  [`:focus .${cbGlobalClassNames.checkmark}`]: {
                      color: commonControlsColors.CheckBoxGlyphFocused,
                      [HighContrastSelector]: {
                        color: 'Highlight',
                      },
                    },


                  // issue hover and focus together ? Does mine override
                  /* [`:hover .${cbGlobalClassNames.text}, :focus .${cbGlobalClassNames.text}`]: {
                      color: checkboxHoveredTextColor,
                      [HighContrastSelector]: {
                        color: props.disabled ? 'GrayText' : 'WindowText',
                      },
                  } */
                  [`:hover .${cbGlobalClassNames.text}`]: {
                      color: commonControlsColors.CheckBoxTextHover,
                      [HighContrastSelector]: {
                        color: props.disabled ? 'GrayText' : 'WindowText',
                      },
                  },
                  [`:focus .${cbGlobalClassNames.text}`]: {
                      color: commonControlsColors.CheckBoxTextFocused,
                      [HighContrastSelector]: {
                        color: props.disabled ? 'GrayText' : 'WindowText',
                      },
                  },
                },

                
          ],
          checkbox:[
              {
                  border: `1px solid ${commonControlsColors.CheckBoxBorder}`,
                  background:commonControlsColors.CheckBoxBackground
              },
              props.checked && {
                  borderColor: commonControlsColors.CheckBoxBorder, // default styling switches from border to borderColor
              },
              props.disabled && {
                  borderColor : commonControlsColors.CheckBoxBorderDisabled,
                  background: commonControlsColors.CheckBoxBackgroundDisabled
              }
          ],

          checkmark:{
              color:props.disabled? commonControlsColors.CheckBoxGlyphDisabled : commonControlsColors.CheckBoxGlyph
          },
          text:{
              color: props.disabled ? commonControlsColors.CheckBoxTextDisabled : commonControlsColors.CheckBoxText
          },
          input: {
              opacity: 0,
              [`.${IsFocusVisibleClassName} &:focus + label::before`]: {
                  outline: `1px solid ${commonControlsColors.FocusVisualText}`,
              },
            },
      };
  }

  export function getBodyStyles(vsColors:CategoryColours){
    const {EnvironmentColors} = vsColors;
    return [
      {
          background: EnvironmentColors.ToolWindowBackground,
          selectors:getScrollbarStyle(
            EnvironmentColors.ScrollBarThumbBackground,
            EnvironmentColors.ScrollBarThumbMouseOverBackground,
            EnvironmentColors.ScrollBarThumbPressedBackground,
            EnvironmentColors.ScrollBarBackground,
            EnvironmentColors.ScrollBarArrowBackground,
            EnvironmentColors.ScrollBarArrowMouseOverBackground,
            EnvironmentColors.ScrollBarArrowPressedBackground,
            EnvironmentColors.ScrollBarArrowGlyph,
            EnvironmentColors.ScrollBarArrowGlyphMouseOver,
            EnvironmentColors.ScrollBarArrowGlyphPressed,
            EnvironmentColors.ScrollBarBorder,
            EnvironmentColors.ScrollBarThumbBorder,
            EnvironmentColors.ScrollBarThumbMouseOverBorder,
            EnvironmentColors.ScrollBarThumbPressedBorder,
            EnvironmentColors.ScrollBarThumbGlyphMouseOverBorder,
            EnvironmentColors.ScrollBarThumbGlyphPressedBorder
  
          ),
      },
    ]
  }

const getFontStyle = (fontFamily:string,fontSize:number) : IRawStyle => {
  if(fontFamily.indexOf(" ")!== -1){
    fontFamily = `'${fontFamily}'`;
  }
  return {
    fontFamily: fontFamily,
    MozOsxFontSmoothing: 'grayscale',
    WebkitFontSmoothing: 'antialiased',
    fontSize: `${fontSize}px`,
    fontWeight: 400,
  };

}


export class VsCustomizerContext implements ICustomizerContext {
  private rowBackgroundFromTreeViewColors = true;
  private rowTextFromTreeViewColors = false;
  private headerColorsForHeaderText = false;
  private surroundTabs = false;
  private vsColors!:CategoryColours
  constructor(
    private styling:Styling|undefined,
    ){
      if(styling){
        this.vsColors = styling.categoryColours;
      // will have to check if vs quotes 
        const consistentFontSize = getFontStyle(styling.fontName,12);
        this.customizations.settings.theme = createTheme({
          fonts:{
            large:consistentFontSize,
            medium:consistentFontSize,
            mediumPlus:consistentFontSize,
            mega:consistentFontSize,
            small:consistentFontSize,
            smallPlus:consistentFontSize
          }
        })
      }
      
  }

  getNext(
    styling:Styling,
  ){
    if(styling === this.styling){
      return this;
    }
    return new VsCustomizerContext(
      styling, 
    )
  }

  customizations: ICustomizations = {
    scopedSettings:{
      "Dropdown":{
        styles:(dropDownStyleProps:IDropdownStyleProps) => {
          const {isRenderingPlaceholder, disabled, isOpen} = dropDownStyleProps;
          const {EnvironmentColors, CommonControlsColors} = this.vsColors;
          const focusColor = CommonControlsColors.FocusVisualText;

          const getDropDownItemSelectors = (isSelected:boolean) => {
            const dropDownItemSelectors =  {
              selectors: {
                '&:hover:focus': [
                  {
                    color: CommonControlsColors.ComboBoxListItemTextHover,//override
                    backgroundColor: CommonControlsColors.ComboBoxListItemBackgroundHover,//override
                    borderColor: CommonControlsColors.ComboBoxListItemBorderHover
                  },
                ],
                //mine for when hover and not active window
                '&:hover': [
                  {
                    color: CommonControlsColors.ComboBoxListItemTextHover,
                    backgroundColor: CommonControlsColors.ComboBoxListItemBackgroundHover,
                    borderColor: CommonControlsColors.ComboBoxListItemBorderHover
                  },
                ],
                '&:focus': [
                  {
                    color: CommonControlsColors.ComboBoxListItemText,
                    backgroundColor: !isSelected ? 'transparent' : CommonControlsColors.ComboBoxTextInputSelection,//override
                  },
                ],
                // contrary to https://learn.microsoft.com/en-us/visualstudio/extensibility/ux-guidelines/shared-colors-for-visual-studio?view=vs-2022#drop-downs-and-combo-boxes
                // ComboBoxListItemTextPressed ComboBoxListItemTextFocused and more
                // changed to active:hover
                '&:active:hover': [
                  {
                    color: CommonControlsColors.ComboBoxListItemTextHover,//override
                    backgroundColor: isSelected ? CommonControlsColors.ComboBoxListBackground :CommonControlsColors.ComboBoxListItemBackgroundHover,//override
                    borderColor: CommonControlsColors.ComboBoxListItemBorderHover
                  },
                ],
              },
            };
            return dropDownItemSelectors;
        }


        return {
          root:{
            width:"200px"
          },
          label:{
            color:EnvironmentColors.ToolWindowText
          },
          dropdown:{
            color:CommonControlsColors.ComboBoxText,
            borderColor:CommonControlsColors.ComboBoxBorder,
            selectors: {
              // title --------------------------------------------------------
              ['&:hover .' + dropDownClassNames.title]: [
                !disabled && {
                  color:CommonControlsColors.ComboBoxTextHover,//override
                  backgroundColor:CommonControlsColors.ComboBoxBackgroundHover
                },
                { borderColor: CommonControlsColors.ComboBoxBorderHover},
              ],
              ['&:focus .' + dropDownClassNames.title]: [
                !disabled && {
                  color:CommonControlsColors.ComboBoxTextFocused,//override
                  borderColor:CommonControlsColors.ComboBoxBorderFocused,
                  backgroundColor:CommonControlsColors.ComboBoxBackgroundFocused
                },
              ],
              ['&:focus:after']: [
                {
                  border: !disabled ? `2px solid ${focusColor}` : 'none',
                },
              ],
              ['&:active .' + dropDownClassNames.title]: [
                !disabled && {
                  color:CommonControlsColors.ComboBoxTextPressed,
                  backgroundColor:CommonControlsColors.ComboBoxBackgroundPressed,
                },
                { borderColor: CommonControlsColors.ComboBoxBorderPressed},
              ],

              //when rendering placeholder ( additional class applied) ------------------------------------------
              ['&:hover .' + dropDownClassNames.titleIsPlaceHolder]:
              !disabled && {
                color:CommonControlsColors.ComboBoxTextHover,//necessary override
              },
              ['&:focus .' + dropDownClassNames.titleIsPlaceHolder]:
                !disabled &&  {
                  color:CommonControlsColors.ComboBoxTextFocused,//necessary override
                
                },
              ['&:active .' + dropDownClassNames.titleIsPlaceHolder]:
              !disabled && {
                color:CommonControlsColors.ComboBoxTextPressed,//necessary override
              },

              //caretDown ----------------------------------------------------------------------------------------
              ['&:hover .' + dropDownClassNames.caretDown]: !disabled && {color:CommonControlsColors.ComboBoxGlyphHover},
              ['&:focus .' + dropDownClassNames.caretDown]: [
                !disabled && {color:CommonControlsColors.ComboBoxGlyphFocused},
              ],
              ['&:active .' + dropDownClassNames.caretDown]: !disabled && {color:CommonControlsColors.ComboBoxGlyphPressed},

    
            },
          },
          callout:{
            selectors: {
              ['.ms-Callout-main']: { 
                border: `1px solid ${CommonControlsColors.ComboBoxListBorder}`,
                backgroundColor:CommonControlsColors.ComboBoxListBackground,
                // could do this - for the 6 themes testing with, all are black for ComboBoxListBackgroundShadow
                // boxShadow:`0 3.2px 7.2px 0 ${commonControlsColors.ComboBoxListBackgroundShadow}, 0 0.6px 1.8px 0 ${commonControlsColors.ComboBoxListBackgroundShadow}`
              },
            },
          },
          dropdownItem:[
            {
              color:CommonControlsColors.ComboBoxListItemText
            }, 
            getDropDownItemSelectors(false),
            getFocusStyle(null as any, { inset: 1, highContrastStyle: buttonHighContrastFocus, borderColor: 'transparent', outlineColor:focusColor }),
          ],
          dropdownItemSelected:[
              {
              backgroundColor:CommonControlsColors.ComboBoxTextInputSelection,
              color:CommonControlsColors.ComboBoxListItemText
            },
            getDropDownItemSelectors(true),
            getFocusStyle(null as any, { inset: 1, highContrastStyle: buttonHighContrastFocus, borderColor: 'transparent', outlineColor:focusColor })
          ],
          title:[
              {
              backgroundColor:CommonControlsColors.ComboBoxBackground,//override
              borderColor:CommonControlsColors.ComboBoxBorder//override
            }, 
            isRenderingPlaceholder && {color:EnvironmentColors.ControlEditHintText} //override
          ],
          caretDown:{
            color:CommonControlsColors.ComboBoxGlyph,
          }
        }
        }
      },
      "Modal": {
        styles:(props:IModalStyleProps) => {
          const {EnvironmentColors} = this.vsColors;
          return {
            main:[{
              backgroundColor:EnvironmentColors.ToolWindowBackground,
              borderColor:EnvironmentColors.ToolWindowBorder
              },
            ],
          }
        }
      },
      "Label":{
        styles:(props:ILabelStyleProps) => {
            return {
              root:{
                color:this.vsColors.EnvironmentColors.ToolWindowText
              }
            }
         
        }
      },
      // necessary as Pivot renders an ActionButton
      "StyledActionButton":{
        styles:(_:never) => {
          return getActionButtonStyles(this.vsColors);
        }
      },
      "Link":{
        styles:(props:ILinkStyleProps) => {
          return getLinkStyle(props,this.vsColors);
        }
      },
      "Checkbox":{
        styles:(props:ICheckboxStyleProps) => {
          return vsCbStylesFn(props,this.vsColors)
        }
      },
      "ProgressIndicator":{
        styles:(props:IProgressIndicatorStyleProps) => {
          const {ProgressBarColors : progressBarColors, EnvironmentColors:environmentColors} = this.vsColors;
          const trackColor = progressBarColors.Background;//environmentColors.ToolWindowText
          const progressBarColor = progressBarColors.IndicatorFill !== trackColor ? progressBarColors.IndicatorFill : environmentColors.ToolWindowText;
          return {
              progressTrack:{
                  //can barely see this with blue, cannot see with light or dark
                  backgroundColor : trackColor,
                  //border:`1px solid ${environmentColors.ToolWindowText}`
              },
              // might use an accent color
              progressBar:[
              {
              },
              props.indeterminate && {
                  background:
                  `linear-gradient(to right, ${progressBarColor} 0%, ` +
                  `${progressBarColor} 50%, ${progressBarColor} 100%)`,
                  //border:`1px solid ${environmentColors.ToolWindowText}`
              }
              ]
          }
        }
      },
      "VsStyledActivityItem":{
        styles:() => {
          return {
            root:{
              color:this.vsColors.EnvironmentColors.ToolWindowText
            }
          }
        }
      },
      "Pivot":{
        styles:(props:IPivotStyleProps) => {
          const {linkFormat} = props;
          const {EnvironmentColors:environmentColors, CommonControlsColors} = this.vsColors;

          return linkFormat === "links" ? 
          {
            link:[
                {
                    color:environmentColors.ToolWindowText,
                    backgroundColor:environmentColors.ToolWindowBackground,
                    selectors: {
                        [`.${IsFocusVisibleClassName} &:focus`]: {
                          outline: `1px solid ${CommonControlsColors.FocusVisualText}`,
                        },
                        
                        ':hover': {
                            color: environmentColors.ToolWindowText,
                            backgroundColor:environmentColors.ToolWindowBackground
                        },
                        ':active': {
                            color: environmentColors.ToolWindowText,
                            backgroundColor:environmentColors.ToolWindowBackground
                        },
        

                    }

                },
            ],
            linkIsSelected:[
                {
                    selectors: { // This is the underline
                        ':before': {
                            //TreeView.SelectedItemInactive commonControlsColors.InnerTabActiveBorder, 

                            // vs seems to use ToolWindowBackground for environmentColors.ToolWindowTabSelectedTab
                            // alone against the ToolWindowBackground this is not that clear or cannot be seen at all
                            //CommonControlsColors.InnerTabActiveBackground 

                            // legible in all - TreeViewColors.SelectedItemInactiveText
                            backgroundColor: environmentColors.ToolWindowText, // of course this works against ToolWindowBackground
                        },
                    }
                }
            ],
          } : {
            root:[this.surroundTabs && {
              paddingTop:'5px',
              paddingLeft:'5px',
              paddingRight:'5px',
              backgroundColor:environmentColors.EnvironmentBackground,
              display:'inline-block'
            }],
            link:[
              {
                  color:environmentColors.ToolWindowTabText,
                  backgroundColor:environmentColors.ToolWindowTabGradientBegin,
                  border:`1px solid ${environmentColors.ToolWindowTabBorder}`,
                  selectors: {
                      [`.${IsFocusVisibleClassName} &:focus`]: {
                        outline: `1px solid ${CommonControlsColors.FocusVisualText}`,
                      },
                      
                      ':hover': {
                          color: environmentColors.ToolWindowTabMouseOverText,
                          backgroundColor:environmentColors.ToolWindowTabMouseOverBackgroundBegin,
                          border:`1px solid ${environmentColors.ToolWindowTabMouseOverBorder}`,
                      },
                      ':active': {
                        color: environmentColors.ToolWindowTabMouseOverText,
                          backgroundColor:environmentColors.ToolWindowTabMouseOverBackgroundBegin
                      },
      

                  }

              },
          ],
          linkIsSelected:[
              {
                selectors: {
                  [`&.is-selected`]:{
                    color:environmentColors.ToolWindowTabSelectedText,
                    backgroundColor:environmentColors.ToolWindowTabSelectedTab,
                    borderTop:`1px solid ${environmentColors.ToolWindowTabSelectedBorder}`,
                    borderLeft:`1px solid ${environmentColors.ToolWindowTabSelectedBorder}`,
                    borderRight:`1px solid ${environmentColors.ToolWindowTabSelectedBorder}`,
                    borderBottom:`0px solid`,
                    ':hover': {
                        color: environmentColors.ToolWindowTabSelectedText,
                        backgroundColor:environmentColors.ToolWindowTabSelectedTab,
                        borderTop:`1px solid ${environmentColors.ToolWindowTabSelectedBorder}`,
                        borderLeft:`1px solid ${environmentColors.ToolWindowTabSelectedBorder}`,
                        borderRight:`1px solid ${environmentColors.ToolWindowTabSelectedBorder}`,
                        borderBottom:`0px solid`,
                    },
                    ':active': {
                      color: environmentColors.ToolWindowTabSelectedActiveText,
                    },
    
                  }
                }
              }
          ],
          }
        }
      },
      "Slider": {
        styles:(props:ISliderStyleProps) => {
          const {CommonControlsColors, EnvironmentColors} = this.vsColors
          const focusStyle = getVsFocusStyle(this.vsColors);

          const toolWindowTextColor = getColor(EnvironmentColors.ToolWindowText);
          const toolWindowTextDark = isDark(toolWindowTextColor);
          const hoverToolWindowTextShade = lightenOrDarken(toolWindowTextColor,0.4,toolWindowTextDark); 
          const hoverToolWindowText=  colorRGBA(hoverToolWindowTextShade);
          return {
            root:{
                width:200
            },
            slideBox: [
                focusStyle,
                {
                  
                  selectors: {
                    [`:active .${sliderClassNames.activeSection}`]: {
                      backgroundColor:hoverToolWindowText
                    },
                    [`:hover .${sliderClassNames.activeSection}`]: {
                      backgroundColor:hoverToolWindowText
                    },
          
                    [`:active .${sliderClassNames.inactiveSection}`]: {
                      backgroundColor:hoverToolWindowText
                    },
                    [`:hover .${sliderClassNames.inactiveSection}`]: {
                      backgroundColor:hoverToolWindowText
                    },
          
                    [`:active .${sliderClassNames.thumb}`]: {
                      border: `2px solid ${CommonControlsColors.ButtonBorderPressed}`,
                    },
                    [`:hover .${sliderClassNames.thumb}`]: {
                      border: `2px solid ${CommonControlsColors.ButtonBorderPressed}`,
                    },
                  },
                },
              ],
            zeroTick:{
              background:"red"
            },
            activeSection:{
              background:EnvironmentColors.ToolWindowText // this is the lhs of the selected value
            },
            inactiveSection:{
              background:EnvironmentColors.ToolWindowText // this is the rhs
            },
            thumb: [
              {
                borderColor: CommonControlsColors.ButtonBorder,
                background: CommonControlsColors.Button,
              },
            ],
            valueLabel:{
              color:EnvironmentColors.ToolWindowText
            }
          }
        }
      },
      "SearchBox" : {
        styles:(props:ISearchBoxStyleProps) => {
          const {theme, underlined, hasFocus} = props;
          const {SearchControlColors, CommonControlsColors} = this.vsColors;
        return { 
          root: [{ 
            backgroundColor:SearchControlColors.Unfocused,
            border: `1px solid ${SearchControlColors.UnfocusedBorder}`,
            selectors: {
              ':hover': {
                borderColor: SearchControlColors.MouseOverBorder,
                backgroundColor:SearchControlColors.MouseOverBackground
              },
              [`:hover .ms-SearchBox-iconContainer`]: {
                color: SearchControlColors.MouseOverSearchGlyph,
              },
            },
          },
            // todo focused states for other
            hasFocus && [
              getInputFocusStyle(CommonControlsColors.FocusVisualText,underlined ? 0 : theme.effects.roundedCorner2, underlined ? 'borderBottom' : 'border'),
              {
                border: `1px solid ${SearchControlColors.FocusedBorder}`,
                backgroundColor:SearchControlColors.FocusedBackground
              }
            ],
          ],
          // gets background color from root
          field: [{
            color:SearchControlColors.UnfocusedText,
            selectors:{
              "::selection":{
                color:SearchControlColors.SelectionText,
                background:SearchControlColors.Selection
              },
              ":hover":{
                color:SearchControlColors.MouseOverBackgroundText
              }
            }
            
          }, hasFocus && {color:SearchControlColors.FocusedBackgroundText}],
          iconContainer: [{
            color:SearchControlColors.SearchGlyph
          }, 
          // no need for this as search glyph not visible when focus
          /*hasFocus && {color:searchControlColors.FocusedSearchGlyph}*/
          ],
          clearButton:[
            {
              [`.${IsFocusVisibleClassName} && .ms-Button:focus:after`]:{
                outline: `1px solid ${this.vsColors.CommonControlsColors.FocusVisualText}`,
              },
              selectors: {
                '&:hover .ms-Button.ms-Button': {
                  backgroundColor: "transparent",
                },
                '&:hover .ms-Button-icon': {
                  color: SearchControlColors.MouseOverClearGlyph,
                },
                '.ms-Button-icon': {
                  color: SearchControlColors.ClearGlyph,
                },
                
              },
              
            },
            hasFocus && {
              '.ms-Button-icon': {
                color:SearchControlColors.FocusedClearGlyph,
              },
            },
            
          ],
          }
        }
      },
      "VsSpan":{
        styles:(_:never) => ({
          color:this.vsColors.EnvironmentColors.ToolWindowText,
        })
      },
      "ToolWindowText":{
        styles:(_:never) => ({
          root:{
            color:this.vsColors.EnvironmentColors.ToolWindowText,
          }
        })
      },
      "SimpleTableRow":{
        styles:(props:IDetailsRowStyleProps) => {
          const {EnvironmentColors} = this.vsColors;
          const environmentCommandBarTextActive = EnvironmentColors.CommandBarTextActive;
          return {
            root: [{
              background:"none",
              borderBottom:"none",
              color:environmentCommandBarTextActive, // this will not style the header text
              selectors: {
                "&:hover":{
                  background:"none",
                  color:environmentCommandBarTextActive,
                  selectors: {
                    [`.is-row-header`]: {
                      color: environmentCommandBarTextActive,
                    },
                  },
                }
              }
            },
            getFocusStyle(null as any,{borderColor:"none", outlineColor:"none"})
            ],
            isRowHeader:{
              color:environmentCommandBarTextActive 
            }
          }
        }
      },
      "Percentage" : {
        styles:(props:{percentage:number | null}) => {
          const {percentage} = props;
          const {EnvironmentColors} = this.vsColors;
          const backgroundColor = percentage === null ? "transparent" : EnvironmentColors.VizSurfaceGreenMedium;
          return {
            progressBar: {
              backgroundColor,
              color: "transparent"
            },
            progressTrack: {
              backgroundColor: "transparent",
              color: "transparent"
            },
            root: {
              color: "transparent"
            },
          }
        }
      },
      "DetailsListCellText" : {
        styles:(props:any) => {
          return {
            root: [{
              color:'inherit',
            },
            getVsFocusStyle(this.vsColors)
            ]
          }
        }
      },
      "DetailsHeader" : {
        styles:(props:IDetailsHeaderStyleProps) => {
          const {HeaderColors} = this.vsColors;
          const focusStyle = getVsFocusStyle(this.vsColors);
          return {
            root:{
              background:HeaderColors.Default,
              borderBottom: `1px solid ${HeaderColors.SeparatorLine}`,
              paddingTop:'0px'//***************************************** 
              // should this be done ?
              //borderTop: `1px solid ${headerColors.SeparatorLine}`,
              //borderLeft: `1px solid ${headerColors.SeparatorLine}`,
              //borderRight: `1px solid ${headerColors.SeparatorLine}`,
              
            },
            cellIsGroupExpander:[
              focusStyle,
              {
                color:HeaderColors.Glyph,
                selectors: {
                  ':hover': {
                    color: HeaderColors.MouseOverGlyph,
                    backgroundColor: HeaderColors.MouseOver
                  },
                  ':active': {
                    color: HeaderColors.MouseDownGlyph,
                    backgroundColor: HeaderColors.MouseDown
                  },
                },

            }]
        }
      }
    },
    "DetailsColumn":{
      styles:(props:IDetailsColumnStyleProps) => {
        const {isActionable} = props;
        const {HeaderColors, EnvironmentColors} = this.vsColors;
        
      
      return {
        root:[
          {
            color: this.headerColorsForHeaderText ? HeaderColors.DefaultText : EnvironmentColors.CommandBarTextActive, // mirroring vs - alt headerColors.DefaultText,
            background:HeaderColors.Default,
            borderLeft:`1px solid ${HeaderColors.SeparatorLine}`
          },
        isActionable && {
          selectors: {
            ':hover': {
              color: this.headerColorsForHeaderText ? HeaderColors.MouseOverText : EnvironmentColors.CommandBarTextHover,// mirroring vs, alt headerColors.MouseOverText,
              background: HeaderColors.MouseOver,
              selectors:{
                ".ms-Icon":{
                  color:HeaderColors.MouseOverGlyph
                }
              }
            },
            ':active': {
              color: this.headerColorsForHeaderText ? HeaderColors.MouseDownText : EnvironmentColors.CommandBarTextSelected, //mirroring vs, alt headerColors.MouseDownText,
              background: HeaderColors.MouseDown,
              selectors:{
                ".ms-Icon":{
                  color:HeaderColors.MouseDownGlyph
                }
              }
            },
          },
        },
      ],
      cellTitle:[
        getVsFocusStyle(this.vsColors)
      ],  
      nearIcon:{
        color:HeaderColors.Glyph,
      },
      sortIcon:{
        color:HeaderColors.Glyph
      },
      gripperBarVerticalStyle:{
        color:HeaderColors.SeparatorLine
      }
     }
      }
    },
     "GroupHeader" : {
      styles: (props:IGroupHeaderStyleProps) => {
        const {HeaderColors, TreeViewColors, EnvironmentColors} = this.vsColors;
        const rowBackground = this.rowBackgroundFromTreeViewColors ? TreeViewColors.Background : "transparent"
        const rowTextColor = this.rowTextFromTreeViewColors ? TreeViewColors.BackgroundText : EnvironmentColors.CommandBarTextActive;
        const focusStyle = getVsFocusStyle(this.vsColors);
        return {
          root:[{
            borderBottom: `1px solid ${HeaderColors.SeparatorLine}`,
            userSelect:'text',
            background: rowBackground,// treeViewColors.Background,
            color: rowTextColor,//environmentColors.CommandBarTextActive, // *** mirroring vs
            selectors: {
              ':hover': {
                background: rowBackground,// treeViewColors.Background,
                color: rowTextColor,// environmentColors.CommandBarTextActive // *** mirroring vs
              },
            },
            
          },
          
          
          focusStyle
          ],
          expand:[{  
            color:HeaderColors.Glyph,
            selectors: { // ignoring selected state
              ':hover': {
                color: HeaderColors.MouseOverGlyph,
                backgroundColor: HeaderColors.MouseOver
              },
              ':active': {
                color: HeaderColors.MouseDownGlyph,
                backgroundColor: HeaderColors.MouseDown
              },
            },
            
            },focusStyle
          ],
          cellSizer: [
            {
              selectors: {
                ':after': {
                  content: '""',
                  position: 'absolute',
                  top: 0,
                  bottom: 0,
                  width: 1,
                  background: HeaderColors.MouseOver,
                  opacity: 0,
                  left: '50%',
                },
                // could change the boxShadow
                /* [`&.${classNames.isResizing}:after`]: [
                  cellSizerFadeInStyles,
                  {
                    boxShadow: '0 0 5px 0 rgba(0, 0, 0, 0.4)',
                  },
                ], */
              },
            },
          ],
        }
      }
     },
     "DetailsRow" : {
      styles:(detailsRowStyleProps:IDetailsRowStyleProps) => {        
        const { TreeViewColors, EnvironmentColors} = this.vsColors;
        const focusStyle = getVsFocusStyle(this.vsColors);
        const rowBackground = this.rowBackgroundFromTreeViewColors ? TreeViewColors.Background : "transparent"
        const rowTextColor = this.rowTextFromTreeViewColors ? TreeViewColors.BackgroundText : EnvironmentColors.CommandBarTextActive;
        const {isSelected} = detailsRowStyleProps;
        return {
          root: [
            {
              background: rowBackground,//  treeViewColors.Background, // mirroring vs, docs say "transparent",
              borderBottom:"none",
              color: rowTextColor,// environmentColors.CommandBarTextActive,
              selectors: {
                "&:hover":{
                  background:rowBackground,//treeViewColors.Background, // mirroring vs, docs say "transparent",
                  color:rowTextColor,// environmentColors.CommandBarTextActive,
                  selectors: {
                    [`.ms-DetailsRow-cell > .ms-Link`]: {
                      color: EnvironmentColors.PanelHyperlink,
                      textDecoration:"underline",
                      cursor:"pointer"
                    }
                  },

                }
              }
            },
          
            isSelected && {
              color:TreeViewColors.SelectedItemInactiveText,
              background: TreeViewColors.SelectedItemInactive,
              borderBottom: "none",
              selectors: {
                ['.ms-DetailsRow-cell button.ms-Link']:{
                  color:TreeViewColors.SelectedItemInactiveText
                },
                '&:active': {
                  ['.ms-DetailsRow-cell button.ms-Link']:{
                    color:TreeViewColors.SelectedItemActiveText
                  },
                },
        

                '&:before': {
                  borderTop: "none",
                },
        
                // Selected State hover
                '&:hover': {
                  color: TreeViewColors.SelectedItemInactiveText,
                  background: TreeViewColors.SelectedItemInactive,
                },
        
                // Focus state
                '&:focus': {
                  color: TreeViewColors.SelectedItemActiveText,
                  background: TreeViewColors.SelectedItemActive,
                  selectors: {
                    [`.ms-DetailsRow-cell`]: {
                      color: TreeViewColors.SelectedItemActiveText,
                      background: TreeViewColors.SelectedItemActive,
                    },
                    ['.ms-DetailsRow-cell button.ms-Link']:{
                      color:TreeViewColors.SelectedItemActiveText
                    },
                  },
                },
        
                // Focus and hover state
                '&:focus:hover': {
                  color: TreeViewColors.SelectedItemActiveText,
                  background: TreeViewColors.SelectedItemActive,
                  selectors: {
                    [`.ms-DetailsRow-cell`]: {
                      color: TreeViewColors.SelectedItemActiveText,
                      background: TreeViewColors.SelectedItemActive,
                    },
                  },
                },
                '&:focus-within':{
                  color: TreeViewColors.SelectedItemActiveText,
                  background: TreeViewColors.SelectedItemActive,
                }
                
              },
            },
            focusStyle,
          ],
          // todo requires override ?
          /* cell:{
            selectors:{
              "[data-is-focusable='true']":
            }
          } */
        }
        }
      }
     
    },
    settings:{
     
    },
  }
  
}


