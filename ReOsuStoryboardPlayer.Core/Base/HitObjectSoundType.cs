using System;

namespace ReOsuStoryboardPlayer.Core.Base
{
    /// <summary>
    /// 判断的时候None理论上默认为Normal
    /// </summary>
    [Flags]
    public enum HitObjectSoundType
    {
        None = 0,
        Normal = 1,
        Whistle = 2,
        Finish = 4,
        Clap = 8,

        All = Normal|Whistle|Finish|Clap
    };
}