import { IStyle, makeStyles, useDocument } from "@fluentui/react";
import React from "react";

export function useBodyStyling(bodyStyles: IStyle) {
    const bodyClasses = useBodyStyles(bodyStyles)();
    useApplyClassToBody([bodyClasses.body]);
}

const useBodyStyles = (bodyStyles: IStyle) => {
    return makeStyles({
        body: bodyStyles,
    });
};

function useApplyClassToBody(classesToApply: string[]): void {
    const body = useDocument()?.body;

    React.useEffect(() => {
        if (!body) {
            return;
        }

        for (const classToApply of classesToApply) {
            if (classToApply) {
                body.classList.add(classToApply);
            }
        }

        return () => {
            if (!body) {
                return;
            }

            for (const classToApply of classesToApply) {
                if (classToApply) {
                    body.classList.remove(classToApply);
                }
            }
        };
    }, [body, classesToApply]);
}
