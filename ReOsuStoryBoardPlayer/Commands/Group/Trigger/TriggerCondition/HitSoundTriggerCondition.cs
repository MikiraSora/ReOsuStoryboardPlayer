using System;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;

namespace ReOsuStoryBoardPlayer.Commands.Group.Trigger.TriggerCondition
{
    public class HitSoundTriggerCondition : TriggerConditionBase
    {
        public HitObjectSoundType HitSound = HitObjectSoundType.Whistle|HitObjectSoundType.Normal|HitObjectSoundType.Clap|HitObjectSoundType.Finish;
        public SampleSetType SampleSet = SampleSetType.All;
        public SampleSetType SampleSetAdditions = SampleSetType.All;
        public CustomSampleSetType CustomSampleSet = CustomSampleSetType.Default;

        public HitSoundTriggerCondition(string description)
        {
            /* MAGIC > <
             *
             * HitSound -> HitSound(All)(All)(Whistle|Normal|Clap|Finish)(Default)
             * HitSoundNormalWhistle -> HitSound(All)(Normal)Whistle(Default)
             * HitSoundWhistle6 -> HitSound(All)(All)Whistle(CustomSampleSet 6)
             *
             */

            var fix_trim_expr = description.Replace("HitSound", string.Empty);
            //why I wrote these sh[]t?
            Parse(Parse(Parse(Parse(fix_trim_expr, ref SampleSet),
                        ref SampleSetAdditions),
                    ref HitSound),
                ref CustomSampleSet);

            if (HitSound!=HitObjectSoundType.None
                &&fix_trim_expr.StartsWith(SampleSet.ToString())
                &&!fix_trim_expr.Substring(SampleSet.ToString().Length).Contains(SampleSetAdditions.ToString()))
            {
                SampleSetAdditions=SampleSet;
                SampleSet=SampleSetType.All;
            }

            //assert check.
            Debug.Assert(HitSound!=HitObjectSoundType.None&&(String.IsNullOrWhiteSpace(fix_trim_expr)||((HitObjectSoundType[])Enum.GetValues(typeof(HitObjectSoundType)))
                .Where(x => HitSound.HasFlag(x))
                .Any(x => description.Contains(x.ToString()))), "parse HitSoundTriggerCondition::HitSound wrong!");
            Debug.Assert(SampleSet==SampleSetType.All||SampleSet!=SampleSetType.None||description.Contains(SampleSet.ToString()), "parse HitSoundTriggerCondition::SampleSet wrong!");
            Debug.Assert(SampleSetAdditions==SampleSetType.All||SampleSetAdditions!=SampleSetType.None||description.Contains(SampleSetAdditions.ToString()), "parse HitSoundTriggerCondition::SampleSetAdditions wrong!");
            Debug.Assert(CustomSampleSet==CustomSampleSetType.Default||description.Contains(CustomSampleSet.ToString())||description.Contains(((int)CustomSampleSet).ToString()), "parse HitSoundTriggerCondition::CustomSampleSet wrong!");
        }

        public string Parse<T>(string str, ref T v) where T : Enum
        {
            Debug.Assert(typeof(T).IsEnum, $"Dont use Parse() for non-enum type parsing.");

            if (string.IsNullOrWhiteSpace(str))
                return str;

            string int_val = Regex.Match(str, @"\d*").Groups[0].Value;

            int iv = 0;
            bool is_value = (!string.IsNullOrWhiteSpace(int_val))&&int.TryParse(int_val, out iv);

            var t = ((T[])Enum.GetValues(typeof(T))).FirstOrDefault(x => is_value ? (iv==Convert.ToInt32(x)) : str.StartsWith(x.ToString()));

            var match = !t.Equals(default(T));

            v=match ? t : v;

            return str.Substring(match ? (is_value ? iv.ToString().Length : v.ToString().Length) : 0);
        }

        public override string ToString() => $"HitSound ({SampleSet}:{SampleSetAdditions}) {HitSound} {CustomSampleSet}";

        public bool CheckCondition(HitSoundInfo hitSoundInfo)
        {
            if (SampleSet!=SampleSetType.All&&
                hitSoundInfo.SampleSet!=SampleSet&&SampleSet!=hitSoundInfo.SampleSetAdditions)
                return false;

            if (SampleSetAdditions!=SampleSetType.All&&hitSoundInfo.SampleSetAdditions!=SampleSetAdditions)
                return false;

            //storybrew可能塞了个HitSound为None的玩意
            if (hitSoundInfo.SoundType==HitObjectSoundType.None||!ContainFlagEnum(HitSound, hitSoundInfo.SoundType))
                return false;

            if (CustomSampleSet!=CustomSampleSetType.Default&&hitSoundInfo.CustomSampleSet!=CustomSampleSet)
                return false;

            return true;

            bool ContainFlagEnum<T>(T source, T compare_with) where T : Enum =>
                Enum.GetValues(typeof(T)).Cast<T>().Any(val => source.HasFlag(val)&&compare_with.HasFlag(val)&&Convert.ToInt32(val)!=0);
        }
    }
}