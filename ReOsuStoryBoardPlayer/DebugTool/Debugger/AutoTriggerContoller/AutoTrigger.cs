using ReOsuStoryBoardPlayer.BeatmapParser;
using ReOsuStoryBoardPlayer.Commands.Group.Trigger;
using ReOsuStoryBoardPlayer.Commands.Group.Trigger.TriggerCondition;
using ReOsuStoryBoardPlayer.Parser;
using ReOsuStoryBoardPlayer.Parser.Stream;
using ReOsuStoryBoardPlayer.Player;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ReOsuStoryBoardPlayer.Commands.Group.Trigger.TriggerCondition.HitSoundTriggerCondition;

namespace ReOsuStoryBoardPlayer.DebugTool.Debugger.AutoTriggerContoller
{
    //Part from osu! source code
    public class AutoTrigger : DebuggerBase
    {
        public LinkedList<HitSoundInfo> objects { get; private set; }
        LinkedListNode<HitSoundInfo> cur;

        double prev_time=0;

        public override void Init()
        {

        }

        public override void Term()
        {

        }

        public void Load(BeatmapFolderInfo info)
        {
            try
            {
                objects=HitSoundInfosHelpers.Parse(info.osu_file_path);
            }
            catch (Exception e)
            {
                Log.Warn(e.Message);
                objects=new LinkedList<HitSoundInfo>();
            }

            Flush();
        }

        private void Flush()
        {
            cur=objects.First;
            prev_time = 0;
        }

        public void Trim()
        {
            var unused_hitsounds = objects.AsParallel().Where(x => !TriggerListener.DefaultListener.CheckTrig(x)).ToList();

            foreach (var sounds in unused_hitsounds)
                objects.Remove(sounds);

            Log.Debug($"Remove {unused_hitsounds.Count()} hitsounds");
            Flush();
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
                    TriggerListener.DefaultListener.Trig(hit_object, (float)hit_object.Time);
                else
                    break;

                cur=cur.Next;
            }
        }
    }
}
