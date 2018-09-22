using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReOsuStoryBoardPlayer
{
    public class StoryBoardObject
    {
        public Dictionary<Event, List<Command>> CommandMap = new Dictionary<Event, List<Command>>();

        public string ImageFilePath;

        public int FrameStartTime, FrameEndTime;

        public bool markDone = false;

        public SpriteInstanceGroup RenderGroup;

        public Layout layout;

        public int Z=-1;

        #region Transform

        public Vector Postion=new Vector(320,240), Scale=new Vector(1,1);

        public Vec4 Color=new Vec4(1,1,1,1);

        public float Rotate=0;

        public Vector Anchor=new Vector(0.5f,0.5f);

        public bool IsAdditive=false,IsHorizonFlip=false,IsVerticalFlip=false;

        #endregion

        public virtual void Update(float current_time)
        {
#if DEBUG
            ExecutedCommands.ForEach(c => c.IsExecuted = false);
            ExecutedCommands.Clear();
#endif

            if (current_time > FrameEndTime)
            {
                markDone = true;
                return;
            }

            foreach (var command_pair in CommandMap)
            {
                if (command_pair.Key==Event.Loop)
                {
                    UpdateForEachCommand(command_pair.Value);
                    continue;
                }

                var command_list = command_pair.Value;

                var command = CommandExecutor.PickCommand(current_time, command_list);
                
                if (command != null)
                {
                    CommandExecutor.DispatchCommandExecute(this, current_time, command);

                    //???不应该这时候clear,但我忘记为啥要这样，先注释mark
                    //CommandExecutor.ClearCommandRegisterArray();
                }
            }

            CommandExecutor.ClearCommandRegisterArray();

            //每个物件可能有多个Loop,全都执行了，至于命令先后循序，交给DispatchCommandExecute()判断
            void UpdateForEachCommand(List<Command> command_list)
            {
                foreach (var cmd in command_list)
                    CommandExecutor.Loop(this, current_time, cmd);
            }
        }

        public override string ToString() => $"line {FileLine} (index {Z}): {ImageFilePath} : {FrameStartTime}~{FrameEndTime}";

#if DEBUG
        internal List<Command> ExecutedCommands=new List<Command>();
        internal long FileLine;
#endif
    }
}
