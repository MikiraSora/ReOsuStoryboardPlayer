﻿namespace ReOsuStoryboardPlayer.Core.Parser
{
    public enum Section
    {
        Unknown = 0,
        General = 1,
        Colours = 2,
        Editor = 4,
        Metadata = 8,
        TimingPoints = 16,
        Events = 32,
        HitObjects = 64,
        Difficulty = 128,
        Variables = 256
    }
}