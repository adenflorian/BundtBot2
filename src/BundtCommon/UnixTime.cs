using System;

namespace BundtCommon
{
    public static class UnixTime
    {
        public static int GetTimestamp()
        {
            return (int)Math.Floor((DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds);
        }
    }
}