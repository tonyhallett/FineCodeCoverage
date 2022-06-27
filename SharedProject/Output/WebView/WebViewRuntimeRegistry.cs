using Microsoft.Win32;
using System.ComponentModel.Composition;

namespace FineCodeCoverage.Output.WebView
{
    [Export(typeof(IWebViewRuntimeRegistry))]
    internal class WebViewRuntimeRegistry : IWebViewRuntimeRegistry
    {
        private class WebViewRuntimeRegistryEntries : IWebViewRuntimeRegistryEntries
        {
            public string Location { get; set; }

            public string Name { get; set; }

            public string SilentUninstallCommand { get; set; }

            public string Version { get; set; }
        }

        public IWebViewRuntimeRegistryEntries GetEntries()
        {
            const string stringKeyPerLocalMachine = "Software\\Wow6432Node\\Microsoft\\EdgeUpdate\\Clients\\{F3017226-FE2A-4295-8BDF-00C3A9A7E4C5}";
            const string stringKeyPerCurrentUser = "Software\\Microsoft\\EdgeUpdate\\Clients\\{F3017226-FE2A-4295-8BDF-00C3A9A7E4C5}";

            //When WebView2 is installed, it should be registered on one of two reg keys
            var registryKey = Registry.LocalMachine.OpenSubKey(stringKeyPerLocalMachine);
            if (registryKey is null)
            {
                registryKey = Registry.CurrentUser.OpenSubKey(stringKeyPerCurrentUser);
            }

            if (registryKey != null)
            {
                return new WebViewRuntimeRegistryEntries
                {
                    Version = this.GetValue(registryKey, "pv"),
                    Name = this.GetValue(registryKey, "name"),
                    Location = this.GetValue(registryKey, "location"),
                    SilentUninstallCommand = this.GetValue(registryKey, "SilentUninstall")
                };
            }

            return null;
        }

        private string GetValue(RegistryKey registryKey, string valueKey) =>
            !(registryKey.GetValue(valueKey) is string value) ? string.Empty : value;
    }



    
}
