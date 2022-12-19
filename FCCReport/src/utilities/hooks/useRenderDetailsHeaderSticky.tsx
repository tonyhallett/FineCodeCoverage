import { IDetailsListProps, Sticky } from "@fluentui/react";
import React from "react";

export const useRenderDetailsHeaderSticky = (
    active: boolean,
    stickyCoverageTable: boolean
) => {
    const onRenderDetailsHeader = React.useCallback<
        NonNullable<IDetailsListProps["onRenderDetailsHeader"]>
    >(
        (detailsHeaderProps, defaultRender) => {
            if (
                detailsHeaderProps === undefined ||
                defaultRender === undefined
            ) {
                return null;
            }
            detailsHeaderProps.styles = {
                root: {
                    paddingTop: "0px",
                },
            };
            return active && stickyCoverageTable ? (
                <Sticky>{defaultRender(detailsHeaderProps)}</Sticky>
            ) : (
                defaultRender(detailsHeaderProps)
            );
        },
        [active, stickyCoverageTable]
    );

    return onRenderDetailsHeader;
};
