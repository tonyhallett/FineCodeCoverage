import { MutableRefObject, useRef } from "react";

export function useRefInitOnce<T>(initialValue: T) {
    const ref = useRef<T | null>();
    if (ref.current === undefined) {
        ref.current = initialValue;
    }
    return ref as MutableRefObject<T>;
}
