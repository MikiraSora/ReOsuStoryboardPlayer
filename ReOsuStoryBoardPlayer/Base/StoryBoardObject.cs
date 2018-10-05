using ReOsuStoryBoardPlayer.Commands;
using ReOsuStoryBoardPlayer.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReOsuStoryBoardPlayer
{
    public class StoryBoardObject
    {

#if DEBUG
        internal
#else
        private
#endif
         Dictionary<Event, _CommandTimeline> CommandMap = new Dictionary<Event, _CommandTimeline>();

        public string ImageFilePath;

        public int FrameStartTime, FrameEndTime;

        public bool markDone = false;

        public SpriteInstanceGroup RenderGroup;

        public Layout layout;

        public int Z=-1;

        public CommandConflictChecker CommandConflictChecker { get; } = new CommandConflictChecker();

        #region Transform

        public Vector Postion=new Vector(320,240), Scale=new Vector(1,1);

        public Vec4 Color=new Vec4(1,1,1,1);

        public float Rotate=0;

        public Vector Anchor=new Vector(0.5f,0.5f);

        public bool IsAdditive=false,IsHorizonFlip=false,IsVerticalFlip=false;

        #endregion

        public void AddCommand(_Command command)
        {
            if (command is _LoopCommand loop)
            {
                AddCommand(loop);
            }

            if (!CommandMap.TryGetValue(command.Event, out var timeline))
                timeline = CommandMap[command.Event] = new _CommandTimeline();
            timeline.Add(command);
        }

        void AddCommand(_LoopCommand loop_command)
        {
            foreach (var @event in loop_command.SubCommands.Keys)
            {
                var sub_command_wrapper = new _LoopSubTimelineCommand(loop_command, @event);
                AddCommand(sub_command_wrapper);
            }
        }

        public void AddCommand(_CommandTimeline timeline) => timeline.ForEach(c => AddCommand(c));
        
        public virtual void Update(float current_time)
        {
            var temp = ObjectPool<List<_Command>>.Instance.GetObject();

#if DEBUG
            ExecutedCommands.ForEach(c => c.IsExecuted = false);
            ExecutedCommands.Clear();
#endif
            //CommandConflictChecker.Reset();

            foreach (var timeline in CommandMap.Values)
            {
                foreach (var command in timeline.PickCommands(current_time, temp))
                {
                    command.Execute(this, current_time);

#if DEBUG
                    MarkCommandExecuted(command);
#endif
                }

                temp.Clear();
            }

            ObjectPool<List<_Command>>.Instance.PutObject(temp);
        }

        public override string ToString() => $"line {FileLine} (index {Z}): {ImageFilePath} : {FrameStartTime}~{FrameEndTime}";

#if DEBUG
        internal List<_Command> ExecutedCommands=new List<_Command>();


        internal void MarkCommandExecuted(_Command command,bool is_exec=true)
        {
            if (is_exec)
                ExecutedCommands.Add(command);
            else
                ExecutedCommands.Remove(command);

            command.IsExecuted = is_exec;
        }
#endif
        public long FileLine { get; set; }
    }
}
