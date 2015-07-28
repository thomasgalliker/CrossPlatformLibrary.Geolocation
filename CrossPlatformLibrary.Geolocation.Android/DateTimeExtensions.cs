using System;

namespace CrossPlatformLibrary.Geolocation
{
    public static class DateTimeExtensions
    {
        /// <summary>
        /// Epoch is a UTC DateTime value from January 1, 1970.
        /// </summary>
        public static readonly DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        /// <summary>
        /// Returns a DateTimeOffset which is Epoch + time.
        /// </summary>
        /// <param name="time">Time value in milliseconds (starting on January 1, 1970).</param>
        public static DateTimeOffset GetTimestamp(long time)
        {
            return new DateTimeOffset(Epoch.AddMilliseconds(time));
        }

        public static TimeSpan GetTimeSpan(long time)
        {
            return new TimeSpan(TimeSpan.TicksPerMillisecond * time);
        }
    }
}