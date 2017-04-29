using System.Collections.Generic;

namespace BundtCommon.Extensions
{
    public static class DictionaryExtensions
    {
        public static bool DoesNotContainKey<TKey, TValue>(this Dictionary<TKey, TValue> @this, TKey key)
        {
            return @this.ContainsKey(key) == false;
        }
    }
}