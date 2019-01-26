using System;

namespace ReOsuStoryboardPlayer.Core.Base
{
    /// <summary>
    /// 表示用于触发TriggerListener绑定的触发器参数封装结构，塞了个时间方便Listener自寻触发对应的触发器
    /// 移植实现时可以通过这个方便使用触发器
    /// </summary>
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