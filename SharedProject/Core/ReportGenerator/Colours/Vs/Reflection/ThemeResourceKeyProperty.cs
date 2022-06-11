using Microsoft.VisualStudio.Shell;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace FineCodeCoverage.Core.ReportGenerator.Colours
{
    public class ThemeResourceKeyProperty : IThemeResourceKeyProperty
    {
        public ThemeResourceKeyProperty(PropertyInfo propertyInfo)
        {
            // required to differentiate as ThemeResourceKey.Name is not unique - differs by KeyType
            PropertyName = propertyInfo.Name;
            ThemeResourceKey = propertyInfo.GetValue(null) as ThemeResourceKey;
            IsColour = ThemeResourceKey.KeyType == ThemeResourceKeyType.ForegroundColor || ThemeResourceKey.KeyType == ThemeResourceKeyType.BackgroundColor;
        }
        public string PropertyName { get; set; }
        public bool IsColour { get; set; }
        public ThemeResourceKey ThemeResourceKey { get; set; }
    }
}
