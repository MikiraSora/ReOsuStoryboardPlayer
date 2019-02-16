using ReOsuStoryboardPlayer.Core.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace ReOsuStoryboardPlayer.Core.Serialization.FileInfo
{
    public class ObjectGenerator
    {
        private SerializationStatisticsInfo statistics;

        private Dictionary<Type, List<object>> object_caches;

        public ObjectGenerator(SerializationStatisticsInfo statisticsInfo)
        {
            statistics=statisticsInfo;

            BuildObjectCache();
        }

        private void BuildObjectCache()
        {
            object_caches=new Dictionary<Type, List<object>>(statistics.CachedTypeCount);

            foreach (var pair in statistics)
            {
                var list = new List<object>(pair.Value);

                var base_obj = Activator.CreateInstance(pair.Key) as ICloneable;
                Debug.Assert(base_obj!=null);

                list.Add(base_obj);

                for (int i = 1; i<pair.Value; i++)
                    list.Add(base_obj.Clone());

                object_caches[pair.Key]=list;
                Log.Debug($"create {pair.Value} {pair.Key.Name} object for cache");
            }
        }

        public T TakeObject<T>() where T : class, new()
        {
            if (object_caches.TryGetValue(typeof(T), out var list))
            {
                var obj = list.First();
                list.Remove(obj);

                return (T)obj;
            }

            Debug.Assert(false);

            return new T();
        }
    }
}