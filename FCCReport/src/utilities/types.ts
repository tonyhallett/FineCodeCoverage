export type KeysOfType<T, KT> = {
    [K in keyof T]: T[K] extends KT ? K : never;
}[keyof T];
