using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ReOsuStoryBoardPlayer.Commands.Group.Trigger.TriggerCondition
{
    public class HitSoundTriggerCondition : TriggerConditionBase
    {
        public HitObjectSoundType HitSound;
        //default All ,for example "HitSound" and accept all hitsoundinfos.
        public SampleSetType SampleSet = SampleSetType.All;
        public SampleSetType SampleSetAdditions = SampleSetType.All;
        public CustomSampleSetType CustomSampleSet;

        internal HitSoundTriggerCondition(string description)
        {
            //why I wrote these sh[]t?
            Parse(Parse(Parse(Parse(description.Replace("HitSound",string.Empty), ref SampleSet),
                        ref SampleSetAdditions),
                    ref HitSound),
                ref CustomSampleSet);

            //todo:Assert check.
        }

        public string Parse<T>(string str, ref T v) where T: Enum
        {
            Debug.Assert(typeof(T).IsEnum, $"Dont use Parse() for non-enum type parsing.");
            
            if (string.IsNullOrWhiteSpace(str))
                return str;

            string int_val = Regex.Match(str, @"\d*").Groups[0].Value;

            int iv=0;
            bool is_value =(!string.IsNullOrWhiteSpace(int_val))&&int.TryParse(int_val,out iv);

            var t = ((T[])Enum.GetValues(typeof(T))).FirstOrDefault(x => is_value? (iv==Convert.ToInt32(x)) : str.StartsWith(x.ToString()));

            var match=!t.Equals(default(T));

            v= match ? t : v;

            return str.Substring(match ? ( is_value ?  iv.ToString().Length : v.ToString().Length ): 0);
        }

        public override string ToString() => $"HitSound {SampleSet} {SampleSetAdditions} {HitSound} {CustomSampleSet}";
        
        public bool CheckCondition(HitSoundInfo hitSoundInfo)
        {
            if (SampleSet!=SampleSetType.All&&
                hitSoundInfo.SampleSet!=SampleSet&&SampleSet!=hitSoundInfo.SampleSetAdditions)
                return false;

            if (SampleSetAdditions!=SampleSetType.All&&
                !(hitSoundInfo.SampleSetAdditions==SampleSetAdditions
                ||hitSoundInfo.SampleSet==SampleSetAdditions))
                return false;

            if (!hitSoundInfo.SoundType.HasFlag(HitSound))
                return false;

            if (CustomSampleSet!=CustomSampleSetType.Default&&hitSoundInfo.CustomSampleSet!=CustomSampleSet)
                return false;

            return true;
        }

        private bool CheckSampleSet(SampleSetType b)
        {
            if (b==SampleSetType.None || b==SampleSetType.All)
                return false;//默认通过(虽然理论上是不存在这些条件)

            if (this.SampleSet==SampleSetType.All||(this.SampleSet!=SampleSetType.All&&this.SampleSet==b))
                return true;

            if (this.SampleSetAdditions==SampleSetType.All||(this.SampleSetAdditions!=SampleSetType.All&&this.SampleSetAdditions==b))
                return true;

            return false;
        }

        public struct HitSoundInfo:IComparable<HitSoundInfo>
        {
            public double Time;

            public HitObjectSoundType SoundType;
            public SampleSetType SampleSet;
            public SampleSetType SampleSetAdditions;
            public CustomSampleSetType CustomSampleSet;

            public HitSoundInfo(double Time,HitObjectSoundType SoundType, SampleSetType SampleSet, CustomSampleSetType CustomSampleSet, SampleSetType SampleSetAdditions = SampleSetType.None)
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

            public override string ToString() => $@"{(int)Time} {SampleSet} {SampleSetAdditions} {SoundType} {CustomSampleSet}";
        };
    }
}
