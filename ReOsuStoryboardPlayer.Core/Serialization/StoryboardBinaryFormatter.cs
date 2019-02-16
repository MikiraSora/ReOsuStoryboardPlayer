using ReOsuStoryboardPlayer.Core.Base;
using ReOsuStoryboardPlayer.Core.Serialization.FileInfo;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;

namespace ReOsuStoryboardPlayer.Core.Serialization
{
    public static class StoryboardBinaryFormatter
    {
        private static readonly byte[] GZIPHEADER = Encoding.UTF8.GetBytes("OSBIN_GZIP");

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

        /// <summary>
        /// Serialize a list storyobjects as .osbin format to writable stream
        /// </summary>
        /// <param name="feature"></param>
        /// <param name="objects"></param>
        /// <param name="stream">output stream</param>
        public static void Serialize(Feature feature, IEnumerable<StoryboardObject> objects, Stream stream)
        {
            var count = objects.Count();
            StringCacheTable string_cache_table = new StringCacheTable();

            var osbin_writer = new BinaryWriter(stream);

            #region Build .osbin format

            //signture
            osbin_writer.Write("OSBIN");

            //feature flag
            osbin_writer.Write((byte)feature);

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

            temp_stream.Flush();
            temp_stream.Dispose();

            #endregion Build .osbin format
        }

        /// <summary>
        /// Deserialize .osbin format binary data from any readable stream
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static IEnumerable<StoryboardObject> Deserialize(Stream stream)
        {
            BinaryReader reader = new BinaryReader(stream);

            var signture = reader.ReadString();

            //signture
            if (signture!="OSBIN")
                throw new InvalidFormatOsbinFormatException();

            //feature
            Feature feature = (Feature)reader.ReadByte();

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

        public static void ZipSerialize(Feature feature, IEnumerable<StoryboardObject> objects, Stream stream)
        {
            stream.Write(GZIPHEADER, 0, GZIPHEADER.Length);

            using (var sbgzip = new MemoryStream())
            {
                using (var zip_stream = new GZipStream(sbgzip, CompressionMode.Compress))
                {
                    Serialize(feature, objects, zip_stream);
                }

                using (var sbgzip2 = new MemoryStream(sbgzip.GetBuffer()))
                    sbgzip2.CopyTo(stream);
            }
        }

        public static IEnumerable<StoryboardObject> UnzipDeserialize(Stream stream)
        {
            byte[] buffer = new byte[GZIPHEADER.Length];

            var read = stream.Read(buffer, 0, buffer.Length);

            if (read!=buffer.Length&&!Enumerable.Range(0, buffer.Length).All(i => buffer[i]==GZIPHEADER[i]))
                throw new InvalidFormatOsbinFormatException();

            var unzip_stream = new GZipStream(stream, CompressionMode.Decompress);

            return Deserialize(unzip_stream);
        }
    }
}