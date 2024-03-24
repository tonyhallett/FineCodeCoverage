﻿using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;

namespace FineCodeCoverage.Core.Utilities
{
    [Export(typeof(IFileRenameListener))]
    internal class FileRenameListener : IFileRenameListener, IVsTrackProjectDocumentsEvents2
    {
        private readonly List<Action<string, string>> callbacks = new List<Action<string, string>>();
        private readonly System.IServiceProvider serviceProvider;
        private bool listening;

        [ImportingConstructor]
        public FileRenameListener(
            [Import(typeof(SVsServiceProvider))] System.IServiceProvider serviceProvider
        ) => this.serviceProvider = serviceProvider;

        private void EnsureListening()
        {
            if (!this.listening)
            {
#pragma warning disable VSTHRD102 // Implement internal logic asynchronously
                ThreadHelper.JoinableTaskFactory.Run(async () =>
                {
                    await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                    var trackProjectDocuments = this.serviceProvider.GetService(typeof(SVsTrackProjectDocuments)) as IVsTrackProjectDocuments2;
                    _ = trackProjectDocuments.AdviseTrackProjectDocumentsEvents(this, out uint cookie);
                });
#pragma warning restore VSTHRD102 // Implement internal logic asynchronously
                this.listening = true;
            }
        }

        public void ListenForFileRename(Action<string, string> callback)
        {
            this.callbacks.Add(callback);
            this.EnsureListening();
        }

        public int OnQueryAddFiles(IVsProject pProject, int cFiles, string[] rgpszMkDocuments, VSQUERYADDFILEFLAGS[] rgFlags, VSQUERYADDFILERESULTS[] pSummaryResult, VSQUERYADDFILERESULTS[] rgResults) 
            => VSConstants.S_OK;

        public int OnAfterAddFilesEx(int cProjects, int cFiles, IVsProject[] rgpProjects, int[] rgFirstIndices, string[] rgpszMkDocuments, VSADDFILEFLAGS[] rgFlags) 
            => VSConstants.S_OK;

        public int OnAfterAddDirectoriesEx(int cProjects, int cDirectories, IVsProject[] rgpProjects, int[] rgFirstIndices, string[] rgpszMkDocuments, VSADDDIRECTORYFLAGS[] rgFlags) 
            => VSConstants.S_OK;

        public int OnAfterRemoveFiles(int cProjects, int cFiles, IVsProject[] rgpProjects, int[] rgFirstIndices, string[] rgpszMkDocuments, VSREMOVEFILEFLAGS[] rgFlags) 
            => VSConstants.S_OK;

        public int OnAfterRemoveDirectories(int cProjects, int cDirectories, IVsProject[] rgpProjects, int[] rgFirstIndices, string[] rgpszMkDocuments, VSREMOVEDIRECTORYFLAGS[] rgFlags) 
            => VSConstants.S_OK;

        public int OnQueryRenameFiles(IVsProject pProject, int cFiles, string[] rgszMkOldNames, string[] rgszMkNewNames, VSQUERYRENAMEFILEFLAGS[] rgFlags, VSQUERYRENAMEFILERESULTS[] pSummaryResult, VSQUERYRENAMEFILERESULTS[] rgResults) 
            => VSConstants.S_OK;

        public int OnQueryRenameDirectories(IVsProject pProject, int cDirs, string[] rgszMkOldNames, string[] rgszMkNewNames, VSQUERYRENAMEDIRECTORYFLAGS[] rgFlags, VSQUERYRENAMEDIRECTORYRESULTS[] pSummaryResult, VSQUERYRENAMEDIRECTORYRESULTS[] rgResults) 
            => VSConstants.S_OK;

        public int OnAfterRenameDirectories(int cProjects, int cDirs, IVsProject[] rgpProjects, int[] rgFirstIndices, string[] rgszMkOldNames, string[] rgszMkNewNames, VSRENAMEDIRECTORYFLAGS[] rgFlags) 
            => VSConstants.S_OK;

        public int OnQueryAddDirectories(IVsProject pProject, int cDirectories, string[] rgpszMkDocuments, VSQUERYADDDIRECTORYFLAGS[] rgFlags, VSQUERYADDDIRECTORYRESULTS[] pSummaryResult, VSQUERYADDDIRECTORYRESULTS[] rgResults) 
            => VSConstants.S_OK;

        public int OnQueryRemoveFiles(IVsProject pProject, int cFiles, string[] rgpszMkDocuments, VSQUERYREMOVEFILEFLAGS[] rgFlags, VSQUERYREMOVEFILERESULTS[] pSummaryResult, VSQUERYREMOVEFILERESULTS[] rgResults) 
            => VSConstants.S_OK;

        public int OnQueryRemoveDirectories(IVsProject pProject, int cDirectories, string[] rgpszMkDocuments, VSQUERYREMOVEDIRECTORYFLAGS[] rgFlags, VSQUERYREMOVEDIRECTORYRESULTS[] pSummaryResult, VSQUERYREMOVEDIRECTORYRESULTS[] rgResults) 
            => VSConstants.S_OK;

        public int OnAfterSccStatusChanged(int cProjects, int cFiles, IVsProject[] rgpProjects, int[] rgFirstIndices, string[] rgpszMkDocuments, uint[] rgdwSccStatus) 
            => VSConstants.S_OK;

        public int OnAfterRenameFiles(int cProjects, int cFiles, IVsProject[] rgpProjects, int[] rgFirstIndices, string[] rgszMkOldNames, string[] rgszMkNewNames, VSRENAMEFILEFLAGS[] rgFlags)
        {
            for (int i = 0; i < cFiles; i++)
            {
                this.Callback(rgszMkOldNames[i], rgszMkNewNames[i]);
            }

            return VSConstants.S_OK;
        }

        private void Callback(string oldFilePath, string newFilePath) => this.callbacks.ForEach(callback => callback(oldFilePath, newFilePath));
    }
}
