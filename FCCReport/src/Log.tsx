import { ActionButton, ActivityItem, IActivityItemProps, Icon, IconButton, IStyle } from '@fluentui/react';
import React, { CSSProperties } from 'react';
import getFontFamily from './helpers/fontName';
import { Emphasis, LogMessage, MessageContext, Styling } from './types';
import { StyledActionButton } from './vs styling/StyledActionButton';
import { VsStyledActivityItem } from './vs styling/StyledActivityItem';
import { ToolWindowText } from './vs styling/ToolWindowText';

// use a map
function getIconNameForContext(messageContext:MessageContext){
  switch(messageContext){
    case MessageContext.Info:
      return "info";
    case MessageContext.Warning:
      return "warning";
    case MessageContext.Error:
      return "error";
    case MessageContext.CoverageStart:
      return "processing";
    case MessageContext.CoverageCancelled:
      return "processingCancelled";
    case MessageContext.TaskCompleted:
    case MessageContext.ReportGeneratorCompleted:
    case MessageContext.CoverageCompleted:
    case MessageContext.CoverageToolCompleted:
      return "completed";
    case MessageContext.ReportGeneratorStart:
      return "table";
    case MessageContext.CoverageToolStart:
      return "tool"
  }
}

function getIconNameForHostObjectMethod(hostObject:string,method:string){
  if(hostObject === 'fccOutputPane'){
    return 'openPane';
  }
  return 'navigate';
}

// todo is this necessary ?
function getActivityIconAriaLabelFromContext(messageContext:MessageContext){
  switch(messageContext){
    case MessageContext.CoverageCancelled:
      return "Coverage Cancelled";
    case MessageContext.CoverageCompleted:
      return "Coverage Completed";
    case MessageContext.CoverageStart:
      return "Coverage Start";
    case MessageContext.CoverageToolCompleted:
      return "Coverage Tool Completed";
    case MessageContext.CoverageToolStart:
      return "Coverage Tool Start";
    case MessageContext.Error:
      return "Error";
    case MessageContext.Info:
      return "Info";
    case MessageContext.ReportGeneratorCompleted:
      return "Report Completed";
    case MessageContext.ReportGeneratorStart:
      return "Report Start";
    case MessageContext.TaskCompleted:
      return "Task Completed";
    case MessageContext.TaskStarted:
      return "Task Started";
    case MessageContext.Warning:
      return "Warning";
  }
}

export function Log(props:{logMessages:LogMessage[], clearLogMessages:() => void}) {
  const {logMessages, clearLogMessages} = props;
  const activityItemsOrBreaks:any[] = [];
  logMessages.forEach((logMessage,i) => {
    
    const activityDescription:React.ReactNode[] =
    logMessage.message.map((msgPart,j) => {
      if(msgPart.type === 'emphasized' ){
        const root:IStyle={
        }
        if(msgPart.emphasis & Emphasis.Bold){
          root.fontWeight='bold';
        }
        if(msgPart.emphasis & Emphasis.Italic){
          root.fontStyle='italic';
        }
        if(msgPart.emphasis & Emphasis.Underline){
          root.textDecoration = 'underline';
        }
        return <ToolWindowText key={j} styles={
          {root}
        }>{msgPart.message}</ToolWindowText>;
      }else{
        const actionButton = <StyledActionButton 
        key={j} 
        style={{marginLeft:'10px'}}
        ariaLabel={msgPart.ariaLabel}
        iconProps={{iconName:getIconNameForHostObjectMethod(msgPart.hostObject,msgPart.methodName)}} 
        onClick={() => {
          const hostObject = (window as any).chrome.webview.hostObjects[msgPart.hostObject];
          const hostMethod:Function = hostObject[msgPart.methodName];
          hostMethod.apply(null,msgPart.arguments);
        }}>{msgPart.title}</StyledActionButton>
        return actionButton;
      }
    })

    let activityItemProps:Partial<IActivityItemProps> = {
      activityDescription,
      activityIcon:<Icon 
        aria-label={getActivityIconAriaLabelFromContext(logMessage.context)} 
        styles={
          {root:{marginLeft:'10px'}}
        } 
        iconName={getIconNameForContext(logMessage.context)}/>,
      styles:{
        root: {
          alignItems:"center"
        },
        activityTypeIcon: {
          height:"16px"
        }
      },
      
      isCompact:false
    }
    
    activityItemsOrBreaks.push(<VsStyledActivityItem {...activityItemProps} key={i}/>);

    // works for ms code coverage but not for old as there are info messages before CoverageStart
    /* if(i !== 0 && logMessage.context === MessageContext.CoverageStart){
      activityItemsOrBreaks.push(<br key={`break${i}`}/>);
    } */
  })
  return <>
    <StyledActionButton ariaLabel='Clear log messages' iconProps={{iconName:'logRemove'}} onClick={clearLogMessages}/>
    {activityItemsOrBreaks}
  </>
}
