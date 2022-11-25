import React, { useEffect, useReducer, useState } from 'react';
import { Payload, webviewPayloadTypeListen, webviewPayloadTypeUnlisten } from './webviewListener';
import { CustomizerContext, ProgressIndicator, registerIcons, ScrollablePane } from '@fluentui/react';
import { LogMessage, MessageContext, Report, ReportOptions, Styling } from './types';
import { ReportTab } from './ReportTab';
import{ SortDownIcon, SortUpIcon, ClearFilterIcon, FilterIcon, ChevronDownIcon, createSvgIcon, ChevronRightMedIcon, TagIcon, BeerMugIcon, GitHubLogoIcon, ReviewSolidIcon, InfoIcon, WarningIcon, ErrorIcon, CompletedIcon, TableIcon, ProcessingIcon, OpenPaneIcon, NavigateExternalInlineIcon, ErrorBadgeIcon, RunningIcon, DeveloperToolsIcon, ProcessingCancelIcon, LogRemoveIcon, GroupedDescendingIcon } from'@fluentui/react-icons-mdl2';
import { useRefInitOnce } from './utilities/hooks/useRefInitiOnce';
import { addScrollBarStyles, getBodyStyles, VsCustomizerContext } from './vs styling/themeStyles';
import { useBodyStyling } from './utilities/hooks/useBody';


//https://github.com/microsoft/fluentui/issues/22895
const VisualStudioIDELogo32Icon = createSvgIcon({
  svg: ({ classes }) => (
    <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 2048 2048" className={classes.svg}>
      <path d="M2048 213v1622l-512 213L0 1536l1536 223V0l512 213zM245 1199l-117-39V590l117-39 283 213 470-465 282 119v913l-282 120-470-466-283 214zm430-324l323 244V631L675 875zm-430 169l171-169-171-170v339z" />
    </svg>
  ),
  displayName: 'VisualStudioIDELogo32Icon',
});

const VisualStudioLogoIcon = createSvgIcon({
  svg: ({ classes }) => (
    <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 2048 2048" className={classes.svg}>
      <path d="M1472 128l448 171v1429l-448 192-704-704-469 363-171-107V577l171-108 469 363 704-704zM320 1280l256-256-256-256v512zm1152 128V640l-448 384 448 384z" />
    </svg>
  ),
  displayName: 'VisualStudioLogoIcon',
});


registerIcons({
  icons: {
    OpenFile: <VisualStudioIDELogo32Icon />,
    sortDown: <SortDownIcon/>,
    sortUp:<SortUpIcon/>,
    clear:<ClearFilterIcon/>,
    filter:<FilterIcon/>,
    chevrondown:<ChevronDownIcon/>,
    chevronrightmed:<ChevronRightMedIcon/>,
    tag:<TagIcon/>,
    beerMug:<BeerMugIcon/>,
    github:<GitHubLogoIcon/>,
    review:<ReviewSolidIcon/>,
    info:<InfoIcon/>,
    warning:<WarningIcon/>,
    error:<ErrorBadgeIcon/>,
    completed:<CompletedIcon/>,
    table:<TableIcon/>,
    processing:<ProcessingIcon/>,
    processingCancelled:<ProcessingCancelIcon/>,
    openPane:<OpenPaneIcon/>,
    navigate:<NavigateExternalInlineIcon/>,
    running:<RunningIcon/>,
    tool:<DeveloperToolsIcon/>,
    logRemove:<LogRemoveIcon/>,
    groupeddescending:<GroupedDescendingIcon/>,
  },
});





const anyWindow = window as any;


if(process.env.MOCK_WEBVIEW){
  let reactListener:(msgEvent:MessageEvent<Payload<unknown>>) => void
  anyWindow.invokeMessageEvent = (msgEvent:MessageEvent<Payload<unknown>>) => {
    reactListener(msgEvent);
  }
  anyWindow.chrome.webview = {
    addEventListener:(_:string,listener:(msgEvent:MessageEvent<Payload<unknown>>) => void) => {
      reactListener = listener;
    }
  }
}

interface NewMessageAction{
  type:'newMessage',
  payload:LogMessage
}
interface ClearMessagesAction{
  type:'clear'
}
function logMessagesReducer(logMessages:LogMessage[],action:NewMessageAction | ClearMessagesAction):LogMessage[]{
  switch(action.type){
    case 'newMessage':
      const newMessage = action.payload;
      return [newMessage,...logMessages];
    default:
      return [];

  }
}

function App() {
  const standalone = !!anyWindow.styling;
  const [logMessages, logMessagesDispatch] = useReducer(logMessagesReducer,[]);
  const [stylingState, setStyling] = useState<Styling>(standalone ? anyWindow.styling : undefined);
  const [reportState, setReport] = useState<Report>(standalone ? anyWindow.report : undefined);
  const [coverageRunning,setCoverageRunning] = useState(false);
  const [reportOptionsState, setReportOptions] = useState<ReportOptions>(standalone ? anyWindow.reportOptions : {});
  
  const clearLogMessages = React.useCallback(() => {
    logMessagesDispatch({
      type:'clear'
    })
  },[]);
  useEffect(() => {
    function stylingListener(styling:Styling){
      addScrollBarStyles(styling.categoryColours);
      setStyling(styling);
    }

    function reportOptionsListener(reportOptions:ReportOptions){
      setReportOptions(reportOptions);
    }

    function reportListener(report:Report){
      setReport(report);
    }

    function messageListener(logMessage:LogMessage){
      if (logMessage.context === MessageContext.CoverageStart){
        setCoverageRunning(true);
      }
      
      logMessagesDispatch({type:'newMessage',payload:logMessage});
    }

    function coverageStoppedListener(){
      setCoverageRunning(false);
    }

    if (!standalone){
      webviewPayloadTypeListen("report",reportListener);
      webviewPayloadTypeListen("styling",stylingListener);
      webviewPayloadTypeListen("reportOptions",reportOptionsListener);
      webviewPayloadTypeListen("message",messageListener);
      webviewPayloadTypeListen("coverageStopped",coverageStoppedListener)

      /* return function cleanUp(){
        webviewPayloadTypeUnlisten("styling",stylingListener);
        webviewPayloadTypeUnlisten("Report",reportListener);
        webviewPayloadTypeUnlisten("ReportOptions",reportOptionsListener);
        webviewPayloadTypeUnlisten("message",messageListener);
      } */
    }
    
  },[])

  const customizationStyling = useRefInitOnce(new VsCustomizerContext(
    stylingState,
    ))

  const bodyStyles = stylingState ? getBodyStyles(stylingState.categoryColours) : {}
  useBodyStyling(bodyStyles);

  if(!stylingState){
    return null;
  }

  customizationStyling.current = customizationStyling.current.getNext(
    stylingState
  );

  const percentComplete = coverageRunning ? undefined : 0;

  
  return (
    <CustomizerContext.Provider value={customizationStyling.current}>
      <ScrollablePane>
      {standalone ? null : <ProgressIndicator percentComplete={percentComplete}/> }
      <ReportTab 
        standalone={standalone} 
        report={reportState} 
        reportOptions={reportOptionsState} 
        logMessages={logMessages}
        clearLogMessages={clearLogMessages}
        />
        </ScrollablePane>
      </CustomizerContext.Provider>
  );
}



export default App;


