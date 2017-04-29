using System;

namespace BundtCommon
{
    public static class TimeEx
    {
        public static TimeSpan _1second => TimeSpan.FromSeconds(1);
        public static TimeSpan _2seconds => TimeSpan.FromSeconds(2);
        public static TimeSpan _3seconds => TimeSpan.FromSeconds(3);
        public static TimeSpan _4seconds => TimeSpan.FromSeconds(4);
        public static TimeSpan _5seconds => TimeSpan.FromSeconds(5);
        public static TimeSpan _10seconds => TimeSpan.FromSeconds(10);
        public static TimeSpan _15seconds => TimeSpan.FromSeconds(15);
        public static TimeSpan _100ms => TimeSpan.FromMilliseconds(100);
    }
}