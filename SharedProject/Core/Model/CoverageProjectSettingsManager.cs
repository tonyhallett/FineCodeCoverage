using FineCodeCoverage.Options;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Documents;

namespace FineCodeCoverage.Engine.Model
{
    internal interface IKnownTestingFrameworksExcluder
    {
        IAppOptions Exclude(IAppOptions appOptions);
    }

    [Export(typeof(IKnownTestingFrameworksExcluder))]
    internal class KnownTestingFrameworksExcluder : IKnownTestingFrameworksExcluder
    {
        private readonly List<string> assemblyFileNames = new List<string>
        {
            "NUnit3.TestAdapter",
        };
        public IAppOptions Exclude(IAppOptions appOptions)
        {
            var msExcludes = ListFromExisting(appOptions.ModulePathsExclude);
            var oldExcludes = ListFromExisting(appOptions.Exclude);
            assemblyFileNames.ForEach(assemblyFileName =>
            {
                var msModulePathExclude = $".*\\{assemblyFileName}.dll$";
                msExcludes.Add(msModulePathExclude);
                var oldExclude = $"[{assemblyFileName}]*";
                oldExcludes.Add(oldExclude);
            });
            appOptions.ModulePathsExclude = msExcludes.ToArray();
            appOptions.Exclude = oldExcludes.ToArray();
            return appOptions;
        }

        private List<string> ListFromExisting(string[] existing)
        {
            return new List<string>(existing??new string[0]);
        }
    }

    [Export(typeof(ICoverageProjectSettingsManager))]
    internal class CoverageProjectSettingsManager : ICoverageProjectSettingsManager
    {
        private readonly IAppOptionsProvider appOptionsProvider;
        private readonly ICoverageProjectSettingsProvider coverageProjectSettingsProvider;
        private readonly IFCCSettingsFilesProvider fccSettingsFilesProvider;
        private readonly ISettingsMerger settingsMerger;
        private readonly IKnownTestingFrameworksExcluder knownTestingFrameworksExcluder;

        [ImportingConstructor]
        public CoverageProjectSettingsManager(
            IAppOptionsProvider appOptionsProvider,
            ICoverageProjectSettingsProvider coverageProjectSettingsProvider,
            IFCCSettingsFilesProvider fccSettingsFilesProvider,
            ISettingsMerger settingsMerger,
            IKnownTestingFrameworksExcluder knownTestingFrameworksExcluder
        )
        {
            this.appOptionsProvider = appOptionsProvider;
            this.coverageProjectSettingsProvider = coverageProjectSettingsProvider;
            this.fccSettingsFilesProvider = fccSettingsFilesProvider;
            this.settingsMerger = settingsMerger;
            this.knownTestingFrameworksExcluder = knownTestingFrameworksExcluder;
        }

        public async Task<IAppOptions> GetSettingsAsync(ICoverageProject coverageProject)
        {
            var projectDirectory = Path.GetDirectoryName(coverageProject.ProjectFile);
            var settingsFilesElements = fccSettingsFilesProvider.Provide(projectDirectory);
            var projectSettingsElement = await coverageProjectSettingsProvider.ProvideAsync(coverageProject);
            var mergedSettings = settingsMerger.Merge(appOptionsProvider.Get(), settingsFilesElements, projectSettingsElement);
            if (mergedSettings.ExcludeKnownTestingFrameworks)
            {
                return knownTestingFrameworksExcluder.Exclude(mergedSettings);
            }
            return mergedSettings;
        }
    }

}
