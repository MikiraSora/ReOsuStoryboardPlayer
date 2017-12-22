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

                Command command = null;
                if (current_time < command_list[0].StartTime)
                {
                    //早于开始前
                    command = command_list[0];
                }
                else if (current_time > command_list[command_list.Count - 1].EndTime)
                {
                    //迟于结束后
                    command = command_list[command_list.Count - 1];
                }

                if (command == null)
                {
                    foreach (var cmd in command_list)
                    {
                        if (current_time >= cmd.StartTime && current_time <= cmd.EndTime)
                        {
                            command = cmd;
                            break;
                        }
                    }
                }

                if (command == null)
                {
                    for (int i = 0; i < command_list.Count - 1; i++)
                    {
                        var cur_cmd = command_list[i];
                        var next_cmd = command_list[i + 1];

                        if (current_time >= cur_cmd.EndTime && current_time <= next_cmd.StartTime)
                        {
                            command = cur_cmd;
                            break;
                        }
                    }
                }

                if (command != null)
                {
                    CommandExecutor.DispatchCommandExecute(this, current_time, command);
                    CommandExecutor.ClearCommandRegisterArray();
                }
            }

            void UpdateForEachCommand(List<Command> command_list)
            {
                foreach (var cmd in command_list)
                {
                    if (current_time>=cmd.StartTime&&current_time<=cmd.EndTime)
                    {
                        CommandExecutor.DispatchCommandExecute(this, current_time, cmd);
                    }
                }
            }
        }

        public override string ToString() => $"{Z}: {ImageFilePath} : {FrameStartTime}~{FrameEndTime}";
    }
}
