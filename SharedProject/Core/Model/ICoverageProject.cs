﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml.Linq;
using FineCodeCoverage.Options;

namespace FineCodeCoverage.Engine.Model
{
    internal interface ICoverageProject
    {
        string FCCOutputFolder { get; }
        string CoverageOutputFile { get; }
        string CoverageOutputFolder { get; set; }
        string DefaultCoverageOutputFolder { get; }
        List<string> ExcludedReferencedProjects { get; }
        List<string> IncludedReferencedProjects { get; }
        bool Is64Bit { get; set; }
        string ProjectFile { get; set; }
        XElement ProjectFileXElement { get; }
        string ProjectName { get; set; }
        string ProjectOutputFolder { get; }
        string RunSettingsFile { get; set; }
        IAppOptions Settings { get; }
        string TestDllFile { get; set; }
        Guid Id { get; set; }
        bool IsDotNetFramework { get; }
        string TargetFramework { get; set; }

        bool IsDotNetSdkStyle();
        Task<CoverageProjectFileSynchronizationDetails> PrepareForCoverageAsync(System.Threading.CancellationToken cancellationToken, bool synchronizeBuildOuput = true);
    }
}