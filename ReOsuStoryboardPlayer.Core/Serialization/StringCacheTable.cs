using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ReOsuStoryboardPlayer.Core.Serialization
{
    public class StringCacheTable : IStoryboardSerializable
    {
        private Dictionary<string, uint> table = new Dictionary<string, uint>();

        public uint this[string str] => GetStringCacheId(str);

        public string this[uint id] => GetStringCache(id);

        private string GetStringCache(uint id)
        {
            return table.First(x => x.Value==id).Key;
        }

        private uint GetStringCacheId(string str)
        {
            if (table.TryGetValue(str, out var v))
                return v;
            return table[str]=(uint)table.Count+1;
        }

        public void OnDeserialize(BinaryReader osbin_writer, StringCacheTable _)
        {
            int map_count = osbin_writer.ReadInt32();

            for (int i = 0; i<map_count; i++)
            {
                var str = osbin_writer.ReadString();
                var id = osbin_writer.ReadUInt32();

                table.Add(str, id);
            }
        }

        public void OnSerialize(BinaryWriter osbin_writer, StringCacheTable _)
        {
            osbin_writer.Write(table.Count);

            foreach (var pair in table)
            {
                osbin_writer.Write(pair.Key);
                osbin_writer.Write(pair.Value);
            }
        }
    }
}