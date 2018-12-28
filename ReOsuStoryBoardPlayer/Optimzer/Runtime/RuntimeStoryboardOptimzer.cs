using ReOsuStoryBoardPlayer.Commands;
using ReOsuStoryBoardPlayer.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReOsuStoryBoardPlayer.Optimzer.Runtime
{
    public class RuntimeStoryboardOptimzer : OptimzerBase
    {
        public override void Optimze(IEnumerable<StoryBoardObject> storyboard_objects)
        {
            int effect_count = 0;
            using (StopwatchRun.Count(() => "TrimFrameTime() optimze count:"+effect_count))
                TrimFrameTime(storyboard_objects, ref effect_count);

            effect_count=0;
            using (StopwatchRun.Count(() => "RemoveUnusedCommand() optimze count:"+effect_count))
                RemoveUnusedCommand(storyboard_objects, ref effect_count);
            
            effect_count=0;
            using (StopwatchRun.Count(() => "TrimInitalEffect() optimze count:"+effect_count))
                TrimInitalEffect(storyboard_objects, ref effect_count);
        }

        /// <summary>
        /// 计算Fade时间轴，优化物件的FrameStartTime/EndTime，避免不必要的计算
        /// 点名批评 -> 181957
        /// 
        ///           
        ///Sprite,Foreground,Centre,"sb\light.png",320,240
        /// S,0,0,,0.22        <----Non-optimze obj.FrameStartTime
        /// MX,0,0,,278
        /// F,0,126739,,0            <----Kill by Optimzer
        /// F,0,208016,,0.7,1           <----Actual obj.FrameStartTime
        /// C,0,208016,,255,255,255
        /// P,0,208016,,A
        /// MY,0,208016,209286,520,-40   <----Actual obj.FrameEndTime
        ///
        /// </summary>
        /// <param name="storyboard_objects"></param>
        /// <param name="effect_count"></param>
        public void TrimFrameTime(IEnumerable<StoryBoardObject> storyboard_objects, ref int effect_count)
        {
            foreach (var obj in storyboard_objects)
            {
                if (obj==null
                    ||obj is StoryboardAnimation  //qnmd
                    ||!obj.CommandMap.TryGetValue(Event.Fade, out var fade_list)
                    ||fade_list.Count==0)
                    continue;

                var first_fade = fade_list.First() as FadeCommand;

                if (first_fade!=null)
                {
                    FadeCommand front_fade = null;


                    if ((first_fade.EndTime==0 || first_fade.StartTime==first_fade.EndTime)
                        &&first_fade.EndValue==0
                        &&fade_list.Count>1)
                    {
                        if (fade_list.Skip(1).First() is FadeCommand second_fade)
                        {
                            front_fade=second_fade;
                        }
                    }
                    else if (first_fade.StartValue==0)
                    {
                        front_fade=first_fade;
                    }

                    if (front_fade!=null&&front_fade.StartTime<=obj.FrameStartTime)
                    {
                        obj.FrameStartTime=front_fade.StartTime;
                        Suggest(obj, $"FrameTime可优化成{front_fade.StartTime}");
                        effect_count++;
                    }
                }

                var last_fade = fade_list.Last() as FadeCommand;

                if (last_fade!=null&&last_fade.EndValue==0)
                {
                    obj.FrameEndTime=last_fade.EndTime;
                    Suggest(obj,$"EndTime可优化成{last_fade.StartTime}.");
                    effect_count++;
                }
            }
        }

        public void CombineCommands(IEnumerable<StoryBoardObject> storyboard_objects, ref int effect_count)
        {
            foreach (var obj in storyboard_objects)
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
                            real_timeline.Insert(index, new_cmd);

                            //skip next command
                            i++;

                            effect_count++;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 将时间轴单个立即命令直接应用到物件上，减少物件执行命令频率
        /// 点名批评 -> 181957 
        /// 
        ///Sprite,Foreground,Centre,"sb\light.png",320,240
        /// S,0,0,,0.22        <---- Set as Object.Scale inital value by Optimzer
        /// MX,0,0,,278        <---- Set as Object.Position.X inital value by Optimzer
        /// F,0,126739,,0            
        /// F,0,208016,,0.7,1           
        /// C,0,208016,,255,255,255
        /// P,0,208016,,A             <---- Set as Object.IsAdditive value by Optimzer
        /// MY,0,208016,209286,520,-40   
        ///
        /// </summary>
        /// <param name="storyboard_objects"></param>
        /// <param name="effect_count"></param>
        public void TrimInitalEffect(IEnumerable<StoryBoardObject> storyboard_objects, ref int effect_count)
        {
            var events = Enum.GetValues(typeof(Event));

            foreach (var obj in storyboard_objects)
            {
                foreach (var timeline in obj.CommandMap.Values.Where(x => x.Count==1))
                {
                    Command cmd = timeline.FirstOrDefault();

                    if (cmd.EndTime<=obj.FrameStartTime
                        &&cmd.StartTime==cmd.EndTime 
                        &&((cmd is ValueCommand vcmd)&&(vcmd.GetEndValue()==vcmd.GetStartValue())||
                        cmd is StateCommand))
                    {
                        /*
                         * 时间或者初始变化值 都相同 的命令可以直接应用到物件上
                         */
                        timeline.Remove(cmd);

                        obj.BaseTransformResetAction += (target) => {
                            cmd.Execute(target, cmd.EndTime+1);
                        };

                        effect_count++;
                    }
                }

                //去掉没有命令的时间轴
                foreach (Event e in events)
                    if (obj.CommandMap.TryGetValue(e, out var timeline)&&timeline.Count==0)
                    {
                        obj.CommandMap.Remove(e);
                        effect_count++;
                    }
            }
        }

        /// <summary>
        /// 删除无用的命令，即理论上根本不会被执行到的命令
        /// 点名批评 -> 381480
        /// </summary>
        /// <param name="storyboard_objects"></param>
        /// <param name="effect_count"></param>
        public void RemoveUnusedCommand(IEnumerable<StoryBoardObject> storyboard_objects, ref int effect_count)
        {
            foreach (var obj in storyboard_objects)
            {
                foreach (var timeline in obj.CommandMap.Values)
                {
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
                            if (itor.StartTime>cmd.StartTime)
                                break;
                                */

                            /*
                             *line n-1 (itor): |--------------------------|
                             *line n   (cmd) :             |--------------|       <--- Kill , biatch
                             */
                            if (cmd.EndTime<=itor.EndTime
                                && cmd.StartTime>=itor.StartTime
                                && itor.RelativeLine<cmd.RelativeLine
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
                }
            }
        }
    }
}
