using ReOsuStoryBoardPlayer.Base;
using ReOsuStoryBoardPlayer.BeatmapParser;
using ReOsuStoryBoardPlayer.Commands.Group.Trigger.TriggerCondition;
using StorybrewCommon.Mapset;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ReOsuStoryBoardPlayer.Commands.Group.Trigger.TriggerCondition.HitSoundTriggerCondition;

namespace ReOsuStoryBoardPlayer.BeatmapParser
{
    public static class HitSoundInfosHelpers
    {
        public static LinkedList<HitSoundInfo> Parse(string path)
        {
            var objs=EditorBeatmap.Load(path).HitObjects;
            LinkedList<HitSoundInfo> infos = new LinkedList<HitSoundInfo>();

            foreach (var obj in objs.OrderBy(o=>o.StartTime))
            {
                switch (obj)
                {
                    case OsuCircle circle:
                        infos.AddLast(new HitSoundInfo() {
                            SampleSet=(SampleSetType)circle.SampleSet,
                            SampleSetAdditions=(SampleSetType)circle.AdditionsSampleSet,
                            CustomSampleSet=(CustomSampleSetType)circle.CustomSampleSet,
                            SoundType=(HitObjectSoundType)circle.Additions,
                            Time=circle.StartTime
                        });
                        break;
                    case OsuSlider slider:
                        foreach (var node in slider.Nodes)
                            infos.AddLast(new HitSoundInfo() {
                                SampleSet=(SampleSetType)node.SampleSet,
                                SampleSetAdditions=(SampleSetType)node.AdditionsSampleSet,
                                CustomSampleSet=(CustomSampleSetType)node.CustomSampleSet,
                                SoundType=(HitObjectSoundType)node.Additions,
                                Time=node.Time
                            });
                        break;
                    case OsuSpinner spinner:
                        infos.AddLast(new HitSoundInfo()
                        {
                            SampleSet=(SampleSetType)spinner.SampleSet,
                            SampleSetAdditions=(SampleSetType)spinner.AdditionsSampleSet,
                            CustomSampleSet=(CustomSampleSetType)spinner.CustomSampleSet,
                            SoundType=(HitObjectSoundType)spinner.Additions,
                            Time=spinner.StartTime
                        });
                        break;
                    default:
                        break;
                }
            }
            
            var itor = infos.First;

            while (itor!=null)
            {
                var info = itor.Value;

                if (info.SoundType==HitObjectSoundType.None)
                {
                    info.SoundType=HitObjectSoundType.Normal;
                    itor.Value=info;
                }

                itor=itor.Next;
            }

            return infos;
        }
    }
}
