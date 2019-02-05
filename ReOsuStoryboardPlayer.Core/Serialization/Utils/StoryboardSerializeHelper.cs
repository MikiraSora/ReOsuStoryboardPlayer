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
        private static IFormatter sysbuild_formatter = new BinaryFormatter();

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
                obj.OnSerialize(writer,map);
            }

            using (var map_writer=new BinaryWriter(stream))
            {
                //write map data
                sysbuild_formatter.Serialize(stream, map);

                //write main data
                temp_stream.Seek(0, SeekOrigin.Begin);
                temp_stream.CopyTo(stream);
            }
        }

        public static IEnumerable<StoryboardObject> Deserialize(Stream stream)
        {
            var map = (sysbuild_formatter.Deserialize(stream) as Dictionary<string, uint>).ToDictionary(x=>x.Value,x=>x.Key); 

            BinaryReader reader = new BinaryReader(stream);
            
            var count = reader.ReadInt32();

            for (int i = 0; i<count; i++)
            {
                var obj = StoryboardObjectDeserializationFactory.Create(reader,map);
                yield return obj;
            }
        }
    }
}
