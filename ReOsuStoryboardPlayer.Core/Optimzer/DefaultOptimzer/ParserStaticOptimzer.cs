using ReOsuStoryboardPlayer.Core.Base;
using ReOsuStoryboardPlayer.Core.Commands;
using ReOsuStoryboardPlayer.Core.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ReOsuStoryboardPlayer.Core.Optimzer.DefaultOptimzer
{
    public class ParserStaticOptimzer:OptimzerBase
    {
        public override void Optimze(int level,IEnumerable<StoryboardObject> Storyboard_objects)
        {
            if (level>=3)
            {
                int effect_count = 0;
                using (StopwatchRun.Count(() => "RemoveUnusedCommand() optimze count:"+effect_count))
                    RemoveUnusedCommand(Storyboard_objects, ref effect_count);
            }

            if (level>=4)
            {
                int effect_count = 0;
                using (StopwatchRun.Count(() => "CombineCommands() optimze count:"+effect_count))
                    CombineCommands(Storyboard_objects, ref effect_count);
            }
        }

        public void CombineCommands(IEnumerable<StoryboardObject> Storyboard_objects, ref int effect_count)
        {
            foreach (var obj in Storyboard_objects)
            {
                foreach (var pair in obj.CommandMap)
                {
                    var real_timeline = pair.Value;

                    //立即求值
                    var normal_timeline = pair.Value.OfType<ValueCommand>().ToArray();

                    if (normal_timeline.Length==0)
                        continue;

                    //ValueCommand<TYPE_VALUE>我敲里吗
                    var type = normal_timeline.First().GetType();
                    var end_value_prop = type.GetProperty("EndValue");
                    var start_value_prop = type.GetProperty("StartValue");

                    for (int i = 0; i<normal_timeline.Count()-1; i++)
                    {
                        var cmd = normal_timeline[i];
                        var next_cmd = normal_timeline[i+1];

                        if ((cmd.Easing==next_cmd.Easing)
                            &&(cmd.EndTime==next_cmd.StartTime)
                            &&(end_value_prop.GetValue(cmd)==start_value_prop.GetValue(next_cmd)))
                        {
                            //combine
                            var new_cmd = (ValueCommand)type.Assembly.CreateInstance(type.FullName);

                            new_cmd.EndTime=next_cmd.EndTime;
                            new_cmd.StartTime=cmd.StartTime;
                            new_cmd.Easing=cmd.Easing;
                            end_value_prop.SetValue(new_cmd, end_value_prop.GetValue(next_cmd));
                            start_value_prop.SetValue(new_cmd, start_value_prop.GetValue(cmd));

                            //remove old
                            var index = real_timeline.IndexOf(cmd);
                            real_timeline.Remove(cmd);
                            real_timeline.Remove(next_cmd);

                            //insert new
                            real_timeline.Add(new_cmd);

                            //skip next command
                            i++;

                            effect_count++;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 删除无用的命令，即理论上根本不会被执行到的命令
        /// 点名批评 -> 381480
        ///
        ///
        /// </summary>
        /// <param name="Storyboard_objects"></param>
        /// <param name="effect_count"></param>
        public void RemoveUnusedCommand(IEnumerable<StoryboardObject> Storyboard_objects, ref int effect_count)
        {
            Event[] skip_event = new[] { Event.Loop, Event.Trigger };

            foreach (var obj in Storyboard_objects)
            {
                foreach (var timeline in obj.CommandMap.Where(x => !skip_event.Contains(x.Key)).Select(x => x.Value))
                {
                    /*
                    for (int i = timeline.Count-1; i>=0; i--)
                    {
                        var cmd = timeline[i]; //待检查的命令

                        //立即命令就跳过
                        if (cmd.StartTime==cmd.EndTime)
                            continue;

                        for (int x = i; x>=0; x--)
                        {
                            var itor = timeline[x]; //遍历的命令

                            /*
                             *line n-1 (itor): |--------------------------|
                             *line n   (cmd) :             |--------------|       <--- Kill , biatch
                             * /
                            if (cmd.EndTime<=itor.EndTime
                                &&cmd.StartTime>=itor.StartTime
                                &&itor.RelativeLine<cmd.RelativeLine
                                )
                            {
                                timeline.Remove(cmd);

                                Log.Debug($"Remove unused command ({cmd}) in ({obj})，compare with ({itor})");
                                Suggest(cmd, $"此命令被\"{itor}\"命令覆盖而不被执行到，可删除");
                                effect_count++;

                                //已被制裁
                                break;
                            }
                        }
                    }
                    */

                    for (int i = 0; timeline.Overlay && i<timeline.Count-1; i++)
                    {
                        var cmd = timeline[i];

                        /*
                         *line n-1 (cmd): |--------------------------|
                         *line n   (next_cmd) :      |--------------|       <--- Killed , biatch
                         */
                        for (int t = i+1;timeline.Overlay && t<timeline.Count; t++)
                        {
                            var next_cmd = timeline[t];

                            if (next_cmd.StartTime>cmd.EndTime)
                                break;

                            if (next_cmd.EndTime<=cmd.EndTime && next_cmd.EndTime!=next_cmd.StartTime && cmd.RelativeLine<next_cmd.RelativeLine)
                            {
                                timeline.Remove(next_cmd);

                                Log.Debug($"Remove unused command ({next_cmd}) in ({obj})，compare with ({cmd})");
                                Suggest(next_cmd, $"此命令被\"{cmd}\"命令覆盖而不被执行到，可删除");
                                effect_count++;
                            }
                        }
                    }
                }
            }
        }
    }
}
