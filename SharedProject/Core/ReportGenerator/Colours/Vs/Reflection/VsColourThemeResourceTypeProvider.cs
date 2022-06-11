using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Reflection;

namespace FineCodeCoverage.Core.ReportGenerator.Colours
{
    [Export(typeof(IVsColourThemeResourceTypeProvider))]
    public class VsColourThemeResourceTypeProvider : IVsColourThemeResourceTypeProvider
    {
        private static FieldInfo GetCategoryField(Type t)
        {
            return t.GetField("Category", BindingFlags.Public | BindingFlags.Static);
        }
        public List<IVsColourThemeResourceType> Provide(Assembly assembly)
        {
            List<IVsColourThemeResourceType> colourThemeResourceTypes = new List<IVsColourThemeResourceType>();
            var possibleTypes = assembly.GetTypes().Where(t => t.IsSealed && t.IsAbstract);
            foreach (var t in possibleTypes)
            {
                var categoryField = GetCategoryField(t);
                if (categoryField != null && categoryField.FieldType == typeof(Guid))
                {
                    colourThemeResourceTypes.Add(new VsColourThemeResourceType(t, categoryField));
                }
            }

            return colourThemeResourceTypes;
        }
    }
}
