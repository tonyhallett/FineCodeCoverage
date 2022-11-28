using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;

namespace FineCodeCoverage.Core.ReportGenerator.Colours
{
    [Export(typeof(IReportColoursProvider))]
    internal class ReportColoursProvider : IReportColoursProvider
    {
        private readonly ICategorizedNamedColoursProvider categorizedNamedColoursProvider;
        private readonly IVsColourTheme vsColourTheme;

        public event EventHandler<CategorizedNamedColoursChangedArgs> CategorizedNamedColoursChanged;

        private List<CategorizedNamedColours> categorizedNamedColoursList; 

        [ImportingConstructor]
        public ReportColoursProvider(
            ICategorizedNamedColoursProvider categorizedNamedColoursProvider,
            IVsColourTheme vsColourTheme
        )
        {
            this.categorizedNamedColoursProvider = categorizedNamedColoursProvider;
            this.vsColourTheme = vsColourTheme;
            vsColourTheme.ThemeChanged += VSColourTheme_ThemeChanged;
        }

        private (List<CategorizedNamedColours>, bool) GetCategorizedNamedColoursListWithUpdated()
        {
            var updatedColours = false;
            if (categorizedNamedColoursList == null)
            {
                categorizedNamedColoursList = categorizedNamedColoursProvider.Provide();
            }
            
            foreach (var categorizedNamedColours in categorizedNamedColoursList)
            {
                foreach (var namedColour in categorizedNamedColours.NamedColours)
                {
                    var updatedColour = namedColour.UpdateColour(vsColourTheme);
                    updatedColours = updatedColours || updatedColour;
                }
            }
            return (categorizedNamedColoursList, updatedColours);
        }

        private void VSColourTheme_ThemeChanged(object sender, ColourThemeChangedArgs args)
        {
            var themeIsHighContrast = ThemeIsHighContrast(args.ThemeId);
            var (newListCategoryNamedColours, updated) = GetCategorizedNamedColoursListWithUpdated();

            if (updated) // should also force if high contrast changed
            {
                CategorizedNamedColoursChanged?.Invoke(this, new CategorizedNamedColoursChangedArgs
                {
                    CategorizedNamedColours = newListCategoryNamedColours,
                    ThemeIsHighContrast = themeIsHighContrast
                });
            }
        }
        
        private bool ThemeIsHighContrast(Guid themeId)
        {
            return themeId == KnownColorThemes.HighContrast;
        }

        public List<CategorizedNamedColours> GetCategorizedNamedColoursList()
        {
            if (categorizedNamedColoursList == null)
            {
                GetCategorizedNamedColoursListWithUpdated();
            }
            return categorizedNamedColoursList;
        }
    }
}
