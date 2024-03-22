using System;
using System.Collections.Generic;
using System.Text;

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
        public CoverageEndedMessage(CoverageEndedStatus status) => this.Status = status;

        public CoverageEndedStatus Status { get; }
    }
}
