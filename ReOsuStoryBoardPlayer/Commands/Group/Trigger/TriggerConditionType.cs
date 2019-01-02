using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReOsuStoryBoardPlayer.Commands.Group.Trigger
{
    //from osu! EventTriggerHitSound.cs
    public class TriggerConditionType
    {
        [Flags]
        public enum HitObjectSoundType
        {
            None = 0,
            Normal = 1,
            Whistle = 2,
            Finish = 4,
            Clap = 8
        };

        public enum SampleSetType
        {
            All = -1,
            None = 0,
            Normal = 1,
            Soft = 2,
            Drum = 3
        }

        public enum CustomSampleSetType
        {
            Default = 0,
            Custom1 = 1,
            Custom2 = 2
        }

        public HitObjectSoundType SoundType { get; private set; }
        public SampleSetType SampleSet { get; private set; } = SampleSetType.All;
        public SampleSetType SampleSetAdditions { get; private set; } = SampleSetType.All;
        public CustomSampleSetType CustomSampleSet { get; private set; }
        public bool MainSampleSetDefined { get; private set; }
        public bool AdditionsSampleSetDefined { get; private set; }
        public bool CheckCustomSampleSet { get; private set; }

        private TriggerConditionType(string description)
        {
            string remainingDescription = description;

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
            if (!ToString().Equals("HitSound"+description))
                throw new Exception("Invalid hitsound trigger description after "+ToString());
        }

        private bool TryParseStartsWith<T>(string val, out T result)
        {
            result=default;

            if (string.IsNullOrWhiteSpace(val))
                throw new ArgumentNullException(nameof(val));

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

        private static Dictionary<string, TriggerConditionType> cache_triggers = new Dictionary<string, TriggerConditionType>();

        public static TriggerConditionType Parse(string description)
        {
            if (!cache_triggers.TryGetValue(description, out var trigger_condition))
                cache_triggers[description]=new TriggerConditionType(description);

            return cache_triggers[description];
        }

        public struct HitSoundInfo
        {
            public HitObjectSoundType SoundType;
            public SampleSetType SampleSet;
            public SampleSetType SampleSetAdditions;
            public CustomSampleSetType CustomSampleSet;
            public int Volume;

            public HitSoundInfo(HitObjectSoundType SoundType, SampleSetType SampleSet, CustomSampleSetType CustomSampleSet, int Volume, SampleSetType SampleSetAdditions = SampleSetType.None)
            {
                this.SoundType=SoundType;
                this.SampleSet=SampleSet;
                this.SampleSetAdditions=SampleSetAdditions;
                this.CustomSampleSet=CustomSampleSet;
                this.Volume=Volume;
            }

            public override string ToString()
            {
                return String.Format(@"{1} {2} {3} {4}% - {0}", SoundType, SampleSet, SampleSetAdditions, CustomSampleSet, Volume);
            }
        };
    }
}
