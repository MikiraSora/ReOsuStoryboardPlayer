using ReOsuStoryBoardPlayer.Commands.Group.Trigger;
using ReOsuStoryBoardPlayer.Parser;
using ReOsuStoryBoardPlayer.Parser.SimpleOsuParser;
using ReOsuStoryBoardPlayer.Player;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ReOsuStoryBoardPlayer.Commands.Group.Trigger.TriggerCondition.HitSoundTriggerCondition;

namespace ReOsuStoryBoardPlayer.DebugTool.Debugger.AutoTriggerContoller
{
    public class AutoTrigger : DebuggerBase
    {
        BeatmapFolderInfo info = null;

        LinkedListNode<HitObject> cur;

        LinkedList<HitObject> objects;

        float prev_time = 0;

        public override void Init()
        {

        }

        public override void Term()
        {

        }

        public void Load(BeatmapFolderInfo info)
        {
            objects = new LinkedList<HitObject>(HitObjectParserHelper.ParseHitObjects(info.osu_file_path));
            Flush();
        }

        private void Flush()
        {
            cur=objects.First;
            prev_time = 0;
        }

        public override void Update()
        {
            var time = MusicPlayerManager.ActivityPlayer.CurrentTime;

            if (time<prev_time)
                Flush();

            prev_time=time;

            while (cur!=null)
            {
                var hit_object = cur.Value;

                if (time>=hit_object.Time)
                {
                    TriggerListener.DefaultListener.Trig(new HitSoundInfo() {
                        CustomSampleSet=hit_object.CustomSampleSet,
                        SampleSet=hit_object.SampleSet,
                        SampleSetAdditions=hit_object.AdditionSampleSet,
                        SoundType=hit_object.HitSoundType,
                        Volume=2857
                    }, hit_object.Time);
                }
                else
                    break;

                cur=cur.Next;
            }
        }
    }
}
