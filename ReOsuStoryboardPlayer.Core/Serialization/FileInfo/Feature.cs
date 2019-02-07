using System;
using System.Collections.Generic;
using System.Text;

namespace ReOsuStoryboardPlayer.Core.Serialization.FileInfo
{
    [Flags]
    public enum Feature : byte
    {
        IsCompression=1,
        ContainStatistics=2
    }
}
