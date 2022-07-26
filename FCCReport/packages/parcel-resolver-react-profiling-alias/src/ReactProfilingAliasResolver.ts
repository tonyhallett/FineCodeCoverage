import {Resolver} from '@parcel/plugin';
import {PluginLogger, Dependency} from '@parcel/types'
import NodeResolver, { FilePath, Options, Environment, ResolverContext, Nullable, ResolveResult } from '@parcel/node-resolver-core';

const reactDom = 'react-dom';
const schedulerTracing = 'scheduler/tracing';

const logOrigin = 'ReactProfilingAliasResolver';

function createDiagnosticWithOrigin(message:string){
  return {
    message,
    origin:logOrigin
  }
}

const shouldLog = true
function logResolveResult(logger:PluginLogger,resolved:Nullable<ResolveResult>, dependency:Dependency){
  if (shouldLog) {
    logger.log(createDiagnosticWithOrigin(`parent - ${dependency.resolveFrom!}`));
      
    if(resolved){
      logger.log(createDiagnosticWithOrigin(`resolved ${dependency.specifier} ${JSON.stringify(resolved)}`));
    }else{
      logger.log(createDiagnosticWithOrigin(`failed to resolve ${dependency.specifier}`));
    }
  }
}

function shouldProfile(specifier:string){
  return process.env.REACT_PROFILING !== undefined && (specifier === reactDom || specifier === schedulerTracing);
}

export default new Resolver({
  async resolve(arg) { 
    const {specifier, dependency,options} = arg;
    const logger = arg.logger;
    if (shouldProfile(specifier)){

      const resolver = new ReactAliasingNodeResolver({
        fs: options.inputFS,
        projectRoot: options.projectRoot,
        // Extensions are always required in URL dependencies.
        extensions:
          dependency.specifierType === 'commonjs' ||
          dependency.specifierType === 'esm'
            ? ['ts', 'tsx', 'js', 'jsx', 'json']
            : [],
        mainFields: ['source', 'browser', 'module', 'main'],
        packageManager: options.shouldAutoInstall
          ? options.packageManager
          : undefined,
        logger,
      });
  
      const resolved = await resolver.resolve({
        filename: specifier,
        specifierType: dependency.specifierType,
        parent: dependency.resolveFrom,
        env: dependency.env,
        sourcePath: dependency.sourcePath,
        loc: dependency.loc,
      });
      
      logResolveResult(logger,resolved, dependency);
      return resolved;
    }
    if(shouldLog && (specifier === reactDom || specifier === schedulerTracing)){
      logger.log(createDiagnosticWithOrigin('Did not profile react ****************************************'))
    }
   
    return null;
  }
});


class ReactAliasingNodeResolver extends NodeResolver{
  constructor(opts:Options){
    super(opts)
  }

  async loadAlias(filename:string, _sourceFile:FilePath, _env:Environment,_ctx:ResolverContext): Promise<Nullable<NodeResolver.ResolvedAlias>> {
    const resolved = filename == reactDom ? 'react-dom/profiling' : 'scheduler/tracing-profiling';
    return {
      type:'file',
      sourcePath:'', // not used
      resolved
    }
  }
  
  /* 
  probably ok too

  async resolveModule(arg:{_filename:string,parent:FilePath,_env:Environment,ctx:ResolverContext,_sourcePath:FilePath}):Promise<Nullable<Module>> {
    const res =  super.findNodeModulePath('react-dom/profiling', arg.parent, arg.ctx);
    return res;
  } */
}
