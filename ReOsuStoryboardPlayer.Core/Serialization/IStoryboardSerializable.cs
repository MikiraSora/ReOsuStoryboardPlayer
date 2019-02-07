using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ReOsuStoryboardPlayer.Core.Serialization
{   
    public interface IStoryboardSerializable
    {
        void OnSerialize(BinaryWriter stream,StringCacheTable table);
        void OnDeserialize(BinaryReader stream, StringCacheTable table);
    }
}
