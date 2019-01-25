using ReOsuStoryboardPlayer.BeatmapParser;
using ReOsuStoryboardPlayer.Core.Base;
using ReOsuStoryboardPlayer.Core.Commands.Group.Trigger;
using ReOsuStoryboardPlayer.Core.Utils;
using ReOsuStoryboardPlayer.Parser;
using ReOsuStoryboardPlayer.Player;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ReOsuStoryboardPlayer.Tools.DefaultTools.AutoTriggerContoller
{
    //Part from osu! source code
    public class AutoTrigger : ToolBase
    {
        public LinkedList<HitSoundInfo> HitSoundInfos { get; private set; }
        private LinkedListNode<HitSoundInfo> cur;

        private double prev_time = 0;

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
                HitSoundInfos=HitSoundInfosHelpers.Parse(info.osu_file_path);
            }
            catch (Exception e)
            {
                Log.Warn(e.Message);
                HitSoundInfos=new LinkedList<HitSoundInfo>();
            }

            Flush();
        }

        private void Flush()
        {
            cur=HitSoundInfos.First;
            prev_time=0;
        }

        public void Trim()
        {
            var unused_hitsounds = HitSoundInfos.AsParallel().Where(x => !TriggerListener.DefaultListener.CheckTrig(x)).ToList();

            foreach (var sounds in unused_hitsounds)
                HitSoundInfos.Remove(sounds);

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