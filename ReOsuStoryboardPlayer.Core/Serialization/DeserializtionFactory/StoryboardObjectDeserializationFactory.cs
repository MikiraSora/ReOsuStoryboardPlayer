using ReOsuStoryboardPlayer.Core.Base;
using System.IO;
using System.Linq;

namespace ReOsuStoryboardPlayer.Core.Serialization
{
    public static class StoryboardObjectDeserializationFactory
    {
        public static StoryboardObject Create(BinaryReader reader, StringCacheTable cache)
        {
            StoryboardObject obj = null;
            var id = reader.ReadByte();

            switch (id)
            {
                case 1:
                    obj=new StoryboardAnimation();
                    break;

                case 2:
                    obj=new StoryboardBackgroundObject();

                    //clean default commands because there will be added by binary file.
                    foreach (var cmd in obj.CommandMap.Values.SelectMany(x=>x).ToArray())
                        obj.RemoveCommand(cmd);

                    break;

                default:
                    obj=new StoryboardObject();
                    break;
            }

            obj.OnDeserialize(reader, cache);
            return obj;
        }

        public static byte GetObjectTypeId(StoryboardObject obj)
        {
            if (obj is StoryboardAnimation)
                return 1;
            if (obj is StoryboardBackgroundObject)
                return 2;
            return 0;
        }
    }
}