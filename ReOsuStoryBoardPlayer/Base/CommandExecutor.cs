using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReOsuStoryBoardPlayer
{
    public static class CommandExecutor
    {
        public delegate void CommandFunc(StoryBoardObject ref_obj, float interpolator_value, Command command);

        #region Build CommandFunctionMap

        public static Dictionary<Event, CommandFunc> CommandFunctionMap = new Dictionary<Event, CommandFunc>()
        {
            {Event.Move,Move},
            {Event.Fade,Fade},
            {Event.Color,Color},
            {Event.MoveX,MoveX},
            {Event.MoveY,MoveY},
            {Event.Scale,Scale},
            {Event.VectorScale,ScaleVector},
            {Event.Rotate,Rotate},
            {Event.Loop,Loop},
            {Event.HorizonFlip,HorizonFlip},
            {Event.VerticalFlip,VerticalFlip},
            {Event.AdditiveBlend,AdditiveBlend}
        };

        #endregion

        #region Command Implatements

        public static void Move(StoryBoardObject ref_obj, float current_value, Command command)
        {
            MoveCommandParameters parameters = (MoveCommandParameters)command.Parameters;

            Vector cal_result = current_value >= 1 ? parameters.EndPosition : (current_value <= 0 ? parameters.StartPostion : parameters.StartPostion + parameters.Distance * current_value);
    
            ref_obj.Postion = cal_result;
        }

        public static void MoveX(StoryBoardObject ref_obj, float current_value, ReOsuStoryBoardPlayer.Command command)
        {
            MoveXCommandParameters parameters = (MoveXCommandParameters)command.Parameters;

            float cal_result = current_value >= 1 ? parameters.EndX : (current_value <= 0 ? parameters.StartX : parameters.StartX + parameters.Distance * current_value);

            ref_obj.Postion.x = cal_result;
        }

        public static void MoveY(StoryBoardObject ref_obj, float current_value, ReOsuStoryBoardPlayer.Command command)
        {
            MoveYCommandParameters parameters = (MoveYCommandParameters)command.Parameters;

            float cal_result = current_value >= 1 ? parameters.EndY : (current_value <= 0 ? parameters.StartY : parameters.StartY + parameters.Distance * current_value);

            ref_obj.Postion.y = cal_result;
        }

        public static void Rotate(StoryBoardObject ref_obj, float current_value, ReOsuStoryBoardPlayer.Command command)
        {
            RotateCommandParamesters parameters = (RotateCommandParamesters)command.Parameters;

            float cal_result = 0;

            if (current_value<=0)
            {
                cal_result = (parameters.StartRotate);
            }
            else if (current_value>=1)
            {
                cal_result = (parameters.EndRotate);
            }
            else
            {
                cal_result = parameters.StartRotate + parameters.Distance * current_value;
            }

            cal_result = -cal_result;

            cal_result = (float)(cal_result/Math.PI*180.0f);

#if DEBUG
            //todo this is a bug fix
            if (float.IsNaN(cal_result))
            {
                cal_result = 0;
            }
#endif
            ref_obj.Rotate =cal_result ;
        }

        public static void Fade(StoryBoardObject ref_obj, float current_value, ReOsuStoryBoardPlayer.Command command)
        {
            FadeCommandParamesters parameters = (FadeCommandParamesters)command.Parameters;
            
            float cal_result = current_value >= 1 ? parameters.EndFade : (current_value <= 0 ? parameters.StartFade : parameters.StartFade + parameters.Distance * current_value);
            
            ref_obj.Color.w = cal_result;
        }

        public static void Scale(StoryBoardObject ref_obj, float current_value, ReOsuStoryBoardPlayer.Command command)
        {
            ScaleCommandParameters parameters = (ScaleCommandParameters)command.Parameters;

            float cal_result = current_value >= 1 ? parameters.EndScale : (current_value <= 0 ? parameters.StartScale : parameters.StartScale + parameters.Distance * current_value);

            ref_obj.Scale.x=ref_obj.Scale.y= cal_result;
        }

        public static void ScaleVector(StoryBoardObject ref_obj, float current_value, ReOsuStoryBoardPlayer.Command command)
        {
            ScaleVectorCommandParamesters parameters = (ScaleVectorCommandParamesters)command.Parameters;

            Vector cal_result = current_value >= 1 ? parameters.EndScale : (current_value <= 0 ? parameters.StartScale : parameters.StartScale + parameters.Distance * current_value);

            ref_obj.Scale = cal_result;
        }

        public static void Color(StoryBoardObject ref_obj, float current_value, ReOsuStoryBoardPlayer.Command command)
        {
            ColorCommandParameters parameters = (ColorCommandParameters)command.Parameters;

            Vec4 temp = new Vec4();
            temp.x = Math.Min((parameters.StartColor.x + parameters.Distance.x * current_value),1);
            temp.y = Math.Min((parameters.StartColor.y + parameters.Distance.y * current_value),1);
            temp.z = Math.Min((parameters.StartColor.z + parameters.Distance.z * current_value),1);

            Vec4 cal_result = current_value >= 1 ? parameters.EndColor : (current_value <= 0 ? parameters.StartColor : temp);
                
            float a = ref_obj.Color.w;
            ref_obj.Color = temp;
            ref_obj.Color.w = a;
        }

        public static void Loop(StoryBoardObject ref_obj, float current_value, ReOsuStoryBoardPlayer.Command _command)
        {
            int recovery_time = (int)((current_value - _command.StartTime));
            LoopCommand loop_command = (LoopCommand)_command;

            int current_time = (int)(recovery_time % loop_command.LoopParamesters.CostTime);

            int current_loop_index = (int)(recovery_time / loop_command.LoopParamesters.CostTime);

            var command_map = loop_command.LoopParamesters.LoopCommandList;
            
            foreach (var command_list in command_map.Values)
            {
                var command = PickCommand(current_time, command_list);

                if (command != null)
                {
                    //store command start/end time
                    var offset_time = (int)(loop_command.StartTime + current_loop_index * loop_command.LoopParamesters.CostTime);
                    command.StartTime += offset_time;
                    command.EndTime += offset_time;

                    DispatchCommandExecute(ref_obj, current_value, command);

                    //restore command
                    command.StartTime -= offset_time;
                    command.EndTime -= offset_time;
                }
            }
        }

        public static void Parameter(StoryBoardObject ref_obj, float current_value, Command command)
        {
            ParameterCommandParamester param = (ParameterCommandParamester)command.Parameters;

            switch (param.Effect)
            {
                case EffectParameter.HorizontalFlip:
                    break;
                case EffectParameter.VerticalFlip:
                    break;
                case EffectParameter.AdditiveBlend:
                    ref_obj.IsAdditive = (current_value >= 0 && current_value <= 1);
                    break;
                default:
                    break;
            }
        }

        public static void AdditiveBlend(StoryBoardObject ref_obj, float current_value, Command command)
        {
            ref_obj.IsAdditive = command.StartTime == command.EndTime? 
                true :
                0 < current_value && current_value < 1;
        }

        public static void HorizonFlip(StoryBoardObject ref_obj, float current_value, Command command)
        {
            ref_obj.IsHorizonFlip = command.StartTime == command.EndTime ?
                true : 
                0 < current_value && current_value < 1;
        }

        public static void VerticalFlip(StoryBoardObject ref_obj, float current_value, Command command)
        {
            ref_obj.IsVerticalFlip = command.StartTime == command.EndTime ?
                true :
                0 <current_value&& current_value < 1;
        }

        #endregion

        static (Command command,int StartTime,int EndTime)[] _ExecutedCommandRegisterArray = new (Command command, int StartTime, int EndTime)[14];

        public static void ClearCommandRegisterArray() => Array.Clear(_ExecutedCommandRegisterArray, 0, 14);

        public static void DispatchCommandExecute(StoryBoardObject ref_obj, float current_playing_time, Command command)
        {
            #region Check Command Conflct

            var reg_cmd_info = command.CommandEventType != Event.Loop ? _ExecutedCommandRegisterArray[(int)command.CommandEventType] : (null, 0, 0);

            /*
             如果之前有同类型命令执行了，比如Loop里面的子命令
             将之前的命令和现在的命令当做在一条时间轴上判断
                                                                      (allow)
                                                                      |
             |-----reg_cmd_a-----|    o   |------current_cmd------|   o  |-----reg_cmd_b-----|
                                      |
                                      current_playing_time(reject)
             至于考虑重叠的，不考虑了
             */
            if (
                //是否存在已执行命令
                reg_cmd_info.command != null && (
                    //已执行的命令比现在还靠后
                    command.EndTime <= reg_cmd_info.command.StartTime ||

                    //现在命令比已执行命令还靠后，但当前时间在现命令之前
                    (reg_cmd_info.command.StartTime < command.StartTime && current_playing_time < command.StartTime)
                ))
                return;//不给执行

            _ExecutedCommandRegisterArray[(int)command.CommandEventType] = (command, command.StartTime, command.EndTime);

            #endregion

            #region Calculate interpolator value

            float current_value = 0;

            if (current_playing_time<command.StartTime)
            {
                current_value = 0;
            }
            else if(current_playing_time > command.EndTime)
            {
                current_value = 1;
            }
            else
                current_value= command.Easing.calculate(current_playing_time - command.StartTime, command.StartTime, command.EndTime);

            //fix infinity
            if (float.IsInfinity(current_value))
            {
                current_value = 1;
            }

            #endregion

            command.executor(ref_obj, current_value, command);

            command.IsExecuted = true;
            ref_obj.ExecutedCommands.Add(command);
        }

        public static Command PickCommand(float current_time,IEnumerable<Command> command_list)
        {
            Command command = null;
            if (current_time < command_list.First().StartTime)
            {
                //早于开始前
                return command_list.First();
            }
            else if (current_time > command_list.Last().EndTime)
            {
                //迟于结束后
                return command_list.Last();
            }

            //尝试选取在时间范围内的命令
            if (command == null)
            {
                foreach (var cmd in command_list)
                {
                    if (current_time >= cmd.StartTime && current_time <= cmd.EndTime)
                    {
                        return cmd;
                    }
                }
            }

            //尝试选取在命令之间的前者
            if (command == null)
            {
                for (int i = 0; i < command_list.Count()-1; i++)
                {
                    var cur_cmd = command_list.ElementAt(i);
                    var next_cmd = command_list.ElementAt(i + 1);

                    if (current_time >= cur_cmd.EndTime && current_time <= next_cmd.StartTime)
                    {
                        return cur_cmd;
                    }
                }
            }

            return null;
        }
    }
}
