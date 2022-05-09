using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Serialization;

namespace FineCodeCoverage.Engine.ReportGenerator
{
    public class ThemeResourceKeyProperty
    {
        public ThemeResourceKeyProperty(PropertyInfo propertyInfo)
        {
            PropertyName = propertyInfo.Name;
            ThemeResourceKey = propertyInfo.GetValue(null) as ThemeResourceKey;
            IsColor = ThemeResourceKey.KeyType == ThemeResourceKeyType.ForegroundColor || ThemeResourceKey.KeyType == ThemeResourceKeyType.BackgroundColor;
            Display = $"{ThemeResourceKey.Name} {ThemeResourceKey.KeyType}";
        }
        public string PropertyName { get; set; }
        public ThemeResourceKey ThemeResourceKey { get; set; }
        public bool IsColor { get; set; }
        public string Display { get; set; }
    }
    
    public class ColorThemeResourceType
    {
        public string Name { get; }
        public Guid Category { get; }
        public List<ThemeResourceKey> ThemeResourceKeys { get; } = new List<ThemeResourceKey>();
        public List<ThemeResourceKeyProperty> ThemeResourceKeyProperties { get; } = new List<ThemeResourceKeyProperty>();

        public ColorThemeResourceType(Type type, FieldInfo categoryField)
        {
            Name = type.Name;
            Category = (Guid)categoryField.GetValue(null);
            var themeResourceKeyPropertyInfos = type.GetProperties(BindingFlags.Public | BindingFlags.Static).Where(p => p.PropertyType == typeof(Microsoft.VisualStudio.Shell.ThemeResourceKey));
            foreach(var themeResourceKeyPropertyInfo in themeResourceKeyPropertyInfos)
            {
                var themeResourceKeyProperty = new ThemeResourceKeyProperty(themeResourceKeyPropertyInfo);
                if (themeResourceKeyProperty.IsColor)
                {
                    var duplicate = ThemeResourceKeyProperties.SingleOrDefault(trkp => trkp.Display == themeResourceKeyProperty.Display);
                    if (duplicate != null)
                    {
                        var duplicateName = duplicate.PropertyName;
                        var duplicateName2 = themeResourceKeyPropertyInfo.Name;

                        var colorName = duplicate.ThemeResourceKey.Name;
                        var IsForeground = duplicate.ThemeResourceKey.KeyType == ThemeResourceKeyType.ForegroundColor;
                        
                    }
                    ThemeResourceKeys.Add(themeResourceKeyProperty.ThemeResourceKey);
                    ThemeResourceKeyProperties.Add(themeResourceKeyProperty);
                }
            }
        }

    }

    public static class ColorThemeResourceTypeProvider
    {
        private static FieldInfo GetCategoryField(Type t)
        {
            return t.GetField("Category", BindingFlags.Public | BindingFlags.Static);
        }
        public static List<ColorThemeResourceType> Provide()
        {
            List<ColorThemeResourceType> colorThemeResourceTypes = new List<ColorThemeResourceType>();
            var possibleTypes = typeof(EnvironmentColors).Assembly.GetTypes().Where(t => t.IsSealed && t.IsAbstract);
            foreach(var t in possibleTypes)
            {
                var categoryField = GetCategoryField(t);
                if (categoryField != null && categoryField.FieldType == typeof(Guid))
                {
                    colorThemeResourceTypes.Add(new ColorThemeResourceType(t, categoryField));
                }
            }

            return colorThemeResourceTypes;
        }
    }

    public class ColourName
    {
        public string Name { get; set; }
        private string jsName;
        public string JsName {
            get
            {
                if(jsName == null)
                {
                    return Name + (IsForeground ? "Fg" : "Bg");
                }
                return jsName;
            }
            set
            {
                jsName = value;
            }
        } 
        public bool IsForeground { get; set; }
        public Color Color { get; set; }

        public ColourName Clone(bool includeColor = false)
        {
            var colorName = new ColourName
            {
                IsForeground = IsForeground,
                JsName = JsName,
                Name = Name,
            };
            if (includeColor)
            {
                colorName.Color = Color;
            }
            return colorName;
        }
    }

    public class CategoryColours
    {
        public List<string> Types { get; set; }
        public List<CategoryColour> Colours { get;set; }
    }

    public class CategoryColour
    {
        public string Name { get; set; }
        public Guid Guid { get; set; }
        public List<ColourName> ColourNames { get; set; }

        public CategoryColour Clone()
        {
            CategoryColour categoryColour = new CategoryColour
            {
                Name = this.Name,
                Guid = this.Guid,
                ColourNames = this.ColourNames.Select(cn => cn.Clone()).ToList()
            };
            return categoryColour;
        }
    }

    internal interface ICategoryColoursProvider
    {
        List<CategoryColour> Provide();
    }

    [Export(typeof(ICategoryColoursProvider))]
    internal class CategoryColourProvider : ICategoryColoursProvider
    {
        private XmlSerializer xmlSerializer = new XmlSerializer(typeof(CategoryColours));
        private Dictionary<string, CategoryColour> themeResourceTypeLookup;
        private Dictionary<string, CategoryColour> ThemeResourceLookup
        {
            get
            {
                if (themeResourceTypeLookup == null)
                {
                    var colorThemeResourceTypes = ColorThemeResourceTypeProvider.Provide();
                    themeResourceTypeLookup = colorThemeResourceTypes.ToDictionary(colorThemeResourceType => colorThemeResourceType.Name, colorThemeResourceType =>
                    {
                        return new CategoryColour
                        {
                            Name = colorThemeResourceType.Name,
                            Guid = colorThemeResourceType.Category,
                            ColourNames = colorThemeResourceType.ThemeResourceKeyProperties.Select(themeResourceKeyProperty =>
                            {
                                var themeResourceKey = themeResourceKeyProperty.ThemeResourceKey;
                                var propertyName = themeResourceKeyProperty.PropertyName;
                                var jsName = propertyName.Replace("ColorKey", "");
                                return new ColourName
                                {
                                    Name = themeResourceKey.Name,
                                    JsName = jsName,
                                    IsForeground = themeResourceKey.KeyType == ThemeResourceKeyType.ForegroundColor
                                };
                            }).ToList()
                        };
                    });
                }
                return themeResourceTypeLookup;
            }
        }
        public List<CategoryColour> Provide()
        {
            CategoryColours categoryColours = null;
            
            var isCustom = false;
            if (isCustom)
            {
                // todo - default from types
                var categoryXmlPath = ""; // this will be hardcoded for now
                categoryColours = Deserialize(categoryXmlPath);
            }
            else
            {
                categoryColours = new CategoryColours
                {
                    Types = new List<string>
                    {
                        "EnvironmentColors",
                        "CommonControlsColors"
                    }
                };
            }
            //try/catch ?
            
            return AddFromTypes(categoryColours.Colours, categoryColours.Types);
        }

        internal CategoryColours Deserialize(string xmlPath)
        {
            var stream = new FileStream(xmlPath, FileMode.Open);
            return xmlSerializer.Deserialize(stream) as CategoryColours;
        }

        private List<CategoryColour> AddFromTypes(List<CategoryColour> categoryColours, List<string> types)
        {
            categoryColours = categoryColours ?? new List<CategoryColour>();
            foreach(var type in types)
            {
                if (ThemeResourceLookup.TryGetValue(type, out var categoryColour))
                {
                    categoryColours.Add(categoryColour.Clone());
                }
                else
                {
                    // throw ?
                }
            }
            return categoryColours;
        }
    }


    [Export(typeof(IReportColoursProvider))]
    internal class ReportColoursProvider : IReportColoursProvider
    {
        private readonly ICategoryColoursProvider categoryColoursProvider;

        public event EventHandler<List<CategoryColour>> ColoursChanged;

        private List<CategoryColour> lastCategoryColours;

        [ImportingConstructor]
        public ReportColoursProvider(
            ICategoryColoursProvider categoryColoursProvider
        )
        {
            this.categoryColoursProvider = categoryColoursProvider;
            VSColorTheme.ThemeChanged += VSColorTheme_ThemeChanged;
        }

        private List<CategoryColour> GetCategoryColours()
        {
            var categoryColours = categoryColoursProvider.Provide();
            foreach (var categoryColour in categoryColours)
            {
                foreach (var colourName in categoryColour.ColourNames)
                {
                    if (colourName.Color.IsEmpty)
                    {
                        colourName.Color = VSColorTheme.GetThemedColor(new ThemeResourceKey(categoryColour.Guid, colourName.Name, colourName.IsForeground ? ThemeResourceKeyType.ForegroundColor : ThemeResourceKeyType.BackgroundColor));
                    }
                }
            }
            return categoryColours;
        }

        private static bool CategoryColoursChanged(List<CategoryColour> oldCategoryColours, List<CategoryColour> newCategoryColours)
        {
            if (oldCategoryColours == null || oldCategoryColours.Count != newCategoryColours.Count)
            {
                return true;
            }
            
            foreach(var oldCategoryColour in oldCategoryColours)
            {
                var name = oldCategoryColour.Name;
                var newCategoryColour = newCategoryColours.FirstOrDefault(cc => cc.Name == name);
                if (newCategoryColour == null)
                {
                    return true;
                }
                if (newCategoryColour.Guid != oldCategoryColour.Guid)
                {
                    return true;
                }
                var oldColourNames = oldCategoryColour.ColourNames;
                var newColourNames = newCategoryColour.ColourNames;
                if (oldColourNames.Count != newColourNames.Count)
                {
                    return true;
                }
                foreach (var oldColourName in oldColourNames)
                {
                    var newColourName = newColourNames.FirstOrDefault(cn => cn.JsName == oldColourName.JsName);
                    if(newColourName == null)
                    {
                        return true;
                    }
                    if (!(newColourName.Name == oldColourName.Name && newColourName.IsForeground == oldColourName.IsForeground && newColourName.Color.ToString() == oldColourName.Color.ToString()))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private void VSColorTheme_ThemeChanged(ThemeChangedEventArgs e)
        {
            var newCategoryColours = GetCategoryColours();
            if (CategoryColoursChanged(lastCategoryColours, newCategoryColours))
            {
                ColoursChanged?.Invoke(this, newCategoryColours);
                lastCategoryColours = newCategoryColours;
            }
        }
        
        public List<CategoryColour> GetColours()
        {
            if (lastCategoryColours == null)
            {
                lastCategoryColours = GetCategoryColours();
            }
            return lastCategoryColours;
        }
    }
}
