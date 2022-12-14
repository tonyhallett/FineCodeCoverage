export interface Assembly {
    name: string;
    shortName: string;
    classes: Class[];
    id: number;
}

export interface ClassCoverage {
    coveredLines: number;
    coverableLines: number;
    totalLines: number | null;
    coverageQuota: number | null;

    coveredBranches: number | null;
    totalBranches: number | null;
    branchCoverageQuota: number | null;

    coveredCodeElements: number;
    totalCodeElements: number;
    codeElementCoverageQuota: number | null;
}

export enum CoverageType {
    LineCoverage,
    MethodCoverage,
}

export type Class = ClassCoverage & {
    name: string;
    displayName: string;
    files: CodeFile[];
    assemblyIndex: number;
    coverageType: CoverageType;
};

export interface CodeFile {
    path: string;
}

export interface SummaryResult {
    assemblies: Assembly[];
    coveredLines: number;
    coverableLines: number;
    totalLines: number | null; //nullable
    coverageQuota: number | null; // nullable
    coveredBranches: number | null; //nullable
    totalBranches: number | null; // nullable
    branchCoverageQuota: number | null; //nullable

    coveredCodeElements: number;
    totalCodeElements: number;
    codeElementCoverageQuota: number | null; //nullable

    supportsBranchCoverage: boolean;
}

export interface Report {
    riskHotspotAnalysisResult: RiskHotspotAnalysisResult;
    riskHotspotsAnalysisThresholds: RiskHotspotsAnalysisThresholds;
    summaryResult: SummaryResult;
}

export interface RiskHotspotAnalysisResult {
    /*
        display differently if false  
    */
    codeCodeQualityMetricsAvailable: boolean;
    riskHotspots: RiskHotspot[];
}

/*
    a RiskHotspot occurs when a MethodMetric, the container for Metric objects that apply to a method, 
    has a code quality metric ( MetricType == MetricType.CodeQuality) that exceeds the threshold.
    It stores all code quality Metric in MetricStatus objects - check Exceeded for those that are an issue
  
  */

// applies to a method
export interface RiskHotspot {
    assemblyIndex: number;
    classIndex: number;
    methodMetric: MethodMetric;
    fileIndex: number;

    statusMetrics: MetricStatus[]; // this is what concerned with for columns
}

export interface MetricStatus {
    metricIndex: number;
    exceeded: boolean;
}

export interface MethodMetric {
    metrics: Metric[];

    fullName: string;
    shortName: string;
    line: number | null;
}

export enum MetricType {
    CoveragePercentual,
    CoverageAbsolute,
    CodeQuality,
}

export enum MetricMergeOrder {
    HigherIsBetter,
    LowerIsBetter,
}

export interface Metric {
    metricType: MetricType;
    mergeOrder: MetricMergeOrder;
    explanationUrl: string;
    name: string;
    value: number | null;
}

export interface RiskHotspotsAnalysisThresholds {
    MetricThresholdForCyclomaticComplexity: number;
    MetricThresholdForCrapScore: number;
    MetricThresholdForNPathComplexity: number;
}

type RGBA = `rgba(${number},${number},${number},${number})`;

export interface CategoryColours {
    SearchControlColors: {
        Unfocused: RGBA;
        UnfocusedBorder: RGBA;
        MouseOverBorder: RGBA;
        MouseOverBackground: RGBA;
        MouseOverSearchGlyph: RGBA;
        FocusedBorder: RGBA;
        FocusedBackground: RGBA;
        UnfocusedText: RGBA;
        SelectionText: RGBA;
        Selection: RGBA;
        MouseOverBackgroundText: RGBA;
        FocusedBackgroundText: RGBA;
        SearchGlyph: RGBA;
        MouseOverClearGlyph: RGBA;
        ClearGlyph: RGBA;
        FocusedClearGlyph: RGBA;
    };
    HeaderColors: {
        Default: RGBA;
        DefaultText: RGBA;
        Glyph: RGBA;
        SeparatorLine: RGBA;
        MouseOverGlyph: RGBA;
        MouseOver: RGBA;
        MouseOverText: RGBA;
        MouseDownGlyph: RGBA;
        MouseDown: RGBA;
        MouseDownText: RGBA;
    };
    ProgressBarColors: {
        Background: RGBA;
        IndicatorFill: RGBA;
    };
    TreeViewColors: {
        Background: RGBA;
        BackgroundText: RGBA;
        SelectedItemInactiveText: RGBA;
        SelectedItemInactive: RGBA;
        SelectedItemActiveText: RGBA;
        SelectedItemActive: RGBA;
    };
    EnvironmentColors: {
        PanelHyperlink: RGBA;
        PanelHyperlinkPressed: RGBA;
        PanelHyperlinkHover: RGBA;
        EnvironmentBackground: RGBA;
        ToolWindowText: RGBA;
        ToolWindowBackground: RGBA;
        ToolWindowBorder: RGBA;
        ToolWindowTabText: RGBA;
        ToolWindowTabGradientBegin: RGBA;
        ToolWindowTabBorder: RGBA;
        ToolWindowTabMouseOverText: RGBA;
        ToolWindowTabMouseOverBackgroundBegin: RGBA;
        ToolWindowTabMouseOverBorder: RGBA;
        ToolWindowTabSelectedText: RGBA;
        ToolWindowTabSelectedTab: RGBA;
        ToolWindowTabSelectedBorder: RGBA;
        ToolWindowTabSelectedActiveText: RGBA;
        ControlEditHintText: RGBA;
        CommandBarTextActive: RGBA;
        CommandBarTextHover: RGBA;
        CommandBarTextSelected: RGBA;
        CommandBarDragHandleShadow: RGBA;
        CommandBarDragHandle: RGBA;
        VizSurfaceGreenMedium: RGBA;
        ScrollBarThumbBackground: RGBA;
        ScrollBarThumbMouseOverBackground: RGBA;
        ScrollBarThumbPressedBackground: RGBA;
        ScrollBarBackground: RGBA;
        ScrollBarArrowBackground: RGBA;
        ScrollBarArrowMouseOverBackground: RGBA;
        ScrollBarArrowPressedBackground: RGBA;
        ScrollBarArrowGlyph: RGBA;
        ScrollBarArrowGlyphMouseOver: RGBA;
        ScrollBarArrowGlyphPressed: RGBA;
        ScrollBarBorder: RGBA;
        ScrollBarThumbBorder: RGBA;
        ScrollBarThumbMouseOverBorder: RGBA;
        ScrollBarThumbPressedBorder: RGBA;
        ScrollBarThumbGlyphMouseOverBorder: RGBA;
        ScrollBarThumbGlyphPressedBorder: RGBA;
    };
    CommonControlsColors: {
        FocusVisualText: RGBA;
        CheckBoxBackgroundHover: RGBA;
        CheckBoxBorderHover: RGBA;
        CheckBoxBackgroundFocused: RGBA;
        CheckBoxBorderFocused: RGBA;
        CheckBoxGlyphHover: RGBA;
        CheckBoxGlyphFocused: RGBA;
        CheckBoxTextHover: RGBA;
        CheckBoxTextFocused: RGBA;
        CheckBoxBorder: RGBA;
        CheckBoxBackground: RGBA;
        CheckBoxBorderDisabled: RGBA;
        CheckBoxBackgroundDisabled: RGBA;
        CheckBoxGlyphDisabled: RGBA;
        CheckBoxTextDisabled: RGBA;
        CheckBoxGlyph: RGBA;
        CheckBoxText: RGBA;
        ComboBoxListItemTextHover: RGBA;
        ComboBoxListItemBackgroundHover: RGBA;
        ComboBoxListItemBorderHover: RGBA;
        ComboBoxListItemText: RGBA;
        ComboBoxTextInputSelection: RGBA;
        ComboBoxListBackground: RGBA;
        ComboBoxListBackgroundShadow: RGBA;
        ComboBoxText: RGBA;
        ComboBoxBorder: RGBA;
        ComboBoxTextHover: RGBA;
        ComboBoxBackgroundHover: RGBA;
        ComboBoxBorderHover: RGBA;
        ComboBoxTextFocused: RGBA;
        ComboBoxBorderFocused: RGBA;
        ComboBoxBackgroundFocused: RGBA;
        ComboBoxTextPressed: RGBA;
        ComboBoxBackgroundPressed: RGBA;
        ComboBoxBorderPressed: RGBA;
        ComboBoxGlyphHover: RGBA;
        ComboBoxGlyphFocused: RGBA;
        ComboBoxGlyphPressed: RGBA;
        ComboBoxGlyphBackgroundFocused: RGBA;
        ComboBoxListBorder: RGBA;
        ComboBoxBackground: RGBA;
        ComboBoxGlyph: RGBA;
        ComboBoxSeparator: RGBA;
        ComboBoxSeparatorPressed: RGBA;
        ComboBoxSeparatorHover: RGBA;
        ComboBoxSeparatorFocused: RGBA;
        ComboBoxGlyphBackground: RGBA;
        ComboBoxGlyphBackgroundPressed: RGBA;
        ComboBoxGlyphBackgroundHover: RGBA;
        ButtonBorderPressed: RGBA;
        ButtonBorder: RGBA;
        Button: RGBA;
        ButtonText: RGBA;
        ButtonFocusedText: RGBA;
        ButtonFocused: RGBA;
        ButtonBorderFocused: RGBA;
        ButtonHoverText: RGBA;
        ButtonHover: RGBA;
        ButtonBorderHover: RGBA;
        ButtonDisabledText: RGBA;
        ButtonDisabled: RGBA;
        ButtonBorderDisabled: RGBA;
        ButtonPressedText: RGBA;
        ButtonPressed: RGBA;
    };
}

export interface FontStyling {
    fontSize: string;
    fontName: string;
}

export interface Styling extends FontStyling {
    categoryColours: CategoryColours;
    themeIsHighContrast: boolean;
}

export interface ReportOptions {
    namespacedClasses: boolean;
    hideFullyCovered: boolean;
    stickyCoverageTable: boolean;
}

export enum MessageContext {
    Info,
    Warning,
    Error,
    CoverageStart,
    CoverageCancelled,
    CoverageCompleted,
    CoverageToolStart,
    CoverageToolCompleted,
    ReportGeneratorStart,
    ReportGeneratorCompleted,
    TaskStarted,
    TaskCompleted, // file synchronization
}
export interface LogMessage {
    context: MessageContext;
    message: (Emphasized | FCCLink)[];
}

export enum Emphasis {
    None = 0,
    Bold = 1,
    Italic = 2,
    Underline = 4,
}

export interface Emphasized {
    message: string;
    emphasis: Emphasis;
    type: "emphasized";
}
export interface FCCLink {
    hostObject: string;
    methodName: string;
    arguments?: any[];
    title: string;
    ariaLabel: string;
    type: "fcclink";
}
