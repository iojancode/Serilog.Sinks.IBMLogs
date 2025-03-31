using System;

namespace Serilog.Sinks.IBMLogs
{
    public static class DateTimeOffsetExtensions
    {
        private static readonly DateTimeOffset UnixEpoch = new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero);

        public static long ToUnixTimeNanoseconds(this DateTimeOffset dateTimeOffset)
        {
            // Get the number of ticks (1 tick = 100 nanoseconds)
            long ticksSinceEpoch = (dateTimeOffset - UnixEpoch).Ticks;

            // Convert ticks to nanoseconds (1 tick = 100 nanoseconds, so multiply by 10)
            return ticksSinceEpoch * 10;
        }
    }
}