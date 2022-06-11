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
                        "EnvironmentColors",
                        "CommonControlsColors"
                    }
            };
        

            return vsCategorizedNamedColoursPopulator.Populate(categoryColoursSpecification);
        }
    }
}
