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

        public event EventHandler<List<CategorizedNamedColours>> CategorizedNamedColoursChanged;

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

        private void VSColourTheme_ThemeChanged(object sender, EventArgs args)
        {
            var (newListCategoryNamedColours, updated) = GetCategorizedNamedColoursListWithUpdated();
            if (updated)
            {
                CategorizedNamedColoursChanged?.Invoke(this, newListCategoryNamedColours);
            }
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
