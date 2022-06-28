using System;
using System.Collections.Generic;
using FineCodeCoverage.Impl;

namespace FineCodeCoverage.Engine.MsTestPlatform.CodeCoverage
{
    internal static class MsCodeCoverageIOperationExtensions
    {
        public static IEnumerable<Uri> GetRunSettingsMsDataCollectorResultUri(this ITestOperation operation)
        {
            return operation.GetRunSettingsDataCollectorResultUri(new Uri(RunSettingsHelper.MsDataCollectorUri));
        }
    }
}
