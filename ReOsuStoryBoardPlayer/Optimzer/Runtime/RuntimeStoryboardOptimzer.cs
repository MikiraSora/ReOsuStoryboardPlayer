using ReOsuStoryBoardPlayer.Commands;
using System;
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
            TrimFrameTime(storyboard_objects,ref effect_count);
            Log.User("TrimFrameTime() optimze count:"+effect_count);

            /*
            effect_count=0;
            CombineCommands(storyboard_objects, ref effect_count);
            Log.User("CombineCommands() optimze count:"+effect_count);
            */
        }

        public void TrimFrameTime(IEnumerable<StoryBoardObject> storyboard_objects,ref int effect_count)
        {
            foreach (var obj in storyboard_objects)
            {
                if (obj==null 
                    || obj is StoryboardAnimation  //qnmd
                    || !obj.CommandMap.TryGetValue(Event.Fade, out var fade_list)
                    ||fade_list.Count==0)
                    continue;

                var first_fade = fade_list.First() as FadeCommand;

                if (first_fade!=null&&first_fade.StartValue==0)
                    obj.FrameStartTime=first_fade.StartTime;

                var last_fade = fade_list.Last() as FadeCommand;

                if (last_fade!=null&&last_fade.EndValue==0)
                    obj.FrameEndTime=last_fade.EndTime;

                effect_count++;
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
    }
}
