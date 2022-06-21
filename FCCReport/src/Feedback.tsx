
import { CommandBar, ICommandBarItemProps } from '@fluentui/react';
import React from 'react';
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
    return <CommandBar items={items} />
  }
  
  export const Feedback = React.memo(FeedbackBase)