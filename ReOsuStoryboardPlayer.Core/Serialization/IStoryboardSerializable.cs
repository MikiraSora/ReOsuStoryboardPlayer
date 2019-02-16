using System.IO;

namespace ReOsuStoryboardPlayer.Core.Serialization
{
    public interface IStoryboardSerializable
    {
        void OnSerialize(BinaryWriter stream, StringCacheTable table);

        void OnDeserialize(BinaryReader stream, StringCacheTable table);
    }
}