﻿using System;
using System.IO;
using System.Linq;
using System.Reflection;
using FineCodeCoverage.Engine;
using Microsoft.VisualStudio.Shell;
using System.Runtime.InteropServices;
using System.ComponentModel.Design;
using Microsoft.VisualStudio.Imaging.Interop;

namespace FineCodeCoverage.Output
{
    /// <summary>
    /// This class implements the tool window exposed by this package and hosts a user control.
    /// </summary>
    /// <remarks>
    /// In Visual Studio tool windows are composed of a frame (implemented by the shell) and a pane,
    /// usually implemented by the package implementer.
    /// <para>
    /// This class derives from the ToolWindowPane class provided from the MPF in order to use its
    /// implementation of the IVsUIElementPane interface.
    /// </para>
    /// </remarks>
    [Guid("320fd13f-632f-4b16-9527-a1adfe555f6c")]
    internal class ReportToolWindow : ToolWindowPane
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReportToolWindow"/> class.
        /// </summary>
        public ReportToolWindow(ReportToolWindowContext context) : base(null) => this.Initialize(context);

        public ReportToolWindow() 
            => this.Initialize(FCCPackage.GetToolWindowContext<ReportToolWindow, ReportToolWindowContext>());

        private void Initialize(ReportToolWindowContext context)
        {
            if (context.ShowToolWindowToolbar())
            {
                this.ToolBar = new CommandID(PackageGuids.guidFCCPackageCmdSet, PackageIds.ToolWindowToolbar);
            }

            this.Caption = Vsix.Name;
            this.BitmapImageMoniker = new ImageMoniker { Guid = new Guid("c07b27ae-9756-4632-a6e6-1578f6dfbedf"), Id = 1 };
            // This is the user control hosted by the tool window; Note that, even if this class implements IDisposable,
            // we are not calling Dispose on this object. This is because ToolWindowPane calls Dispose on
            // the object returned by the Content property.

            try
            {
                AppDomain.CurrentDomain.AssemblyResolve += this.CurrentDomain_AssemblyResolve;
                this.Content = new ReportToolWindowControl(context.ReportViewModel);
            }
            finally
            {
                AppDomain.CurrentDomain.AssemblyResolve -= this.CurrentDomain_AssemblyResolve;
            }
        }

        private Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            var assemblyName = new AssemblyName(args.Name);

            try
            {
                AppDomain.CurrentDomain.AssemblyResolve -= this.CurrentDomain_AssemblyResolve;

                // try resolve by name

                try
                {
                    var assembly = Assembly.Load(assemblyName.Name);
                    if (assembly != null) return assembly;
                }
                catch
                {
                    // ignore
                }

                // try resolve by path

                try
                {
                    string dllName = $"{assemblyName.Name}.dll";
                    string projectDllPath = Path.GetDirectoryName(typeof(FCCEngine).Assembly.Location);
                    string dllPath = Directory.GetFiles(projectDllPath, "*.dll", SearchOption.AllDirectories).FirstOrDefault(x => Path.GetFileName(x).Equals(x.Equals(dllName, StringComparison.OrdinalIgnoreCase)));

                    if (!string.IsNullOrWhiteSpace(dllPath))
                    {
                        var assembly = Assembly.LoadFile(dllPath);
                        if (assembly != null) return assembly;
                    }
                }
                catch
                {
                    // ignore
                }
            }
            finally
            {
                AppDomain.CurrentDomain.AssemblyResolve += this.CurrentDomain_AssemblyResolve;
            }

            return null;
        }
    }
}
