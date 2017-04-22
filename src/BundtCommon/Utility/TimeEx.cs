using System;

namespace BundtCommon
{
    public static class TimeEx
    {
        public static TimeSpan _1second => TimeSpan.FromSeconds(1);
        public static TimeSpan _5seconds => TimeSpan.FromSeconds(5);
        public static TimeSpan _100ms => TimeSpan.FromMilliseconds(100);
    }
}