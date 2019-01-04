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
                    Log.Debug($"Object {bind_object}({this}) Reset in time {time}!");
                    Reset();
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
            if (Trigged)
                return; //trigged,ignore.

            Log.Debug($"Object {bind_object}({this}) Trigged in time {time}!");

            last_trigged_time=time;

            foreach (Command command in SubCommands.Values.SelectMany(l => l))
            {
                //map to real time
                command.StartTime+=(int)time;
                command.EndTime+=(int)time;

                bind_object.AddCommand(command);
            }

            //todo ,优化掉这货
            bind_object.SortCommands();

            Trigged=true;
        }

        public void Reset()
        {
            foreach (Command command in SubCommands.Values.SelectMany(l => l))
            {
                //recovery to relative time
                command.StartTime-=(int)last_trigged_time;
                command.EndTime-=(int)last_trigged_time;

                bind_object.RemoveCommand(command);
            }

            last_trigged_time=0;

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
        }

        public override string ToString() => $"{base.ToString()} {Condition}";
    }
}
