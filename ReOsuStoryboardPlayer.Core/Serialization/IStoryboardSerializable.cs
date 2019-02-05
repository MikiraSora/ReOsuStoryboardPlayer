using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ReOsuStoryboardPlayer.Core.Serialization
{   
    public interface IStoryboardSerializable
    {
        void OnSerialize(BinaryWriter stream,Dictionary<string,uint> map);
        void OnDeserialize(BinaryReader stream, Dictionary<uint, string> map);
    }
}
