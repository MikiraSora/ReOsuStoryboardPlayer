using ReOsuStoryboardPlayer.Core.Base;
using ReOsuStoryboardPlayer.Core.Serialization.FileInfo;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace ReOsuStoryboardPlayer.Core.Serialization
{
    public static class StoryboardSerializationHelper
    {
        /*  .osbin format:
         *  -----------------
         *  string : "OSBIN" (case-sensitive)
         *  -----------------
         *  byte : Feature flags
         *  -----------------
         *  ---Compressed data (option by Feature flag)
         *  |    -----------------
         *  |    Statistics data (option by Feature flag)
         *  |    -----------------
         *  |    String cache table
         *  |    -----------------
         *  |    Storyboard object/command data
         *  ---
         */

        public static void Serialize(Feature feature,IEnumerable<StoryboardObject> objects, Stream stream)
        {
            var count = objects.Count();
            StringCacheTable string_cache_table = new StringCacheTable();

            var osbin_writer = new BinaryWriter(stream);

            #region Build .osbin format
            
            //signture
            osbin_writer.Write("OSBIN");

            //feature flag
            osbin_writer.Write((byte)feature);

            //normal stream switch to gzip compression stream if feature IsCompression was set
            if (feature.HasFlag(Feature.IsCompression))
                osbin_writer=new BinaryWriter(new GZipStream(stream,CompressionMode.Compress));

            //storyboard object/command data
            MemoryStream temp_stream = new MemoryStream();

            BinaryWriter writer = new BinaryWriter(temp_stream);
            count.OnSerialize(writer);

            foreach (var obj in objects)
            {
                StoryboardObjectDeserializationFactory.GetObjectTypeId(obj).OnSerialize(writer);
                obj.OnSerialize(writer, string_cache_table);
            }

            //statistics data
            
            //string cache tables
            string_cache_table.OnSerialize(osbin_writer, null);

            temp_stream.Seek(0, SeekOrigin.Begin);
            temp_stream.CopyTo(stream);

            #endregion
        }

        public static IEnumerable<StoryboardObject> Deserialize(Stream stream)
        {
            BinaryReader reader = new BinaryReader(stream);

            var signture = reader.ReadString();

            //signture
            if (signture!="OSBIN")
                throw new InvalidFormatOsbinFormatException();

            //feature
            Feature feature = (Feature)reader.ReadByte();

            //convert to gzip stream if IsCompression was set
            if (feature.HasFlag(Feature.IsCompression))
                reader=new BinaryReader(new GZipStream(stream,CompressionMode.Decompress));

            //statistics data

            //string cache table
            StringCacheTable cache_table = new StringCacheTable();
            cache_table.OnDeserialize(reader, null);
            
            //storyboard object/command data
            var count = reader.ReadInt32();
            for (int i = 0; i<count; i++)
            {
                var obj = StoryboardObjectDeserializationFactory.Create(reader, cache_table);
                yield return obj;
            }
        }
    }
}
