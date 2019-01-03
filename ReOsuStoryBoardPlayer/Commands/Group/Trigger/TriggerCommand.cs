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

        public TriggerCommand(TriggerConditionBase condition)
        {
            Event=Event.Trigger;
            Condition=condition??throw new ArgumentNullException(nameof(condition));
        }

        public override void Execute(StoryBoardObject @object, float time)
        {
            //TODO,不对貌似也没啥可实现的
        }

        public void BindObject(StoryBoardObject obj)
        {
            Debug.Assert(bind_object==null, "Not allow trigger command bind more storyboard objects");

            bind_object=obj??throw new ArgumentNullException(nameof(obj));
        }

        public void Trig()
        {
            foreach (Command command in SubCommands.Values.SelectMany(l=>l))
            {
                bind_object.AddCommand(command);
            }
        }

        public void Clean()
        {
            foreach (Command command in SubCommands.Values.SelectMany(l => l))
            {
                bind_object.RemoveCommand(command);
            }
        }

        public bool CheckTimeVaild(float time)
        {
            //todo
            return false;
        }

        public override string ToString() => $"{base.ToString()} {Condition}";
    }
}
