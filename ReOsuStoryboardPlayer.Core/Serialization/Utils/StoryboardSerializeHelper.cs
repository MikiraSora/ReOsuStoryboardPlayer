using ReOsuStoryboardPlayer.Core.Base;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace ReOsuStoryboardPlayer.Core.Serialization
{
    public static class StoryboardSerializationHelper
    {
        public static void Serialize(IEnumerable<StoryboardObject> objects, Stream stream)
        {
            var count = objects.Count();
            Dictionary<string, uint> map = new Dictionary<string, uint>();

            MemoryStream temp_stream = new MemoryStream();

            BinaryWriter writer = new BinaryWriter(temp_stream);
            count.OnSerialize(writer);

            foreach (var obj in objects)
            {
                StoryboardObjectDeserializationFactory.GetObjectTypeId(obj).OnSerialize(writer);
                obj.OnSerialize(writer, map);
            }

            var map_writer = new BinaryWriter(stream);

            //write map data
            #region Write Map Data

            map_writer.Write(map.Count);

            foreach (var pair in map)
            {
                map_writer.Write(pair.Key);
                map_writer.Write(pair.Value);
            }

            #endregion
            
            //write main data
            temp_stream.Seek(0, SeekOrigin.Begin);
            temp_stream.CopyTo(stream);
        }

        public static IEnumerable<StoryboardObject> Deserialize(Stream stream)
        {
            BinaryReader reader = new BinaryReader(stream);

            #region Read Map Data

            int map_count = reader.ReadInt32();

            var map = new Dictionary<uint, string>(map_count);

            for (int i = 0; i<map_count; i++)
            {
                var str = reader.ReadString();
                var id = reader.ReadUInt32();

                map.Add(id, str);
            }

            #endregion
            
            var count = reader.ReadInt32();

            for (int i = 0; i<count; i++)
            {
                var obj = StoryboardObjectDeserializationFactory.Create(reader,map);
                yield return obj;
            }
        }
    }
}
