import { IIconProps } from "@fluentui/react";
import React from "react";
import { VsStyledActionButton } from "./vs-styling/VsStyledActionButton";
import { VsChromeWebViewWindow } from "./webviewTypes";

const vsWindow = window as unknown as VsChromeWebViewWindow;
const fccResourcesNavigator =
    vsWindow.chrome.webview.hostObjects.fccResourcesNavigator;

interface IFeedbackItem {
    key: string;
    text: string;
    iconProps: IIconProps;
    ariaLabel: string;
    onClick: () => void;
}

export function FeedbackBase() {
    const items: IFeedbackItem[] = [
        {
            key: "buyBeer",
            text: "Buy beer",
            iconProps: { iconName: "beerMug" },
            ariaLabel: "Buy me a beer",
            onClick() {
                fccResourcesNavigator.buyMeACoffee();
            },
        },
        {
            key: "logIssueOrSuggestion",
            text: "Issue / feature",
            iconProps: { iconName: "github" },
            ariaLabel: "Log issue or suggestion",
            onClick() {
                fccResourcesNavigator.logIssueOrSuggestion();
            },
        },
        {
            key: "rateAndReview",
            text: "Rate and review",
            iconProps: { iconName: "review" },
            ariaLabel: "Review",
            onClick() {
                fccResourcesNavigator.rateAndReview();
            },
        },
    ];

    return (
        <>
            {items.map((props) => (
                <VsStyledActionButton
                    key={props.key}
                    style={{ marginRight: "5px" }}
                    onClick={props.onClick}
                    iconProps={props.iconProps}
                >
                    {props.text}{" "}
                </VsStyledActionButton>
            ))}
        </>
    );
}

export const Feedback = React.memo(FeedbackBase);
