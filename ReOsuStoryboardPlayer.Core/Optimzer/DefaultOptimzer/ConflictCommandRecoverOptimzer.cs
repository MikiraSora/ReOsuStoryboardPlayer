using ReOsuStoryboardPlayer.Core.Base;
using ReOsuStoryboardPlayer.Core.Commands;
using ReOsuStoryboardPlayer.Core.Utils;
using System.Collections.Generic;

namespace ReOsuStoryboardPlayer.Core.Optimzer.DefaultOptimzer
{
    public class ConflictCommandRecoverOptimzer : OptimzerBase
    {
        public override void Optimze(int level, IEnumerable<StoryboardObject> Storyboard_objects)
        {
            if (level>=3)
            {
                int effect_count = 0;
                using (StopwatchRun.Count(() => "TrimHoldingStatusCommand() optimze count:"+effect_count))
                    TrimHoldingStatusCommand(Storyboard_objects, ref effect_count);
            }
        }

        /*
         *  MX,0,94938,130595,320     ---simplify--->     MX,0,94938,94938,320,320
         *  it could avoid a part cause about command conflict:
         *
         *  MX,0,94938,130595,320
         *  M,20,95008,95078,320,240,322.9271,226.3689
         *  M,20,95078,95148,322.9271,226.3689,320.6659,236.2696
         *  M,20,95148,95218,320.6659,236.2696,321.3301,232.5321
         */

        private void TrimHoldingStatusCommand(IEnumerable<StoryboardObject> storyboard_objects, ref int effect_count)
        {
            foreach (var obj in storyboard_objects)
            {
                foreach (CommandTimeline timeline in obj.CommandMap.Values)
                {
                    if (!timeline.Overlay)
                        continue;

                    for (int i = 0; i<timeline.Count; i++)
                    {
                        if (timeline[i] is ValueCommand cmd)
                        {
                            if (cmd.EqualityComparer.Equals(cmd.GetEndValue(), cmd.GetStartValue())&&cmd.Easing==EasingTypes.None)
                            {
                                timeline.Remove(cmd);
                                cmd.EndTime=cmd.StartTime;
                                timeline.Add(cmd);

                                effect_count++;
                            }
                        }
                    }
                }
            }
        }
    }
}