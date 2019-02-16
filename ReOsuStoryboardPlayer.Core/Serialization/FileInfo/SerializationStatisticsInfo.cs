using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace ReOsuStoryboardPlayer.Core.Serialization
{
    public class SerializationStatisticsInfo : IStoryboardSerializable, IEnumerable<KeyValuePair<Type, int>>
    {
        private Dictionary<Type, int> statistics = new Dictionary<Type, int>();

        public int CachedTypeCount => statistics.Count;

        public int GetSerializedObjectCount<T>() => statistics.TryGetValue(typeof(T), out var count) ? count : 0;

        public void RegistedSerializedObject<T>(T obj) where T : class, ICloneable, new()
        {
            var count = GetSerializedObjectCount<T>();

            statistics[typeof(T)]=++count;
        }

        public void OnDeserialize(BinaryReader stream, StringCacheTable _)
        {
            int count = stream.ReadInt32();

            for (int i = 0; i<count; i++)
            {
                var fullname = stream.ReadString();
                var c = stream.ReadInt32();

                var type = Type.GetType(fullname);

                statistics[type]=c;
            }
        }

        public void OnSerialize(BinaryWriter stream, StringCacheTable _)
        {
            stream.Write(statistics.Count);

            foreach (var pair in statistics)
            {
                stream.Write(pair.Key.FullName);
                stream.Write(pair.Value);
            }
        }

        public IEnumerator<KeyValuePair<Type, int>> GetEnumerator() => GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => statistics.GetEnumerator();
    }
}