import { makeStyles, useDocument } from "@fluentui/react";
import React from "react";

export function useBodyToolWindow(bodyStyles:any){
    const bodyClasses = useBodyStyles(bodyStyles)();
    useApplyClassToBody([bodyClasses.body])
}
  
//todo type
const useBodyStyles = (bodyStyles:any) => {
    return makeStyles({
        body: bodyStyles
    } as Record<string, any>)
}
  
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