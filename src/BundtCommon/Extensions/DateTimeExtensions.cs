using System;

namespace BundtCommon.Extensions
{
    public static class DateTimeExtensions
    {
        public static long ToUnixTimestampSeconds(this DateTime @this)
        {
            return (int)Math.Floor((@this - new DateTime(1970, 1, 1)).TotalSeconds);
        }
    }
}