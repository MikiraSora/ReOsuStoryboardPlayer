using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ReOsuStoryboardPlayer.Core.Base;
using ReOsuStoryboardPlayer.Core.Commands;
using ReOsuStoryboardPlayer.Core.Commands.Group;
using ReOsuStoryboardPlayer.Core.Utils;

namespace ReOsuStoryboardPlayer.Core.Optimzer.DefaultOptimzer
{
    /// <summary>
    /// 一般用于播放器的物件优化器，提升实现播放器的物件更新效率
    /// </summary>
    public class RuntimeOptimzer : OptimzerBase
    {
        public override void Optimze(int level,IEnumerable<StoryboardObject> Storyboard_objects)
        {
            if (level>=1)
            {
                int effect_count = 0;
                using (StopwatchRun.Count(() => "TrimFrameTime() optimze count:"+effect_count))
                    TrimFrameTime(Storyboard_objects, ref effect_count);

                effect_count=0;
                using (StopwatchRun.Count(() => "TrimInitalEffect() optimze count:"+effect_count))
                    TrimInitalEffect(Storyboard_objects, ref effect_count);
            }
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
        /// <param name="Storyboard_objects"></param>
        /// <param name="effect_count"></param>
        public void TrimFrameTime(IEnumerable<StoryboardObject> Storyboard_objects, ref int effect_count)
        {
            foreach (var obj in Storyboard_objects)
            {
                if (obj==null
                    ||obj is StoryboardAnimation  //qnmd
                    ||!obj.CommandMap.TryGetValue(Event.Fade, out var fade_list)
                    ||fade_list.Count==0
                    ||fade_list.Overlay
                    ||fade_list.Count<=1
                    )
                    continue;

                var first_fade = fade_list.First() as FadeCommand;

                if (first_fade!=null)
                {
                    FadeCommand front_fade = null;

                    if ((first_fade.EndTime==0||first_fade.StartTime==first_fade.EndTime) //是否为立即命令（dutation=0）
                        &&first_fade.EndValue==0) //是否是隐藏的
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

                    if (front_fade!=null&&obj.FrameStartTime<=front_fade.StartTime)
                    {
                        var trigger_time = obj.ContainTrigger ? obj.CommandMap[Event.Trigger].Min(x => x.StartTime) : int.MaxValue;
                        var trim_start_time = Math.Min(trigger_time, front_fade.StartTime);

                        obj.BaseTransformResetAction+=x => x.FrameStartTime=trim_start_time;
                        obj.FrameStartTime=trim_start_time;
                        Suggest(obj, $"FrameTime可优化成{front_fade.StartTime}");
                        effect_count++;
                    }
                }

                var last_fade = fade_list.Last() as FadeCommand;

                if (last_fade!=null&&last_fade.EndValue==0)
                {
                    obj.FrameEndTime=last_fade.EndTime;
                    Suggest(obj, $"EndTime可优化成{last_fade.StartTime}.");
                    effect_count++;
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
        /// <param name="Storyboard_objects"></param>
        /// <param name="effect_count"></param>
        public void TrimInitalEffect(IEnumerable<StoryboardObject> Storyboard_objects, ref int effect_count)
        {
            var events = Enum.GetValues(typeof(Event));

            foreach (var obj in Storyboard_objects)
            {
                //物件命令数量!=0 且 无Trigger对应类型的子命令
                foreach (var timeline in obj.CommandMap.Where(x => x.Value.Count==1&&((!obj.ContainTrigger)||(
                !obj.CommandMap[Event.Trigger]
                .OfType<GroupCommand>()
                .SelectMany(
                    l => l.SubCommands
                    .Where(w => w.Key==x.Key)).Select(m => m.Value)
                    .Any()))).Select(e => e.Value))
                {
                    Command cmd = timeline.FirstOrDefault();

                    if (cmd.EndTime<=obj.FrameStartTime
                        &&cmd.StartTime==cmd.EndTime
                        //C,0,0,,0,0,0,226,172,247
                        &&((cmd is ValueCommand)/*&&(vcmd.EqualityComparer.Equals(vcmd.GetEndValue(), vcmd.GetStartValue()))*/||
                        cmd is StateCommand))
                    {
                        /*
                         * 低于时间或者初始变化值 都相同 的命令可以直接应用到物件上
                         */

                        timeline.Remove(cmd);

                        obj.BaseTransformResetAction+=(target) =>
                        {
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
    }
}
