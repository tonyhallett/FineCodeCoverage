﻿using FineCodeCoverage.Core.Utilities;
using FineCodeCoverage.Engine.ReportGenerator;
using FineCodeCoverage.Options;
using FineCodeCoverage.Output.HostObjects;
using System.Collections.Generic;

namespace FineCodeCoverage.Output
{
    internal class OutputToolWindowContext
    {
		public IEventAggregator EventAggregator { get; set; }
        public IReportColoursProvider ReportColoursProvider { get; set; }
        public List<IWebViewHostObjectRegistration> WebViewHostObjectRegistrations { get; set; }
        public IAppOptionsProvider AppOptionsProvider { get; internal set; }
    }
}
