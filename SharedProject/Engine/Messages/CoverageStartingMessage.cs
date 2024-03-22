using System;
using System.Collections.Generic;
using System.Text;
using FineCodeCoverage.Engine.Model;

namespace FineCodeCoverage.Engine.Messages
{
    internal class CoverageStartingMessage
    {
        public CoverageStartingMessage(bool pending = false) => this.Pending = pending;

        public bool Pending { get; }
    }

    internal enum CoverageEndedStatus
    {
        Success,
        Disabled,
        ConditionsNotMet,
        Cancelled,
        Faulted
    }
    internal class CoverageEndedMessage
    {
        public CoverageEndedMessage(CoverageEndedStatus status, List<ICoverageProject> coverageProjects)
        {
            this.Status = status;
            this.CoverageProjects = coverageProjects;
        }

        public CoverageEndedStatus Status { get; }
        public List<ICoverageProject> CoverageProjects { get; }
    }
}
