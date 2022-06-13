using FineCodeCoverage.Logging;
using Microsoft.VisualStudio.PlatformUI;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;

namespace FineCodeCoverage.Core.ReportGenerator.Colours
{
    [Export(typeof(IVsCategorizedNamedColoursPopulator))]
    internal class VsCategorizedNamedColoursPopulator : IVsCategorizedNamedColoursPopulator
    {
        private readonly IVsColourThemeResourceTypeProvider colourThemeResourceTypeProvider;
        private readonly ILogger logger;
        private Dictionary<string, CategorizedNamedColours> categorizedNamedColoursTypeNameLookup;

        private Dictionary<string, CategorizedNamedColours> CategoryColourTypeNameLookup
        {
            get
            {
                if (categorizedNamedColoursTypeNameLookup == null)
                {
                    var colourThemeResourceTypes = colourThemeResourceTypeProvider.Provide(typeof(EnvironmentColors).Assembly);

                    categorizedNamedColoursTypeNameLookup = colourThemeResourceTypes.ToDictionary(
                        colourThemeResourceType => colourThemeResourceType.Name,
                        colourThemeResourceType =>
                        {
                            return new CategorizedNamedColours
                            {
                                Name = colourThemeResourceType.Name,
                                NamedColours = colourThemeResourceType.ColourThemeResourceKeyProperties.Select(
                                    themeResourceKeyProperty =>
                                    {
                                        return new VsThemedNamedColour(themeResourceKeyProperty);

                                    }
                                ).ToList<INamedColour>()
                            };
                        }
                    );
                }
                return categorizedNamedColoursTypeNameLookup;
            }
        }


        [ImportingConstructor]
        public VsCategorizedNamedColoursPopulator(
            IVsColourThemeResourceTypeProvider colourThemeResourceTypeProvider,
            ILogger logger
        )
        {
            this.colourThemeResourceTypeProvider = colourThemeResourceTypeProvider;
            this.logger = logger;
        }

        public List<CategorizedNamedColours> Populate(CategorizedNamedColoursSpecification categoryColoursSpecification)
        {
            var categorizedNamedColoursList = categoryColoursSpecification.CategorizedNamedColoursList ?? new List<CategorizedNamedColours>();
            foreach (var fullCategoryType in categoryColoursSpecification.FullCategoryTypes)
            {
                if (CategoryColourTypeNameLookup.TryGetValue(fullCategoryType, out var categoryColour))
                {
                    categorizedNamedColoursList.Add(categoryColour);
                }
                else
                {
                    logger.Log($"Cannot find vs colours category type {fullCategoryType}");
                }
            }
            return categorizedNamedColoursList;
        }
    }

}
