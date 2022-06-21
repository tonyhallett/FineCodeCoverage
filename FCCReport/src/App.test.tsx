import React from 'react';
import { act, fireEvent, getByRole, render, screen, waitFor, within } from '@testing-library/react';
import App from './App';
import { Payload } from './webviewListener';
import userEvent from '@testing-library/user-event';
import { Report, Styling } from './types';

/*
  https://jestjs.io/docs/26.x/manual-mocks#mocking-methods-which-are-not-implemented-in-jsdom

  if is used in a function invoked by test adding to window is ok otherwise see the link
*/

const anyWindow = window as any;

function expectTabTitles(tabList:HTMLElement,expectedTabTitles:string[]){
  const tabs = within(tabList).getAllByRole('tab');
  expect(tabs.length).toBe(expectedTabTitles.length);
  
  tabs.forEach((tab,i) => within(tab).getByText(expectedTabTitles[i]));
}

function expectOnlyFirstTabSelected(tabList:HTMLElement){
  const tabs = within(tabList).getAllByRole('tab');
  expect(tabs.length > 0).toEqual(true);
  tabs.forEach((tab,i) => {
    expect(tab).toHaveAttribute('aria-selected',i == 0 ? 'true' : 'false');
  })
}

describe('<App/>', () => {
  beforeEach(() => {
    delete anyWindow.report;
    delete anyWindow.styling;
    delete anyWindow.reportOptions;

    delete anyWindow.chrome;
  })

  describe('namespace grouping', () => {
    interface NamespaceTest{
      namespacedClass:string,
      expectedGroups:string[]
    }
    function getGrouping(namespacedClass:string,level:number):string{
      const parts = namespacedClass.split('.');
      const namespaceParts = parts.length - 1;
      if(namespaceParts === 0){
        return "Global";
      }
      const takeParts = namespaceParts < level ? namespaceParts : level;
      return parts.slice(0,takeParts).join('.');
    }

    it('works as expected', () => {
       
      const tests:NamespaceTest[] = [
        {
          namespacedClass:'NoNamespace',
          expectedGroups:['Global','Global', 'Global']
        },
        {
          namespacedClass:'MyProject.Class1',
          expectedGroups:['MyProject','MyProject','MyProject']
        },
        {
          namespacedClass:'MyProject.Nested1.NS1NestedLevel1',
          expectedGroups:['MyProject','MyProject.Nested1', 'MyProject.Nested1']
        },
        {
          namespacedClass:'MyProject.Nested1.Nested1_1.NS1NestedLevel2',
          expectedGroups:['MyProject','MyProject.Nested1','MyProject.Nested1.Nested1_1']
        },
        {
          namespacedClass:'MyProject.Nested2.NS2NestedLevel1',
          expectedGroups:['MyProject','MyProject.Nested2','MyProject.Nested2']
        },
        {
          namespacedClass:'MyProject.Nested2.Nested2_2.NS2NestedLevel2',
          expectedGroups:['MyProject','MyProject.Nested2','MyProject.Nested2.Nested2_2']
        },
        {
          namespacedClass:'OtherNamespace.Other',
          expectedGroups:['OtherNamespace','OtherNamespace','OtherNamespace']
        }
      ]
      tests.forEach(test => {
        test.expectedGroups.forEach((expectedGroup,index) => {
          expect(getGrouping(test.namespacedClass,index + 1)).toEqual(expectedGroup);
        })
        
      })
    })
  })

describe("Standalone", () => {
  describe('tabs', () => {
    let tabList:HTMLElement;
    beforeEach(() => {
      const styling:Styling = {
        fontName:"Arial",
        fontSize:"12px",
        categoryColours:{
          EnvironmentColors:{
            ToolWindowText:"red",
            ToolWindowBackground:"yellow"
          },
          CommonControlsColors:{
  
          }
        }
      }
      anyWindow.styling = styling;
      anyWindow.reportOptions = {
        namespacedClasses:true
      }
      // todo put back :Report
      const report:any = {
        riskHotspotsAnalysisThresholds: {
          MetricThresholdForCrapScore:1,
          MetricThresholdForCyclomaticComplexity:2,
          MetricThresholdForNPathComplexity:3
        },
        riskHotspotAnalysisResult:{
          codeCodeQualityMetricsAvailable:false,
          riskHotspots:[]
        },
        summaryResult: {
          assemblies:[
            {
              id:1,
              name:'a1Name',
              shortName:'a1ShortName',
              classes:[
                {
                  id:1,
                  name:'c1name',
                  displayName:'c1displayname',
                  files:[
                    {
                      path:'Class1File1Path'
                    }
                  ]
                }
              ]
            }
          ],
          coveredLines : 5,
          coverableLines: 10,
          totalLines : 100,
          coverageQuota : 0.5,
          coveredBranches:1,
          totalBranches:4,
          branchCoverageQuota:0.25,
          coveredCodeElements: 2,
          totalCodeElements: 10,
          codeElementCoverageQuota:0.2
        }
      }
      anyWindow.report = report;

      const {getByRole} = render(<App />);
      
      tabList = getByRole('tablist');
    });

    it('should have aria-label Coverage Report', () => {
      expect(tabList).toHaveAttribute('aria-label','Coverage Report');
    });

    it('should be missing Log', () => {
      expectTabTitles(tabList,['Coverage','Summary', 'Risk Hotspots']);
    });

    it('should have the first tab selected', () => {
      expectOnlyFirstTabSelected(tabList);
    });
  });
  
})

  describe('Webview app', () => {
    it('should render an empty div until it receives a WebView2 message', () => {
      /* (window as any).report = 123;
      const {container,getByText} = render(<App />);
      var rendered = container.firstChild; // Avoid direct Node access
      expect(rendered).toBeEmptyDOMElement(); */
      
      //expect(rendered).toHaveTextContent("Hello");
    });
    
    describe('when received initial style', () => {
      let chromeListener:((msgEvent:MessageEvent) => void) | null
      beforeEach(() => {
        anyWindow.chrome = {
          webview : {
            addEventListener:(_:string,listener:(msgEvent:MessageEvent) => void) => {
              chromeListener = listener;
            }
          }
        }
      })
        
      function sendStylingMessage(){
        const stylingMessageEvent:Partial<MessageEvent<Payload<Styling>>>={
          data:{
            type:'styling',
            data:{
              fontSize:"10px",
              fontName:"Arial",
              categoryColours:{
                EnvironmentColors:{
                  ToolWindowText: "red",
                  ToolWindowBackground:"yellow"
                },
                CommonControlsColors:{
                  
                }
              }
            }
          }
        }

        act(() => chromeListener!(stylingMessageEvent as any)); // typing todo
      }

      // todo - will be more interested in getComputedStyle for text elements
      it('should style the body from WebView2', () => {
        render(<App />);
        
        sendStylingMessage();
        expect(document.body.style.backgroundColor).toEqual("yellow");
        expect(document.body.style.color).toEqual("red");
        expect(document.body.style.fontSize).toEqual('10px');
        expect(document.body.style.fontFamily).toEqual('Arial');
      });

      describe("tabs", () => {
        // need to test for underline.....? Snapshot ?
        // should I create a wrapper div and add a class='tabs'
        let getTabPanels:() => HTMLElement[]
        let getTabList:() => HTMLElement
        beforeEach(() => {
          const {getByRole, getAllByRole} = render(<App />);
          getTabPanels = () => {
            return getAllByRole("tabpanel",{hidden:true})
          }
          getTabList = () => {
            return getByRole('tablist');
          }
          sendStylingMessage();
        });

        it('should have aria-label Live Coverage Report', () => {
          expect(getTabList()).toHaveAttribute('aria-label','Live Coverage Report');
        });

        it('should render a tab list with 4 tabs', () => {
          expectTabTitles(getTabList(),['Coverage','Summary', 'Risk Hotspots','Log']);
        });

        it('should render a tab panel for each tab', () => {
          screen.debug();
          // is this heeding the aria-hidden / hidden ( which means expectActivated not working as expected )
          expect(getTabPanels().length).toBe(4);
        });

        it('should have the first tab selected', () => {
          expectOnlyFirstTabSelected(getTabList());
        });

        it('should show the first tab panel', () => {
          const firstTab = within(getTabList()).getAllByRole('tab')[0];
          expectActivated(firstTab);
        })

        it('should have the first tab focused when tab to the tablist', async () => {
          const user = userEvent.setup()
          await user.tab();
          const firstTab = within(getTabList()).getAllByRole('tab')[0];
          expect(window.document.activeElement).toBe(firstTab);
        })

        it('should support mouse activation', async () => {
          const user = userEvent.setup();
          const secondTab = within(getTabList()).getAllByRole('tab')[1];
          await user.click(secondTab);

          expectActivated(secondTab);
        });

        function expectActivated(tab:HTMLElement){
          expect(tab).toHaveAttribute('aria-selected','true');
          const tabPanels = getTabPanels();
          tabPanels.forEach(tabPanel => {
            const labelledBy = tabPanel.getAttribute("aria-labelledby");
            if (labelledBy == tab.id){
              expect(tabPanel).toHaveAttribute('aria-hidden','false');
              expect(tabPanel).not.toHaveAttribute('hidden');
            }else{
              expect(tabPanel).toHaveAttribute('aria-hidden','true');
              expect(tabPanel).toHaveAttribute('hidden');
            }
          })
        }

        async function keyboardActivation(keyDownOptions:{}, expectedActivatedTabIndex:number){
          // for this to work the next tab needs to be visible.  In test environment offsetHeight does not work hence
          const tabList = getTabList();
          const tabs = within(tabList).getAllByRole('tab');
          tabs.forEach(tab => (tab as any).isVisible = true);

          // current active element is the body
          const user = userEvent.setup();

          /*
            Do not need to wrap in act as react testing library configures eventWrapper 
            https://github.com/testing-library/react-testing-library/blob/9171163fccf0a7ea43763475ca2980898b4079a5/src/pure.js#L15

            click becomes 
            https://github.com/testing-library/user-event/blob/ee062e762f9ac185d982dbf990387e97e05b3c9d/src/pointer/firePointerEvents.ts#L47
            https://github.com/testing-library/user-event/blob/ee062e762f9ac185d982dbf990387e97e05b3c9d/src/event/index.ts#L10 dispatchUIEvent
            https://github.com/testing-library/user-event/blob/ee062e762f9ac185d982dbf990387e97e05b3c9d/src/event/dispatchEvent.ts#L6
            *** through the eventWrapper 
            https://github.com/testing-library/user-event/blob/ee062e762f9ac185d982dbf990387e97e05b3c9d/src/event/wrapEvent.ts#L3
          */
          await user.click(tabs[0]);

          /*
            Important - cannot use as fluentui relies upon evt.which which is deprecated
            https://github.com/microsoft/fluentui/issues/22763

            await act(() => {
              return user.keyboard('{ArrowRight}');
          });
          */
          
        /*
          do not need to wrap in act as react testing library configures eventWrapper
          https://github.com/testing-library/react-testing-library/blob/9171163fccf0a7ea43763475ca2980898b4079a5/src/pure.js#L15

          used by fireEvent
          https://github.com/testing-library/dom-testing-library/blob/11fc7731e5081fd0994bf2da84d688fdb592eda7/src/events.js#L6
        */
         fireEvent.keyDown(window.document.activeElement!, keyDownOptions);

          expectActivated(tabs[expectedActivatedTabIndex])
        }
        
        it('should activate on keyboard navigation', async () => {
          await keyboardActivation({
            "key": "ArrowRight",
            "code": "ArrowRight",
            "charCode": 39,
            "keyCode":39
            },1)
        })

        it('should have circular keyboard navigation', async () => {
          await keyboardActivation({
            "key": "ArrowLeft",
            "code": "ArrowLeft",
            "charCode": 37,
            "keyCode":37
            },3)
          
        })

        xit('should tab from tablist to first focusable element in activated tab panel', () => {

        });
      });
    });
  })
})

