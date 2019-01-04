using ReOsuStoryBoardPlayer.Commands.Group.Trigger.TriggerCondition;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReOsuStoryBoardPlayer.Parser.SimpleOsuParser
{
    //只有Time和其他Trigger必要的属性
    public struct HitObject
    {
        /// <summary>
        /// [2]
        /// </summary>
        public int Time;

        public HitObjectType Type;

        /// <summary>
        /// [4]
        /// </summary>
        public HitObjectSoundType HitSoundType;


        public SampleSetType SampleSet;
        public SampleSetType AdditionSampleSet;
        public CustomSampleSetType CustomSampleSet;
    }
}
