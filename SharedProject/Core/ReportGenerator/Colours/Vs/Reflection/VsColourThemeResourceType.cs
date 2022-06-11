using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace FineCodeCoverage.Core.ReportGenerator.Colours
{
    public class VsColourThemeResourceType : IVsColourThemeResourceType
    {
        public string Name { get; }
        public Guid Category { get; }
        public List<IThemeResourceKeyProperty> ColourThemeResourceKeyProperties { get; } = new List<IThemeResourceKeyProperty>();

        public VsColourThemeResourceType(Type type, FieldInfo categoryField)
        {
            Name = type.Name;
            Category = (Guid)categoryField.GetValue(null);

            AddColourThemeResourceKeys(type);
        }

        private IEnumerable<PropertyInfo> GetThemeResourceKeyProperties(Type type)
        {
            return type.GetProperties(BindingFlags.Public | BindingFlags.Static).Where(p => p.PropertyType == typeof(ThemeResourceKey));
        }

        private void AddColourThemeResourceKeys(Type type)
        {
            var themeResourceKeyPropertyInfos = GetThemeResourceKeyProperties(type);
            foreach (var themeResourceKeyPropertyInfo in themeResourceKeyPropertyInfos)
            {
                var themeResourceKeyProperty = new ThemeResourceKeyProperty(themeResourceKeyPropertyInfo);
                if (themeResourceKeyProperty.IsColour)
                {
                    ColourThemeResourceKeyProperties.Add(themeResourceKeyProperty);
                }
            }
        }
    }
}
