
import { ICommandBarItemProps } from '@fluentui/react';
import React from 'react';
import { StyledActionButton } from './vs styling/StyledActionButton';
// memo / items outside
export function FeedbackBase(){
    const items:ICommandBarItemProps[] = [
      {
        key:'buyBeer',
        text:'Buy beer',
        iconProps:{iconName:'beerMug'},
        ariaLabel:'Buy me a beer',
        onClick(){
          (window as any).chrome.webview.hostObjects.fccResourcesNavigator.buyMeACoffee();
        }
      },
      {
        key:'logIssueOrSuggestion',
        text:'Issue / feature',
        iconProps:{iconName:'github'},
        ariaLabel:'Log issue or suggestion',
        onClick(){
          (window as any).chrome.webview.hostObjects.fccResourcesNavigator.logIssueOrSuggestion();
        }
      },
      {
        key:'rateAndReview',
        text:'Rate and review',
        iconProps:{iconName:'review'},
        ariaLabel:'Review',
        onClick(){
          (window as any).chrome.webview.hostObjects.fccResourcesNavigator.rateAndReview();
        }
      }
      
    ]
    
    return  <>
    {items.map(props => <StyledActionButton key={props.key} style={{marginRight:'5px'}} onClick={props.onClick as any} iconProps={props.iconProps}>{props.text} </StyledActionButton>)}
  </>
  }
  
  export const Feedback = React.memo(FeedbackBase)