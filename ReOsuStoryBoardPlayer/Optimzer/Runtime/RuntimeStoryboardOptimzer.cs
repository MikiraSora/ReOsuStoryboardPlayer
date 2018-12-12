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
        }

        public void TrimFrameTime(IEnumerable<StoryBoardObject> storyboard_objects,ref int effect_count)
        {
            foreach (var obj in storyboard_objects)
            {
                if (!obj.CommandMap.TryGetValue(Event.Fade, out var fade_list)||fade_list.Count==0)
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

        /* ValueCommand<VALUE_TYPE> 我敲里吗
        public void CombineCommands(IEnumerable<StoryBoardObject> storyboard_objects)
        {
            foreach (var obj in storyboard_objects)
            {
                foreach (var pair in obj.CommandMap)
                {
                    var timeline = pair.Value.OfType<ValueCommand>();

                    for (int i = 0; i<timeline.Count()-1; i++)
                    {
                        var cmd = timeline.ElementAt(i);
                        var next_cmd = timeline.ElementAt(i+1);
                        
                        if ((cmd.Easing==next_cmd.Easing)
                            &&(cmd.EndTime==next_cmd.StartTime)
                            &&(cmd.EndValue==next_cmd.StartValue))
                        {
                            //combine
                        }
                    }
                }
            }
        }
        */
    }
}
