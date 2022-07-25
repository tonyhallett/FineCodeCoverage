using System.IO;
using System.Windows;
using System.Windows.Media;

namespace FineCodeCoverageWebViewReport
{
    public class ChangingVsResourcesDictionary : ResourceDictionary
    {
        private const string VsFontEnvironmentFontFamilyResourceKey = "VsFont.EnvironmentFontFamily";
        private const string VsFontEnvironmentFontSizeResourceKey = "VsFont.EnvironmentFontSize";
        private const string VsBrushToolWindowBackgroundResourceKey = "VsBrush.ToolWindowBackground";
        private const string VsBrushToolWindowTextResourceKey = "VsBrush.ToolWindowText";

        public const string ChangeResourcesFileName = "changeresourcesfilename";

        private SolidColorBrush SolidColorBrushFromHex(string hex) =>
            new SolidColorBrush((Color)ColorConverter.ConvertFromString(hex));
        public ChangingVsResourcesDictionary(string[] arguments)
        {
            var namedArguments = NamedArguments.Get(arguments);
            if(namedArguments.TryGetValue(ChangeResourcesFileName, out var changeResourcesFileName))
            {
                var fileSystemWatcher = new FileSystemWatcher(Path.Combine(Path.GetTempPath()));
                fileSystemWatcher.Filter = changeResourcesFileName;
                fileSystemWatcher.Created += (sender, args) =>
                {
                    this.Change();
                };
                fileSystemWatcher.EnableRaisingEvents = true;
            }

            this.Add(VsFontEnvironmentFontFamilyResourceKey, new FontFamily(VsResourceValues.VsFontEnvironmentFontFamily));
            this.Add(VsFontEnvironmentFontSizeResourceKey, VsResourceValues.VsFontEnvironmentFontSize);
            this.Add(
                VsBrushToolWindowBackgroundResourceKey,
                SolidColorBrushFromHex(VsResourceValues.VsBrushToolWindowBackgroundColorHex)
            );
            this.Add(
                VsBrushToolWindowTextResourceKey,
                SolidColorBrushFromHex(VsResourceValues.VsBrushToolWindowTextColorHex)
            );
        }

        private void Change()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                this[VsFontEnvironmentFontFamilyResourceKey] = new FontFamily(VsResourceValues.ChangedVsFontEnvironmentFontFamily);
                this[VsFontEnvironmentFontSizeResourceKey] = VsResourceValues.ChangedVsFontEnvironmentFontSize;
                this[VsBrushToolWindowBackgroundResourceKey] = SolidColorBrushFromHex(VsResourceValues.ChangedVsBrushToolWindowBackgroundColorHex);
                this[VsBrushToolWindowTextResourceKey] = SolidColorBrushFromHex(VsResourceValues.ChangedVsBrushToolWindowTextColorHex);
            });
        }
    }
}
