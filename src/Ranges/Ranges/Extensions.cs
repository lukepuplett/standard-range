using System;

namespace Evoq.Ranges
{
    /// <summary>
    /// Contains extension methods for System.DateTimeOffset.
    /// </summary>
    public static class DateTimeOffsetExtensions
    {
        /// <summary>
        /// Checks if a the current System.DateTime falls within a time range.
        /// </summary>
        public static bool IsWithinRange(this DateTimeOffset instant, TimeRange timeRange)
        {
            return timeRange.IsEnveloping(instant);
        }
    }
}