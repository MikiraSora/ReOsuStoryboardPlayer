using System;
using System.Collections.Generic;

namespace ReOsuStoryboardPlayer.Core.Serialization
{
    public static class DictionaryCacheExtenstion
    {
        public static uint GetStringCacheId(this IDictionary<string, uint> map, string str)
        {
            if (map.TryGetValue(str, out var v))
                return v;
            return map[str]=(uint)map.Count+1;
        }

        public static string GetStringCache(this IDictionary<uint, string> map, uint id)
        {
            if (!map.TryGetValue(id, out var v))
                throw new Exception(".osbin had been broken and not found string in cache table");
            return v;
        }
    }
}