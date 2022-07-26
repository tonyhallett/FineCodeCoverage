declare module '@parcel/node-resolver-core' {
    import type {FileSystem} from '@parcel/fs';
    import type {PackageManager} from '@parcel/package-manager';
    import type {PluginLogger, ResolveResult} from '@parcel/types';
    export type {ResolveResult} from '@parcel/types';

    export type Nullable<T> = T | undefined | null;

    export type FilePath = string;
    

    // to type if required
    export type URLSearchParams = unknown
    export type ResolverContext = unknown
    export type Environment = unknown

    export type Module = {
        moduleName?: string,
        subPath?: Nullable<string>,
        moduleDir?: FilePath,
        filePath?: FilePath,
        code?: string,
        query?: URLSearchParams,
    };
    export type Options = {
        fs: FileSystem,
        projectRoot: FilePath,
        extensions: Array<string>,
        mainFields: Array<string>,
        packageManager?: PackageManager,
        logger?: PluginLogger,
    };
    export type ResolveArg = {
        filename:string,
        parent,
        specifierType,
        env,
        sourcePath,
        loc,
    }
    export type ResolvedAlias = {
        type: 'file' | 'global',
        sourcePath: FilePath,
        resolved: string,
    };
    class NodeResolver {
        projectRoot: FilePath
        constructor(opts: Options) {}
        resolve(resolveArg:ResolveArg):Promise<Nullable<ResolveResult>> 
        loadAlias(filename:string, sourceFile:FilePath, env:Environment,ctx:ResolverContext):Promise<Nullable<ResolvedAlias>>
        findNodeModulePath(filename:string,sourceFile:FilePath,ctx:ResolverContext):Module | null | undefined
    }
    export = NodeResolver
}