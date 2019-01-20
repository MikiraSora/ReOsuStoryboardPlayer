using System;

namespace ReOsuStoryBoardPlayer.Commands.Group.Trigger.TriggerCondition
{
    public struct HitSoundInfo : IComparable<HitSoundInfo>
    {
        //easy to debug
        public double Time;

        public HitObjectSoundType SoundType;
        public SampleSetType SampleSet;
        public SampleSetType SampleSetAdditions;
        public CustomSampleSetType CustomSampleSet;

        public HitSoundInfo(double Time, HitObjectSoundType SoundType, SampleSetType SampleSet, CustomSampleSetType CustomSampleSet, SampleSetType SampleSetAdditions = SampleSetType.None)
        {
            this.SoundType=SoundType;
            this.SampleSet=SampleSet;
            this.SampleSetAdditions=SampleSetAdditions;
            this.CustomSampleSet=CustomSampleSet;
            this.Time=Time;
        }

        public int CompareTo(HitSoundInfo other)
        {
            return Time.CompareTo(other.Time);
        }

        public override string ToString() => $@"{(int)Time} ({SampleSet}:{SampleSetAdditions}) {SoundType} {CustomSampleSet}";
    }
}