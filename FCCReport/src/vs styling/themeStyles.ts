import { createTheme, getFocusStyle, getInputFocusStyle, HighContrastSelector, IActivityItemStyles, IButtonStyles, ICheckboxStyleProps, ICheckboxStyles, ICustomizations, ICustomizerContext, IDetailsColumnStyleProps, IDetailsColumnStyles, IDetailsHeaderStyleProps, IDetailsHeaderStyles, IDetailsRowStyleProps, IDetailsRowStyles, IDropdownStyleProps, IDropdownStyles, IGroupHeaderStyleProps, IGroupHeaderStyles, ILabelStyleProps, ILabelStyles, ILinkStyleProps, ILinkStyles, IModalStyleProps, IModalStyles, IPivotStyleProps, IPivotStyles, IProgressIndicatorStyleProps, IProgressIndicatorStyles, IRawStyle, isDark, ISearchBoxStyleProps, ISearchBoxStyles, IsFocusVisibleClassName, ISliderStyleProps, ISliderStyles, ITextStyles, styled } from "@fluentui/react";
import { cbGlobalClassNames, dropDownClassNames, sliderClassNames } from "./globalClassNames";
import { getScrollbarStyle } from "./getScrollbarStyle";
import { colorRGBA, getColor, lightenOrDarken } from "./colorHelpers";
import { CategoryColours, Styling } from "../types";
import { DeepPartial } from "@fluentui/merge-styles";
import { vsStyledActionButtonScope } from "./VsStyledActionButton";
import { VsStyledActivityItemScope } from "./VsStyledActivityItem";
import { vsStyledToolWindowTextScope } from "./VsStyledToolWindowText";
import { vsStyledPercentageScope } from "./VsStyledPercentage";
import { vsStyledDetailsListCellTextScope } from "./VsStyledDetailsListCellText";
import { CSSProperties } from "react";
const reactToCSS = require('react-style-object-to-css')

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

const overrideHighContrast = (themeNotHighContrast:boolean,...propertyNames:Array<keyof CSSProperties>) => {
  if(themeNotHighContrast){
    // to type
    const removedProperties :any= {};
    propertyNames.forEach(propertyName => removedProperties[propertyName] = false);

    return {
      selectors:{
        [HighContrastSelector]:removedProperties
      }
    }
  }

  return {};
}

export function getActionButtonStyles(vsColors:CategoryColours,themeNotHighContrast:boolean):DeepPartial<IButtonStyles>{
    const {CommonControlsColors} = vsColors;
    const overrideHighContrastColor = overrideHighContrast(themeNotHighContrast,"color");
    const focusHighContrastStyle:any = themeNotHighContrast ? {
      left: false,
      top: false,
      bottom: false,
      right: false,
      outlineColor: false 
  } : buttonHighContrastFocus;
    const actionButtonStyles:IButtonStyles = {
      root:[
          getFocusStyle(null as any, { inset: 1, highContrastStyle: focusHighContrastStyle, borderColor: 'transparent',outlineColor:CommonControlsColors.FocusVisualText }),
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
          overrideHighContrast(themeNotHighContrast,"borderColor")
      ],
      rootHovered:[{
          color:CommonControlsColors.ButtonHoverText, // affects the text
          backgroundColor:CommonControlsColors.ButtonHover,
          border:`1px solid ${CommonControlsColors.ButtonBorderHover}`
      },overrideHighContrastColor],
      rootDisabled:[{
          color:CommonControlsColors.ButtonDisabledText,
          backgroundColor:CommonControlsColors.ButtonDisabled,
          border:`1px solid ${CommonControlsColors.ButtonBorderDisabled}`
      },overrideHighContrastColor],
      rootPressed:{
          color:CommonControlsColors.ButtonPressedText,
          backgroundColor:CommonControlsColors.ButtonPressed,
          border:`1px solid ${CommonControlsColors.ButtonBorderPressed}`
      },
      
      icon:{
          color:"inherit"
      },
      iconDisabled:overrideHighContrastColor,
      iconHovered:{
          color:"inherit"
      },
      //iconDisabled inherits
      iconPressed:{
          color:"inherit"
      },
      menuIconDisabled:overrideHighContrastColor
      
    }
    return actionButtonStyles
  }

  export function getLinkStyle(props:ILinkStyleProps,vsColors:CategoryColours):DeepPartial<ILinkStyles>{
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
      },
    ]
  }

export const addScrollBarStyles = (vsColors:CategoryColours) => {
  const id = "scrollbarStyles";
  const previousStyle = document.getElementById(id);
  if(previousStyle){
    previousStyle.remove();
  }
  const style = document.createElement("style");
  style.id = id;
  const scrollBarStyle = getVsScrollbarStyle(vsColors);
  for(const [key,value] of Object.entries(scrollBarStyle)){
    style.textContent += `${key}{ ${reactToCSS(value)}}`
  }
  document.head.append(style);
}

export const addVsHighContrastBlocker = (isHighContrastTheme:boolean) => {
  const id = "vsHighContrast";
  const previousStyle = document.getElementById(id);
  if(previousStyle){
    previousStyle.remove();
  }
  if(!isHighContrastTheme){
    const style = document.createElement("style");
    style.id = id;
    style.textContent = `
    @media (forced-colors: active) {
      * {
        forced-color-adjust: none;
      }
    }
    `;
  document.head.append(style);
  }
  
}

export const getVsScrollbarStyle = (vsColors:CategoryColours) => {
  const {EnvironmentColors} = vsColors;

  return getScrollbarStyle(
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
  );
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
  private readonly rowBackgroundFromTreeViewColors = true;
  private readonly rowTextFromTreeViewColors = false;
  private readonly headerColorsForHeaderText = false;
  private readonly surroundTabs = false;
  private readonly maxFontSize = 20;
  private vsColors!:CategoryColours
  constructor();
  constructor(styling:Styling,zoomFactor:number);
  constructor(
    private styling?:Styling,
    private zoomFactor = 1
    ){
      if(styling){
        let fontSize = Number.parseFloat(styling.fontSize);
        let newZoomFactor = 1;
        if(fontSize > this.maxFontSize){
          newZoomFactor = fontSize / this.maxFontSize;
          fontSize = this.maxFontSize;
        }
        this.vsColors = styling.categoryColours;
        
        const consistentFontSize = getFontStyle(styling.fontName,fontSize);
        this.customizations.settings.theme = createTheme({
          fonts:{
            large:consistentFontSize,
            medium:consistentFontSize,
            mediumPlus:consistentFontSize,
            mega:consistentFontSize,
            small:consistentFontSize,
            smallPlus:consistentFontSize
          }
        });

        if(newZoomFactor !== 1){
          (window as any).chrome.webview.hostObjects.webView.setZoomFactor(newZoomFactor);
        }else if(this.zoomFactor !== 1){
          (window as any).chrome.webview.hostObjects.webView.setZoomFactor(1);
        }
        this.zoomFactor = newZoomFactor;

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
      this.zoomFactor
    )
  }

  customizations: ICustomizations = {
    scopedSettings:{
      "Dropdown":{
        styles:(dropDownStyleProps:IDropdownStyleProps):DeepPartial<IDropdownStyles> => {
          const {isRenderingPlaceholder, disabled,} = dropDownStyleProps;
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
        styles:():DeepPartial<IModalStyles> => {
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
        styles:():DeepPartial<ILabelStyles> => {
            return {
              root:{
                color:this.vsColors.EnvironmentColors.ToolWindowText
              }
            }
         
        }
      },
      // necessary as Pivot renders an ActionButton
      [vsStyledActionButtonScope]:{
        styles:():DeepPartial<IButtonStyles> => {
          return getActionButtonStyles(this.vsColors,!this.styling?.themeIsHighContrast);
        }
      },
      "Link":{
        styles:(linkStyleProps:ILinkStyleProps):DeepPartial<ILinkStyles> => {
          return getLinkStyle(linkStyleProps,this.vsColors);
        }
      },
      "Checkbox":{
        styles:(checkboxStyleProps:ICheckboxStyleProps):DeepPartial<ICheckboxStyles> => {
          return vsCbStylesFn(checkboxStyleProps,this.vsColors)
        }
      },
      "ProgressIndicator":{
        styles:(progressIndicatorStyleProps:IProgressIndicatorStyleProps):DeepPartial<IProgressIndicatorStyles> => {
          const {ProgressBarColors : progressBarColors, EnvironmentColors:environmentColors} = this.vsColors;
          const trackColor = progressBarColors.Background;
          const progressBarColor = progressBarColors.IndicatorFill !== trackColor ? progressBarColors.IndicatorFill : environmentColors.ToolWindowText;
          const themeNotHighContrast = !this.styling?.themeIsHighContrast;
          
          return {
              progressTrack:[{
                  backgroundColor : trackColor,
              },themeNotHighContrast && {
                [HighContrastSelector]:{
                  borderBottom:false
                }
              }],
              progressBar:[
                themeNotHighContrast && {
                  [HighContrastSelector]:{
                    backgroundColor:false
                  }
                },
                progressIndicatorStyleProps.indeterminate && {
                    background:
                    `linear-gradient(to right, ${progressBarColor} 0%, ` +
                    `${progressBarColor} 50%, ${progressBarColor} 100%)`,
                }
              ]
          }
        }
      },
      [VsStyledActivityItemScope]:{
        styles:():DeepPartial<IActivityItemStyles> => {
          return {
            root:{
              color:this.vsColors.EnvironmentColors.ToolWindowText
            }
          }
        }
      },
      "Pivot":{
        styles:(pivotStyleProps:IPivotStyleProps):DeepPartial<IPivotStyles>=> {
          const {linkFormat} = pivotStyleProps;
          const {EnvironmentColors:environmentColors, CommonControlsColors} = this.vsColors;
          const themeNotHighContrast = !this.styling?.themeIsHighContrast;

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
                themeNotHighContrast && {
                  selectors:{

                    [HighContrastSelector]:{
                      borderColor:false as any,
                    },
                    ':hover':{
                      [HighContrastSelector]:{
                        color:false as any
                      }
                    }

                  }
                }
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
                },
                themeNotHighContrast && {
                  selectors: {
                    ':before': {
                       selectors:{
                        [HighContrastSelector]:{
                          backgroundColor:false as any
                        }
                       }
                    },
                    [HighContrastSelector]:{
                      color:false as any
                    }
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
              themeNotHighContrast && {
                selectors:{
                  [HighContrastSelector]:{
                    borderColor:false as any,
                  },
                  ':hover':{
                    [HighContrastSelector]:{
                      color:false as any
                    }
                  }

                }
              }
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
                    ':before': {
                      backgroundColor: 'transparent',
                      transition: 'none',
                      position: 'absolute',
                      top: 0,
                      left: 0,
                      right: 0,
                      bottom: 0,
                      content: '""',
                      height: 0,
                    },
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
              },
              themeNotHighContrast && {
                selectors: {
                  ['&.is-selected']: {
                    [HighContrastSelector]:{
                      fontWeight:false,
                      color:false,
                      background:false
                    }
                }
              }
            }
          ],
          }
        }
      },
      "Slider": {
        styles:():DeepPartial<ISliderStyles> => {
          const {CommonControlsColors, EnvironmentColors} = this.vsColors
          const focusStyle = getVsFocusStyle(this.vsColors);

          const toolWindowTextColor = getColor(EnvironmentColors.ToolWindowText);
          const toolWindowTextDark = isDark(toolWindowTextColor);
          const hoverToolWindowTextShade = lightenOrDarken(toolWindowTextColor,0.4,toolWindowTextDark); 
          const hoverToolWindowText =  colorRGBA(hoverToolWindowTextShade);
          const themeNotHighContrast = !this.styling?.themeIsHighContrast;

          
          const overrideHighContrastBackgroundColor = overrideHighContrast(themeNotHighContrast,"backgroundColor");
          const overrideHighContrastBorderColor = overrideHighContrast(themeNotHighContrast,"borderColor");

          return {
            root:{
                width:200
            },
            slideBox: [
                focusStyle,
                {
                  selectors: {
                    [`:active .${sliderClassNames.activeSection}`]: {
                      backgroundColor:hoverToolWindowText,
                      ...overrideHighContrastBackgroundColor
                    },
                    [`:hover .${sliderClassNames.activeSection}`]: {
                      backgroundColor:hoverToolWindowText,
                      ...overrideHighContrastBackgroundColor
                    },
          
                    [`:active .${sliderClassNames.inactiveSection}`]: {
                      backgroundColor:hoverToolWindowText,
                      ...overrideHighContrastBorderColor
                    },
                    [`:hover .${sliderClassNames.inactiveSection}`]: {
                      backgroundColor:hoverToolWindowText,
                      ...overrideHighContrastBorderColor
                    },
          
                    [`:active .${sliderClassNames.thumb}`]: {
                      border: `2px solid ${CommonControlsColors.ButtonBorderPressed}`,
                      ...overrideHighContrastBorderColor
                    },
                    [`:hover .${sliderClassNames.thumb}`]: {
                      border: `2px solid ${CommonControlsColors.ButtonBorderHover}`,
                      ...overrideHighContrastBorderColor
                    },
                  },
                },
              ],
            
            activeSection:{
              background:EnvironmentColors.ToolWindowText, // this is the lhs of the selected value
              ...overrideHighContrastBackgroundColor
            },
            inactiveSection:{
              background:EnvironmentColors.ToolWindowText, // this is the rhs
              ...overrideHighContrast(themeNotHighContrast,"border")
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
        styles:(searchBoxStyleProps:ISearchBoxStyleProps):DeepPartial<ISearchBoxStyles>=> {
          const {theme, underlined, hasFocus} = searchBoxStyleProps;
          const {SearchControlColors, CommonControlsColors} = this.vsColors;
          const themeNotHighContrast = !this.styling?.themeIsHighContrast;

        return { 
          root: [
            { 
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
            themeNotHighContrast && {
              selectors:{
                [HighContrastSelector]:{
                  borderColor:false as any
                },
                ':hover':{
                  selectors:{
                    [HighContrastSelector]:{
                      borderColor:false as any
                    }
                  }
                }
              }
            },
            // todo focused states for other
            hasFocus && [
              getInputFocusStyle(CommonControlsColors.FocusVisualText,underlined ? 0 : theme.effects.roundedCorner2, underlined ? 'borderBottom' : 'border'),
              {
                border: `1px solid ${SearchControlColors.FocusedBorder}`,
                backgroundColor:SearchControlColors.FocusedBackground
              },
              {
                selectors: {
                  ':after': {
                    selectors: {
                      [HighContrastSelector]: {
                        borderColor:false as any,
                        borderBottomColor:false as any
                      },

                  }
                }
              }}
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
              [`.${IsFocusVisibleClassName} && .ms-Button:focus:after`]:[
                {
                outline: `1px solid ${this.vsColors.CommonControlsColors.FocusVisualText}`,
                },
                themeNotHighContrast && {
                  [HighContrastSelector]:{
                    inset:"0px"
                  }
                }
              ],
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
        styles:() => ({
          color:this.vsColors.EnvironmentColors.ToolWindowText,
        })
      },
      [vsStyledToolWindowTextScope]:{
        styles:() : DeepPartial<ITextStyles> => ({
          root:{
            color:this.vsColors.EnvironmentColors.ToolWindowText,
          }
        })
      },
      "SimpleTableRow":{
        styles:():DeepPartial<IDetailsRowStyles>=> {
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
      [vsStyledPercentageScope] : {
        styles:(props:{percentage:number | null}) : DeepPartial<IProgressIndicatorStyles> => {
          const {percentage} = props;
          const {EnvironmentColors} = this.vsColors;
          const backgroundColor = percentage === null ? "transparent" : EnvironmentColors.VizSurfaceGreenMedium;
          //const themeNotHighContrast = !this.styling?.themeIsHighContrast;
          return {
            progressBar: [{
              backgroundColor,
              color: "transparent"
            },/* themeNotHighContrast && {
              [HighContrastSelector]:{
                backgroundColor:false
              }
            } */],
            progressTrack: [{
              backgroundColor: "transparent",
              color: "transparent"
            }/* ,themeNotHighContrast && {
              [HighContrastSelector]:{
                borderBottom:false
              }
            } */],
            root: {
              color: "transparent"
            },
          }
        }
      },
      [vsStyledDetailsListCellTextScope] : {
        styles:():DeepPartial<ITextStyles> => {
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
        styles:(detailsHeaderStyleProps:IDetailsHeaderStyleProps):DeepPartial<IDetailsHeaderStyles> => {
          const {HeaderColors, EnvironmentColors} = this.vsColors;
          const {isSizing} = detailsHeaderStyleProps;
          const themeNotHighContrast = !this.styling?.themeIsHighContrast;
          const focusStyle = getVsFocusStyle(this.vsColors);
          return {
            root:{
              background:HeaderColors.Default,
              borderBottom: `1px solid ${HeaderColors.SeparatorLine}`,
              paddingTop:'0px'
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

            }],
            cellSizer: [
              {
                selectors: {
                  ':after': {
                    content: '""',
                    position: 'absolute',
                    top: 0,
                    bottom: 0,
                    width: 1,
                    background: this.headerColorsForHeaderText ? HeaderColors.MouseOverText : EnvironmentColors.CommandBarDragHandle,
                    opacity: 0,
                    left: '50%',
                  },
                  [`&.is-resizing:after`]: [
                    {
                      boxShadow: `0 0 5px 0 ${EnvironmentColors.CommandBarDragHandleShadow}`,
                    },
                  ],
                },
              },
            ],
            sizingOverlay: isSizing && themeNotHighContrast && overrideHighContrast(themeNotHighContrast,"background")
            
        }
      }
    },
    "DetailsColumn":{
      styles:(detailsColumnStyleProps:IDetailsColumnStyleProps):DeepPartial<IDetailsColumnStyles> => {
        const {isActionable} = detailsColumnStyleProps;
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
      styles: ():DeepPartial<IGroupHeaderStyles> => {
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
        }
      }
     },
     "DetailsRow" : {
      styles:(detailsRowStyleProps:IDetailsRowStyleProps):DeepPartial<IDetailsRowStyles> => {        
        const { TreeViewColors, EnvironmentColors} = this.vsColors;
        const focusStyle = getVsFocusStyle(this.vsColors);
        const rowBackground = this.rowBackgroundFromTreeViewColors ? TreeViewColors.Background : "transparent"
        const rowTextColor = this.rowTextFromTreeViewColors ? TreeViewColors.BackgroundText : EnvironmentColors.CommandBarTextActive;
        const themeNotHighContrast = !this.styling?.themeIsHighContrast;
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
                    ['.ms-DetailsRow-cell > .ms-Link']: {
                      color: EnvironmentColors.PanelHyperlink,
                      textDecoration:"underline",
                      cursor:"pointer"
                    }
                  },

                }
              }
            },
            isSelected && overrideHighContrast(themeNotHighContrast,"background", "color") as any,
            isSelected &&               
              {
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
                '&:hover': [
                  {
                    color: TreeViewColors.SelectedItemInactiveText,
                    background: TreeViewColors.SelectedItemInactive,
                  },
                  themeNotHighContrast && 
                  {
                    selectors: {
                      [HighContrastSelector]: {
                        background: false as any,
                        selectors: {
                          [`.ms-DetailsRow-cell`]: {
                            color: false as any,
                          },
                          [`.ms-DetailsRow-cell > .ms-Link`]: {
                            color: false as any,
                          },
                        },
                      },
                    }
                  }
                ],
        
                // Focus state
                '&:focus': [
                  {
                  color: TreeViewColors.SelectedItemActiveText,
                  background: TreeViewColors.SelectedItemActive,
                  selectors: {
                    [`.ms-DetailsRow-cell`]: [
                      {
                        color: TreeViewColors.SelectedItemActiveText,
                        background: TreeViewColors.SelectedItemActive,
                      }, 
                      themeNotHighContrast && 
                      {
                        selectors:{
                          [HighContrastSelector]: {
                            color: false as any,
                            selectors: {
                              '> a': {
                                color: false as any,
                              },
                            },
                          },
                      }
                    }],
                    ['.ms-DetailsRow-cell button.ms-Link']:{
                      color:TreeViewColors.SelectedItemActiveText
                    },
                  },
                },
                overrideHighContrast(themeNotHighContrast,"background")
              ],
        
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
          cell:[
            {
              selectors:{
                "[data-is-focusable='true']":getFocusStyle(null as any,{ inset: -1, borderColor: "cyan", outlineColor: "pink" })
              }
            },
            isSelected && overrideHighContrast(themeNotHighContrast,"background","color")
          ]
        }
        }
      }
     
    },
    settings:{
      
    },
  }
  
}


