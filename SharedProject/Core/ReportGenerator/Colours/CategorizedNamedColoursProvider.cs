using Microsoft.VisualStudio.PlatformUI;
using System.Collections.Generic;
using System.ComponentModel.Composition;

namespace FineCodeCoverage.Core.ReportGenerator.Colours
{
    [Export(typeof(ICategorizedNamedColoursProvider))]
    internal class CategorizedNamedColoursProvider : ICategorizedNamedColoursProvider
    {
        private readonly IVsCategorizedNamedColoursPopulator vsCategorizedNamedColoursPopulator;
        
        [ImportingConstructor]
        public CategorizedNamedColoursProvider(IVsCategorizedNamedColoursPopulator vsCategorizedNamedColoursPopulator)
        {
            this.vsCategorizedNamedColoursPopulator = vsCategorizedNamedColoursPopulator;
        }

        public List<CategorizedNamedColours> Provide()
        {
            // todo custom
            CategorizedNamedColoursSpecification categoryColoursSpecification = new CategorizedNamedColoursSpecification
            {
                FullCategoryTypes = new List<string>
                    {
                        nameof(EnvironmentColors),
                        nameof(CommonControlsColors),
                        nameof(SearchControlColors),
                        nameof(HeaderColors),
                        nameof(ProgressBarColors),
                        nameof(TreeViewColors),
                    }
            };
        

            return vsCategorizedNamedColoursPopulator.Populate(categoryColoursSpecification);
        }
    }
}
