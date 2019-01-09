using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReOsuStoryBoardPlayer.Commands.Group.Trigger.TriggerCondition
{
    public class HitSoundTriggerCondition:TriggerConditionBase
    {
        public HitObjectSoundType SoundType { get; private set; }
        public SampleSetType SampleSet { get; private set; } = SampleSetType.All;
        public SampleSetType SampleSetAdditions { get; private set; } = SampleSetType.All;
        public CustomSampleSetType CustomSampleSet { get; private set; }
        public bool MainSampleSetDefined { get; private set; }
        public bool AdditionsSampleSetDefined { get; private set; }
        public bool CheckCustomSampleSet { get; private set; }

        internal HitSoundTriggerCondition(string description)
        {
            string remainingDescription = description.Remove(0,"HitSound".Length);

            // Main and additions sample sets
            SampleSetType parsedSampleSet;
            if (TryParseStartsWith(remainingDescription, out parsedSampleSet)
                &&parsedSampleSet!=SampleSetType.None)
            {
                SampleSet=parsedSampleSet;
                MainSampleSetDefined=true;
                remainingDescription=remainingDescription.Substring(SampleSet.ToString().Length,
                    remainingDescription.Length-SampleSet.ToString().Length);

                // Additions

                if (TryParseStartsWith(remainingDescription, out parsedSampleSet)
                    &&parsedSampleSet!=SampleSetType.None)
                {
                    SampleSetAdditions=parsedSampleSet;
                    AdditionsSampleSetDefined=true;
                    remainingDescription=remainingDescription.Substring(SampleSetAdditions.ToString().Length,
                        remainingDescription.Length-SampleSetAdditions.ToString().Length);
                }
            }

            // Sound type
            HitObjectSoundType parsedSoundType;
            if (TryParseStartsWith(remainingDescription, out parsedSoundType)
                &&parsedSoundType!=HitObjectSoundType.None&&parsedSoundType!=HitObjectSoundType.Normal)
            {
                SoundType=parsedSoundType;
                remainingDescription=remainingDescription.Substring(SoundType.ToString().Length,
                    remainingDescription.Length-SoundType.ToString().Length);
            }

            // Custom sample set
            int parsedSampleSetIndex;
            if (Int32.TryParse(remainingDescription, out parsedSampleSetIndex))
            {
                CustomSampleSet=(CustomSampleSetType)parsedSampleSetIndex;
                CheckCustomSampleSet=true;
            }

            // Make trigger descriptions have more intuitive results:
            // - HitSoundDrumWhistle refers to the whistle addition being from the drum sampleset, 
            //   if you'd wanted a trigger on a drum sampleset + any whistle addition (uncommon), you'd use HitSoundDrumAllWhistle
            if (SoundType!=HitObjectSoundType.None&&MainSampleSetDefined&&!AdditionsSampleSetDefined)
            {
                SampleSetAdditions=SampleSet;
                SampleSet=SampleSetType.All;

                MainSampleSetDefined=false;
                AdditionsSampleSetDefined=true;
            }

            // Check that the description is valid
            if (!ToString().Equals(description))
                throw new Exception("Invalid hitsound trigger description after "+ToString());
        }

        public override string ToString()
        {
            string triggerName = "HitSound";
            if (MainSampleSetDefined)
                triggerName+=SampleSet;
            if (AdditionsSampleSetDefined)
                triggerName+=SampleSetAdditions;
            if (SoundType!=HitObjectSoundType.None)
                triggerName+=SoundType;
            if (CheckCustomSampleSet)
                triggerName+=((int)CustomSampleSet);
            return triggerName;
        }

        private bool TryParseStartsWith<T>(string val, out T result)
        {
            result=default;

            foreach (object v in Enum.GetValues(typeof(T)))
            {
                if (val.StartsWith(v.ToString()))
                {
                    result=(T)v;
                    return true;
                }
            }
            
            return false;
        }

        public bool CheckCondition(HitSoundInfo hitSoundInfo)
        {
            if (SampleSet!=SampleSetType.All&&hitSoundInfo.SampleSet!=SampleSet)
                return false;

            if (SampleSetAdditions!=SampleSetType.All&&
                !(hitSoundInfo.SampleSetAdditions==SampleSetAdditions
                ||hitSoundInfo.SampleSetAdditions==SampleSetType.None&&hitSoundInfo.SampleSet==SampleSetAdditions))
                return false;

            if (SoundType!=HitObjectSoundType.None&&!hitSoundInfo.SoundType.HasFlag(SoundType))
                return false;

            if (CheckCustomSampleSet&&hitSoundInfo.CustomSampleSet!=CustomSampleSet)
                return false;

            return true;
        }

        public struct HitSoundInfo:IComparable<HitSoundInfo>
        {
            public double Time;

            public HitObjectSoundType SoundType;
            public SampleSetType SampleSet;
            public SampleSetType SampleSetAdditions;
            public CustomSampleSetType CustomSampleSet;
            public int Volume;

            public HitSoundInfo(double Time,HitObjectSoundType SoundType, SampleSetType SampleSet, CustomSampleSetType CustomSampleSet, int Volume, SampleSetType SampleSetAdditions = SampleSetType.None)
            {
                this.SoundType=SoundType;
                this.SampleSet=SampleSet;
                this.SampleSetAdditions=SampleSetAdditions;
                this.CustomSampleSet=CustomSampleSet;
                this.Volume=Volume;
                this.Time=Time;
            }

            public int CompareTo(HitSoundInfo other)
            {
                return this.Time.CompareTo(other.Time);
            }

            public override string ToString() => $@"{(int)Time} {SampleSet} {SampleSetAdditions} {CustomSampleSet} {Volume}% - {SoundType}";
        };
    }
}
