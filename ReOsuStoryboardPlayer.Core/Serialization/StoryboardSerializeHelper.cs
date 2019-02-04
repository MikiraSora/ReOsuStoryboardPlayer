using ReOsuStoryboardPlayer.Core.Base;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ReOsuStoryboardPlayer.Core.Serialization
{
    public static class StoryboardSerializationHelper
    {
        public static void Serialize(IEnumerable<StoryboardObject> objects,Stream stream)
        {
            var count = objects.Count();

            using (BinaryWriter writer=new BinaryWriter(stream))
            {
                count.OnSerialize(writer);

                foreach (var obj in objects)
                {
                    StoryboardObjectDeserializationFactory.GetObjectTypeId(obj).OnSerialize(writer);
                    obj.OnSerialize(writer);
                }
            }
        }

        public static IEnumerable<StoryboardObject> Deserialize(Stream stream)
        {
            using (BinaryReader reader = new BinaryReader(stream))
            {
                var count = reader.ReadInt32();

                for (int i = 0; i<count; i++)
                {
                    var obj=StoryboardObjectDeserializationFactory.Create(reader);
                    yield return obj;
                }
            }
        }
    }
}
