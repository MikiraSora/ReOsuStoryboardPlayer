﻿using ReOsuStoryBoardPlayer.Commands.Group.Trigger;
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
        LinkedList<HitSoundInfo> objects;
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
            objects = HitSoundInfosHelpers.Parse(info.osu_file_path);
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
                    TriggerListener.DefaultListener.Trig(hit_object, (float)hit_object.Time);
                else
                    break;

                cur=cur.Next;
            }
        }
    }
}
