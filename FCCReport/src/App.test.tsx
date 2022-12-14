import React from "react";
import {
    act,
    fireEvent,
    getByRole,
    render,
    screen,
    waitFor,
    within,
} from "@testing-library/react";
import App from "./App";
import { Payload } from "./webviewListener";
import userEvent from "@testing-library/user-event";
import { CategoryColours, Report, Styling } from "./types";

/*
  https://jestjs.io/docs/26.x/manual-mocks#mocking-methods-which-are-not-implemented-in-jsdom

  if is used in a function invoked by test adding to window is ok otherwise see the link
*/

const anyWindow = window as any;

function expectTabTitles(tabList: HTMLElement, expectedTabTitles: string[]) {
    const tabs = within(tabList).getAllByRole("tab");
    expect(tabs.length).toBe(expectedTabTitles.length);

    tabs.forEach((tab, i) => within(tab).getByText(expectedTabTitles[i]));
}

function expectOnlyFirstTabSelected(tabList: HTMLElement) {
    const tabs = within(tabList).getAllByRole("tab");
    expect(tabs.length > 0).toEqual(true);
    tabs.forEach((tab, i) => {
        expect(tab).toHaveAttribute("aria-selected", i == 0 ? "true" : "false");
    });
}

function getCategoryColours(): CategoryColours {
    return {
        EnvironmentColors: {
            ToolWindowText: "rgba(1,2,3,1)",
            ToolWindowBackground: "rgba(1,2,3,1)",
            CommandBarDragHandle: "rgba(1,2,3,1)",
            CommandBarDragHandleShadow: "rgba(1,2,3,1)",
            CommandBarTextActive: "rgba(1,2,3,1)",
            CommandBarTextHover: "rgba(1,2,3,1)",
            CommandBarTextSelected: "rgba(1,2,3,1)",
            ControlEditHintText: "rgba(1,2,3,1)",
            EnvironmentBackground: "rgba(1,2,3,1)",
            PanelHyperlink: "rgba(1,2,3,1)",
            PanelHyperlinkHover: "rgba(1,2,3,1)",
            PanelHyperlinkPressed: "rgba(1,2,3,1)",
            ScrollBarArrowBackground: "rgba(1,2,3,1)",
            ScrollBarArrowGlyph: "rgba(1,2,3,1)",
            ScrollBarArrowGlyphMouseOver: "rgba(1,2,3,1)",
            ScrollBarArrowGlyphPressed: "rgba(1,2,3,1)",
            ScrollBarArrowMouseOverBackground: "rgba(1,2,3,1)",
            ScrollBarArrowPressedBackground: "rgba(1,2,3,1)",
            ScrollBarBackground: "rgba(1,2,3,1)",
            ScrollBarBorder: "rgba(1,2,3,1)",
            ScrollBarThumbBackground: "rgba(1,2,3,1)",
            ScrollBarThumbBorder: "rgba(1,2,3,1)",
            ScrollBarThumbGlyphMouseOverBorder: "rgba(1,2,3,1)",
            ScrollBarThumbGlyphPressedBorder: "rgba(1,2,3,1)",
            ScrollBarThumbMouseOverBackground: "rgba(1,2,3,1)",
            ScrollBarThumbMouseOverBorder: "rgba(1,2,3,1)",
            ScrollBarThumbPressedBackground: "rgba(1,2,3,1)",
            ScrollBarThumbPressedBorder: "rgba(1,2,3,1)",
            ToolWindowBorder: "rgba(1,2,3,1)",
            ToolWindowTabBorder: "rgba(1,2,3,1)",
            ToolWindowTabGradientBegin: "rgba(1,2,3,1)",
            ToolWindowTabMouseOverBackgroundBegin: "rgba(1,2,3,1)",
            ToolWindowTabMouseOverBorder: "rgba(1,2,3,1)",
            ToolWindowTabMouseOverText: "rgba(1,2,3,1)",
            ToolWindowTabSelectedActiveText: "rgba(1,2,3,1)",
            ToolWindowTabSelectedBorder: "rgba(1,2,3,1)",
            ToolWindowTabSelectedTab: "rgba(1,2,3,1)",
            ToolWindowTabSelectedText: "rgba(1,2,3,1)",
            ToolWindowTabText: "rgba(1,2,3,1)",
            VizSurfaceGreenMedium: "rgba(1,2,3,1)",
        },
        CommonControlsColors: {
            ComboBoxGlyphBackground: "rgba(1,2,3,1)",
            ComboBoxGlyphBackgroundFocused: "rgba(1,2,3,1)",
            ComboBoxGlyphBackgroundHover: "rgba(1,2,3,1)",
            ComboBoxGlyphBackgroundPressed: "rgba(1,2,3,1)",
            ComboBoxListBackgroundShadow: "rgba(1,2,3,1)",
            ComboBoxSeparator: "rgba(1,2,3,1)",
            ComboBoxSeparatorFocused: "rgba(1,2,3,1)",
            ComboBoxSeparatorHover: "rgba(1,2,3,1)",
            ComboBoxSeparatorPressed: "rgba(1,2,3,1)",
            Button: "rgba(1,2,3,1)",
            ButtonBorder: "rgba(1,2,3,1)",
            ButtonBorderDisabled: "rgba(1,2,3,1)",
            ButtonBorderFocused: "rgba(1,2,3,1)",
            ButtonBorderHover: "rgba(1,2,3,1)",
            ButtonBorderPressed: "rgba(1,2,3,1)",
            ButtonDisabled: "rgba(1,2,3,1)",
            ButtonDisabledText: "rgba(1,2,3,1)",
            ButtonFocused: "rgba(1,2,3,1)",
            ButtonFocusedText: "rgba(1,2,3,1)",
            ButtonHover: "rgba(1,2,3,1)",
            ButtonHoverText: "rgba(1,2,3,1)",
            ButtonPressed: "rgba(1,2,3,1)",
            ButtonPressedText: "rgba(1,2,3,1)",
            ButtonText: "rgba(1,2,3,1)",
            CheckBoxBackground: "rgba(1,2,3,1)",
            CheckBoxBackgroundDisabled: "rgba(1,2,3,1)",
            CheckBoxBackgroundFocused: "rgba(1,2,3,1)",
            CheckBoxBackgroundHover: "rgba(1,2,3,1)",
            CheckBoxBorder: "rgba(1,2,3,1)",
            CheckBoxBorderDisabled: "rgba(1,2,3,1)",
            CheckBoxBorderFocused: "rgba(1,2,3,1)",
            CheckBoxBorderHover: "rgba(1,2,3,1)",
            CheckBoxGlyph: "rgba(1,2,3,1)",
            CheckBoxGlyphDisabled: "rgba(1,2,3,1)",
            CheckBoxGlyphFocused: "rgba(1,2,3,1)",
            CheckBoxGlyphHover: "rgba(1,2,3,1)",
            CheckBoxText: "rgba(1,2,3,1)",
            CheckBoxTextDisabled: "rgba(1,2,3,1)",
            CheckBoxTextFocused: "rgba(1,2,3,1)",
            CheckBoxTextHover: "rgba(1,2,3,1)",
            ComboBoxBackground: "rgba(1,2,3,1)",
            ComboBoxBackgroundFocused: "rgba(1,2,3,1)",
            ComboBoxBackgroundHover: "rgba(1,2,3,1)",
            ComboBoxBackgroundPressed: "rgba(1,2,3,1)",
            ComboBoxBorder: "rgba(1,2,3,1)",
            ComboBoxBorderFocused: "rgba(1,2,3,1)",
            ComboBoxBorderHover: "rgba(1,2,3,1)",
            ComboBoxBorderPressed: "rgba(1,2,3,1)",
            ComboBoxGlyph: "rgba(1,2,3,1)",
            ComboBoxGlyphFocused: "rgba(1,2,3,1)",
            ComboBoxGlyphHover: "rgba(1,2,3,1)",
            ComboBoxGlyphPressed: "rgba(1,2,3,1)",
            ComboBoxListBackground: "rgba(1,2,3,1)",
            ComboBoxListBorder: "rgba(1,2,3,1)",
            ComboBoxListItemBackgroundHover: "rgba(1,2,3,1)",
            ComboBoxListItemBorderHover: "rgba(1,2,3,1)",
            ComboBoxListItemText: "rgba(1,2,3,1)",
            ComboBoxListItemTextHover: "rgba(1,2,3,1)",
            ComboBoxText: "rgba(1,2,3,1)",
            ComboBoxTextFocused: "rgba(1,2,3,1)",
            ComboBoxTextHover: "rgba(1,2,3,1)",
            ComboBoxTextInputSelection: "rgba(1,2,3,1)",
            ComboBoxTextPressed: "rgba(1,2,3,1)",
            FocusVisualText: "rgba(1,2,3,1)",
        },
        HeaderColors: {
            Default: "rgba(1,2,3,1)",
            DefaultText: "rgba(1,2,3,1)",
            Glyph: "rgba(1,2,3,1)",
            MouseDown: "rgba(1,2,3,1)",
            MouseDownGlyph: "rgba(1,2,3,1)",
            MouseDownText: "rgba(1,2,3,1)",
            MouseOver: "rgba(1,2,3,1)",
            MouseOverGlyph: "rgba(1,2,3,1)",
            MouseOverText: "rgba(1,2,3,1)",
            SeparatorLine: "rgba(1,2,3,1)",
        },
        ProgressBarColors: {
            Background: "rgba(1,2,3,1)",
            IndicatorFill: "rgba(1,2,3,1)",
        },
        SearchControlColors: {
            ClearGlyph: "rgba(1,2,3,1)",
            FocusedBackground: "rgba(1,2,3,1)",
            FocusedBackgroundText: "rgba(1,2,3,1)",
            FocusedBorder: "rgba(1,2,3,1)",
            FocusedClearGlyph: "rgba(1,2,3,1)",
            MouseOverBackground: "rgba(1,2,3,1)",
            MouseOverBackgroundText: "rgba(1,2,3,1)",
            MouseOverBorder: "rgba(1,2,3,1)",
            MouseOverClearGlyph: "rgba(1,2,3,1)",
            MouseOverSearchGlyph: "rgba(1,2,3,1)",
            SearchGlyph: "rgba(1,2,3,1)",
            Selection: "rgba(1,2,3,1)",
            SelectionText: "rgba(1,2,3,1)",
            Unfocused: "rgba(1,2,3,1)",
            UnfocusedBorder: "rgba(1,2,3,1)",
            UnfocusedText: "rgba(1,2,3,1)",
        },
        TreeViewColors: {
            Background: "rgba(1,2,3,1)",
            BackgroundText: "rgba(1,2,3,1)",
            SelectedItemActive: "rgba(1,2,3,1)",
            SelectedItemActiveText: "rgba(1,2,3,1)",
            SelectedItemInactive: "rgba(1,2,3,1)",
            SelectedItemInactiveText: "rgba(1,2,3,1)",
        },
    };
}

describe("<App/>", () => {
    beforeEach(() => {
        delete anyWindow.report;
        delete anyWindow.styling;
        delete anyWindow.reportOptions;

        delete anyWindow.chrome;
    });

    describe("namespace grouping", () => {
        interface NamespaceTest {
            namespacedClass: string;
            expectedGroups: string[];
        }
        function getGrouping(namespacedClass: string, level: number): string {
            const parts = namespacedClass.split(".");
            const namespaceParts = parts.length - 1;
            if (namespaceParts === 0) {
                return "Global";
            }
            const takeParts = namespaceParts < level ? namespaceParts : level;
            return parts.slice(0, takeParts).join(".");
        }

        it("works as expected", () => {
            const tests: NamespaceTest[] = [
                {
                    namespacedClass: "NoNamespace",
                    expectedGroups: ["Global", "Global", "Global"],
                },
                {
                    namespacedClass: "MyProject.Class1",
                    expectedGroups: ["MyProject", "MyProject", "MyProject"],
                },
                {
                    namespacedClass: "MyProject.Nested1.NS1NestedLevel1",
                    expectedGroups: [
                        "MyProject",
                        "MyProject.Nested1",
                        "MyProject.Nested1",
                    ],
                },
                {
                    namespacedClass:
                        "MyProject.Nested1.Nested1_1.NS1NestedLevel2",
                    expectedGroups: [
                        "MyProject",
                        "MyProject.Nested1",
                        "MyProject.Nested1.Nested1_1",
                    ],
                },
                {
                    namespacedClass: "MyProject.Nested2.NS2NestedLevel1",
                    expectedGroups: [
                        "MyProject",
                        "MyProject.Nested2",
                        "MyProject.Nested2",
                    ],
                },
                {
                    namespacedClass:
                        "MyProject.Nested2.Nested2_2.NS2NestedLevel2",
                    expectedGroups: [
                        "MyProject",
                        "MyProject.Nested2",
                        "MyProject.Nested2.Nested2_2",
                    ],
                },
                {
                    namespacedClass: "OtherNamespace.Other",
                    expectedGroups: [
                        "OtherNamespace",
                        "OtherNamespace",
                        "OtherNamespace",
                    ],
                },
            ];
            tests.forEach((test) => {
                test.expectedGroups.forEach((expectedGroup, index) => {
                    expect(
                        getGrouping(test.namespacedClass, index + 1)
                    ).toEqual(expectedGroup);
                });
            });
        });
    });

    describe("Standalone", () => {
        describe("tabs", () => {
            let tabList: HTMLElement;
            beforeEach(() => {
                const styling: Styling = {
                    fontName: "Arial",
                    fontSize: "12px",
                    categoryColours: getCategoryColours(),
                    themeIsHighContrast: false,
                };
                anyWindow.styling = styling;
                anyWindow.reportOptions = {
                    namespacedClasses: true,
                };
                // todo put back :Report
                const report: any = {
                    riskHotspotsAnalysisThresholds: {
                        MetricThresholdForCrapScore: 1,
                        MetricThresholdForCyclomaticComplexity: 2,
                        MetricThresholdForNPathComplexity: 3,
                    },
                    riskHotspotAnalysisResult: {
                        codeCodeQualityMetricsAvailable: false,
                        riskHotspots: [],
                    },
                    summaryResult: {
                        assemblies: [
                            {
                                id: 1,
                                name: "a1Name",
                                shortName: "a1ShortName",
                                classes: [
                                    {
                                        id: 1,
                                        name: "c1name",
                                        displayName: "c1displayname",
                                        files: [
                                            {
                                                path: "Class1File1Path",
                                            },
                                        ],
                                    },
                                ],
                            },
                        ],
                        coveredLines: 5,
                        coverableLines: 10,
                        totalLines: 100,
                        coverageQuota: 0.5,
                        coveredBranches: 1,
                        totalBranches: 4,
                        branchCoverageQuota: 0.25,
                        coveredCodeElements: 2,
                        totalCodeElements: 10,
                        codeElementCoverageQuota: 0.2,
                    },
                };
                anyWindow.report = report;

                const { getByRole } = render(<App />);

                tabList = getByRole("tablist");
            });

            it("should have aria-label Coverage Report", () => {
                expect(tabList).toHaveAttribute(
                    "aria-label",
                    "Coverage Report"
                );
            });

            it("should be missing Log", () => {
                expectTabTitles(tabList, [
                    "Coverage",
                    "Summary",
                    "Risk Hotspots",
                ]);
            });

            it("should have the first tab selected", () => {
                expectOnlyFirstTabSelected(tabList);
            });
        });
    });

    describe("Webview app", () => {
        it("should render an empty div until it receives a WebView2 message", () => {
            /* (window as any).report = 123;
      const {container,getByText} = render(<App />);
      var rendered = container.firstChild; // Avoid direct Node access
      expect(rendered).toBeEmptyDOMElement(); */
            //expect(rendered).toHaveTextContent("Hello");
        });

        describe("when received initial style", () => {
            let chromeListener: ((msgEvent: MessageEvent) => void) | null;
            beforeEach(() => {
                anyWindow.chrome = {
                    webview: {
                        addEventListener: (
                            _: string,
                            listener: (msgEvent: MessageEvent) => void
                        ) => {
                            chromeListener = listener;
                        },
                    },
                };
            });

            function sendStylingMessage() {
                const stylingMessageEvent: Partial<
                    MessageEvent<Payload<Styling>>
                > = {
                    data: {
                        type: "styling",
                        data: {
                            fontSize: "10px",
                            fontName: "Arial",
                            categoryColours: getCategoryColours(),
                            themeIsHighContrast: false,
                        },
                    },
                };

                act(() => chromeListener!(stylingMessageEvent as any)); // typing todo
            }

            // todo - will be more interested in getComputedStyle for text elements
            it("should style the body from WebView2", () => {
                render(<App />);

                sendStylingMessage();
                expect(document.body.style.backgroundColor).toEqual("yellow");
                expect(document.body.style.color).toEqual("red");
                expect(document.body.style.fontSize).toEqual("10px");
                expect(document.body.style.fontFamily).toEqual("Arial");
            });

            describe("tabs", () => {
                // need to test for underline.....? Snapshot ?
                // should I create a wrapper div and add a class='tabs'
                let getTabPanels: () => HTMLElement[];
                let getTabList: () => HTMLElement;
                beforeEach(() => {
                    const { getByRole, getAllByRole } = render(<App />);
                    getTabPanels = () => {
                        return getAllByRole("tabpanel", { hidden: true });
                    };
                    getTabList = () => {
                        return getByRole("tablist");
                    };
                    sendStylingMessage();
                });

                it("should have aria-label Live Coverage Report", () => {
                    expect(getTabList()).toHaveAttribute(
                        "aria-label",
                        "Live Coverage Report"
                    );
                });

                it("should render a tab list with 4 tabs", () => {
                    expectTabTitles(getTabList(), [
                        "Coverage",
                        "Summary",
                        "Risk Hotspots",
                        "Log",
                    ]);
                });

                it("should render a tab panel for each tab", () => {
                    screen.debug();
                    // is this heeding the aria-hidden / hidden ( which means expectActivated not working as expected )
                    expect(getTabPanels().length).toBe(4);
                });

                it("should have the first tab selected", () => {
                    expectOnlyFirstTabSelected(getTabList());
                });

                it("should show the first tab panel", () => {
                    const firstTab = within(getTabList()).getAllByRole(
                        "tab"
                    )[0];
                    expectActivated(firstTab);
                });

                it("should have the first tab focused when tab to the tablist", async () => {
                    const user = userEvent.setup();
                    await user.tab();
                    const firstTab = within(getTabList()).getAllByRole(
                        "tab"
                    )[0];
                    expect(window.document.activeElement).toBe(firstTab);
                });

                it("should support mouse activation", async () => {
                    const user = userEvent.setup();
                    const secondTab = within(getTabList()).getAllByRole(
                        "tab"
                    )[1];
                    await user.click(secondTab);

                    expectActivated(secondTab);
                });

                function expectActivated(tab: HTMLElement) {
                    expect(tab).toHaveAttribute("aria-selected", "true");
                    const tabPanels = getTabPanels();
                    tabPanels.forEach((tabPanel) => {
                        const labelledBy =
                            tabPanel.getAttribute("aria-labelledby");
                        if (labelledBy == tab.id) {
                            expect(tabPanel).toHaveAttribute(
                                "aria-hidden",
                                "false"
                            );
                            expect(tabPanel).not.toHaveAttribute("hidden");
                        } else {
                            expect(tabPanel).toHaveAttribute(
                                "aria-hidden",
                                "true"
                            );
                            expect(tabPanel).toHaveAttribute("hidden");
                        }
                    });
                }

                async function keyboardActivation(
                    keyDownOptions: {},
                    expectedActivatedTabIndex: number
                ) {
                    // for this to work the next tab needs to be visible.  In test environment offsetHeight does not work hence
                    const tabList = getTabList();
                    const tabs = within(tabList).getAllByRole("tab");
                    tabs.forEach((tab) => ((tab as any).isVisible = true));

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
                    fireEvent.keyDown(
                        window.document.activeElement!,
                        keyDownOptions
                    );

                    expectActivated(tabs[expectedActivatedTabIndex]);
                }

                it("should activate on keyboard navigation", async () => {
                    await keyboardActivation(
                        {
                            key: "ArrowRight",
                            code: "ArrowRight",
                            charCode: 39,
                            keyCode: 39,
                        },
                        1
                    );
                });

                it("should have circular keyboard navigation", async () => {
                    await keyboardActivation(
                        {
                            key: "ArrowLeft",
                            code: "ArrowLeft",
                            charCode: 37,
                            keyCode: 37,
                        },
                        3
                    );
                });

                xit("should tab from tablist to first focusable element in activated tab panel", () => {});
            });
        });
    });
});
