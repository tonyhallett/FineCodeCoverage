using System;

namespace FineCodeCoverage.Core.Utilities
{
    internal static class TimespanExtensions
    {
        public static string ToStringHoursMinutesSeconds(this TimeSpan timespan)
        {
            return timespan.ToString(@"hh\:mm\:ss");
        }
    }
}
