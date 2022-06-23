﻿using System.Diagnostics;
using System.Windows;

namespace FineCodeCoverageWebViewReport
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            var mainWindow = new MainWindow(e.Args);
            mainWindow.Show();
        }
    }
}
