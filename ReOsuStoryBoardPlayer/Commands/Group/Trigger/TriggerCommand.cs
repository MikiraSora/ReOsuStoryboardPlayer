using ReOsuStoryBoardPlayer.Commands;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReOsuStoryBoardPlayer.Commands.Group.Trigger
{
    public class TriggerCommand : GroupCommand
    {
        public TriggerConditionBase Condition { get; set; }

        private Dictionary<Event, TriggerSubTimelineCommand> cache_timline_wrapper = new Dictionary<Event, TriggerSubTimelineCommand>();

        private StoryBoardObject bind_object;

        public int GroupID;

        public bool Trigged { get; private set; }

        public int CostTime { get; private set; }

        private float last_trigged_time = 0;

        public TriggerCommand(TriggerConditionBase condition)
        {
            Event=Event.Trigger;
            Condition=condition??throw new ArgumentNullException(nameof(condition));
        }

        public override void Execute(StoryBoardObject @object, float time)
        {
            if (Trigged)
            {
                //executed,recovery status and reset
                if (last_trigged_time+CostTime<=time)
                {
                    Trigged=false;
                    Reset(true);
                }
            }
        }

        public void BindObject(StoryBoardObject obj)
        {
            Debug.Assert(bind_object==null, "Not allow trigger command bind more storyboard objects");

            bind_object=obj??throw new ArgumentNullException(nameof(obj));
        }

        public void Trig(float time)
        {
            /*
            if (Trigged)
                return; //trigged,ignore.
            */

            Reset(true);
            last_trigged_time=time;

            AttachSubCommands(time);

            //todo ,优化掉这货
            bind_object.SortCommands();
            bind_object.UpdateObjectFrameTime();

            Trigged=true;
        }

        private void AttachSubCommands(float time)
        {
            foreach (var wrapper in cache_timline_wrapper)
            {
                bind_object.InternalRemoveCommand(wrapper.Value);
                wrapper.Value.UpdateOffset((int)time);
                bind_object.InternalAddCommand(wrapper.Value);
            }
        }
        
        private void DetachSubCommands(bool magic=false)
        {
            foreach (var wrapper in cache_timline_wrapper.Where(x=>!magic||(x.Value.StartTime==x.Value.EndTime&&x.Value.StartTime==0)))
                bind_object.InternalRemoveCommand(wrapper.Value);
        }

        public void Reset(bool magic = false)
        {
            DetachSubCommands(magic);

            last_trigged_time =0;

            Trigged=false;
        }

        public bool CheckTimeVaild(float time)
        {
            return StartTime<=time&&time<=EndTime;
        }

        public override void UpdateSubCommand()
        {
            base.UpdateSubCommand();

            CostTime=SubCommands.Values.SelectMany(l => l).Max(p => p.EndTime);

            cache_timline_wrapper.Clear();

            foreach (var timeline in SubCommands)
                cache_timline_wrapper[timeline.Key]=new TriggerSubTimelineCommand(this, timeline.Key);
        }

        public override string ToString() => $"{base.ToString()} {Condition}";

        public readonly static Action<StoryBoardObject> OverrideDefaultValue = obj => obj.Color.w=0;
    }
}
