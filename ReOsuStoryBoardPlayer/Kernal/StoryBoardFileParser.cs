using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReOsuStoryBoardPlayer
{
    public class StoryBoardFileParser
    {
        public static List<StoryBoardObject> ParseFromOsbFile(string osb_file_path, ref int z_order)
        {
            using (var reader = File.OpenText(osb_file_path))
            {
                return ParseFromStream(reader, ref z_order);
            }
        }

        public static List<StoryBoardObject> ParseFromOsuFile(string osu_file_path, ref int z_order)
        {
            using (StreamReader reader = File.OpenText(osu_file_path))
            {

                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();
                    if (line.Trim() == "[Events]")
                    {
                        break;
                    }
                }

                return ParseFromStream(reader, ref z_order);
            }
        }

        static List<StoryBoardObject> ParseFromStream(StreamReader reader,ref int z_order)
        {
            List<StoryBoardObject> obj_list = new List<StoryBoardObject>();
            List<Command> current_command = new List<Command>();
            
            StoryBoardObject current_storyboard_obj = null;

            int command_count = 0;
            long file_line = 0;
            bool isSubCommand = false;

            LoopCommand current_loop_command = null;

            while (!reader.EndOfStream)
            {
                string line = ReadFileLine();

                if (line.StartsWith("//") || line.StartsWith("[") || string.IsNullOrWhiteSpace(line))
                    continue;

                //for osu file
                if (line == "[TimingPoints]")
                    break;

                //if is command
                if (line.StartsWith(" ") || line.StartsWith("_"))
                {
                    Command cmd = ParseCommandLine(line,out isSubCommand);

                    if (cmd==null)
                    {
                        continue;
                    }

                    if (isSubCommand)
                    {
                        if (current_loop_command!=null)
                        {
                            Log.Debug($"add subCommand \"{cmd.ToString()}\" to Loop \"{current_loop_command.ToString()}\"");
                            if (!current_loop_command.LoopParamesters.LoopCommandList.ContainsKey(cmd.CommandEventType))
                                current_loop_command.LoopParamesters.LoopCommandList[cmd.CommandEventType] = new List<Command>();
                            current_loop_command.LoopParamesters.LoopCommandList[cmd.CommandEventType].Add(cmd);
                        }

                        continue;
                    }
                    else
                    {
                        current_loop_command = null;
                    }

                    command_count++;

                    //Loop 中断
                    if (cmd is LoopCommand)
                    {
                        current_loop_command = (LoopCommand)cmd;
                        current_command.Add(cmd);
                        continue;
                    }

                    current_command.Add(cmd);
                }
                else
                {
                    //try parse sprite/anamition obj
                    StoryBoardObject obj = ParseStoryBoardObject(line);
                    
                    if (obj != null)
                    {
#if DEBUG
                        obj.FileLine = file_line;
#endif
                        obj.Z = z_order++;

                        AddCommandMapToStoryboardObject(current_storyboard_obj, current_command);
                        
                        //set current storyboard obj
                        current_storyboard_obj = obj;
                        obj_list.Add(obj);

                        current_command.Clear();
                    }
                }
            }

            AddCommandMapToStoryboardObject(current_storyboard_obj, current_command);

            Log.Debug($"parsed {obj_list.Count} objects and {command_count} commands");

            return obj_list;

            string ReadFileLine()
            {
                if (!reader.EndOfStream)
                {
                    file_line++;
                    return reader.ReadLine();
                }

                return null;
            }
        }

        private static void AddCommandMapToStoryboardObject(StoryBoardObject obj, List<Command> command)
        {
            if (obj != null)
            {
                if (obj.ImageFilePath.Contains("mini_j8"))
                {

                }
                var (cmd_map, start_time, end_time) = StoryBoardAdjustment.AdujustCommands(command);

                obj.CommandMap = cmd_map;
                obj.FrameStartTime = start_time;
                obj.FrameEndTime = end_time;
            }
        }

        public static Command ParseCommandLine(string line,out bool IsSubCommand)
        {
            Command cmd = new Command();
            string[] command_params = line.Split(',');

            IsSubCommand = false;

            #region Event

            if (command_params[0].StartsWith("  "))
            {
                IsSubCommand = true;
            }

            switch (command_params[0].Trim().Replace("_",string.Empty).ToUpper())
            {
                case "M":
                    cmd.CommandEventType = Event.Move;
                    break;
                case "C":
                    cmd.CommandEventType = Event.Color;
                    break;
                case "S":
                    cmd.CommandEventType = Event.Scale;
                    break;
                case "F":
                    cmd.CommandEventType = Event.Fade;
                    break;
                case "V":
                    cmd.CommandEventType = Event.VectorScale;
                    break;
                case "R":
                    cmd.CommandEventType = Event.Rotate;
                    break;
                case "MX":
                    cmd.CommandEventType = Event.MoveX;
                    break;
                case "MY":
                    cmd.CommandEventType = Event.MoveY;
                    break;
                case "AB":
                    cmd.CommandEventType = Event.AdditiveBlend;
                    break;
                case "VF":
                    cmd.CommandEventType = Event.VerticalFlip;
                    break;
                case "HF":
                    cmd.CommandEventType = Event.HorizonFlip;
                    break;
                case "T":
                    return null;
                case "P":
                    {
                        //改一下内容,然后递归一下
                        line = line.TrimEnd();
                        string pp=string.Empty;
                        switch (line.Last())
                        {
                            case 'A':
                                pp = "AB";
                                break;
                            case 'H':
                                pp = "HF";
                                break;
                            case 'V':
                                pp = "VF";
                                break;
                            default:
                                break;
                        }
                        line = line.Replace("P", pp);

                        return ParseCommandLine(line, out IsSubCommand);
                    }
                case "L":
                    var loop_cmd = new LoopCommand
                    {
                        Easing = EasingConverter.CacheEasingInterpolatorMap[Easing.Linear],
                        CommandEventType = Event.Loop,
                        StartTime = int.Parse(command_params[1]),
                        LoopCount = int.Parse(command_params[2]),
                        executor = CommandExecutor.CommandFunctionMap[Event.Loop]
                    };
                    return loop_cmd;
                default:
                    break;
            }

            cmd.executor = CommandExecutor.CommandFunctionMap[cmd.CommandEventType];

            #endregion

            #region Easing

            Easing easingID = (Easing)int.Parse(command_params[1].Trim());

            if (!EasingConverter.CacheEasingInterpolatorMap.ContainsKey(easingID))
            {
                Log.Warn($"Cant found the easing = {easingID.ToString()} , will be set Linear");
                cmd.Easing = EasingConverter.CacheEasingInterpolatorMap[Easing.Linear];
            }
            else
                cmd.Easing = EasingConverter.CacheEasingInterpolatorMap[easingID];
            
            #endregion

            #region Start-End Time

            cmd.StartTime = int.Parse(command_params[2]);

            cmd.EndTime = string.IsNullOrWhiteSpace(command_params[3]) ? cmd.StartTime : int.Parse(command_params[3]);

            #endregion

            #region Params

            switch (cmd.CommandEventType)
            {
                case Event.Move:
                    Vector start_pos = new Vector
                    {
                        x = float.Parse(command_params[4]),
                        y = float.Parse(command_params[5])
                    };

                    Vector end_pos = start_pos;

                    if (command_params.Length>6)
                    {
                        end_pos.x = float.Parse(command_params[6]);
                        end_pos.y = float.Parse(command_params[7]);
                    }

                    cmd.Parameters = new MoveCommandParameters(start_pos, end_pos);
                    break;

                case Event.Fade:
                    float start_fade = float.Parse(command_params[4]);

                    float end_fade = start_fade;

                    if (command_params.Length > 5)
                    {
                        end_fade = float.Parse(command_params[5]);
                    }

                    cmd.Parameters = new FadeCommandParamesters(start_fade, end_fade);
                    break;

                case Event.Scale:
                    {
                        float start = float.Parse(command_params[4]);

                        float end = start;

                        if (command_params.Length > 5)
                        {
                            end = float.Parse(command_params[5]);
                        }

                        cmd.Parameters = new ScaleCommandParameters(start, end);
                    }
                    break;

                case Event.VectorScale:
                    {
                        Vector start = new Vector
                        {
                            x = float.Parse(command_params[4]),
                            y = float.Parse(command_params[5])
                        };

                        Vector end = start;

                        if (command_params.Length > 6)
                        {
                            end.x = float.Parse(command_params[6]);
                            end.y = float.Parse(command_params[7]);
                        }

                        cmd.Parameters = new ScaleVectorCommandParamesters(start, end);
                    }
                    break;

                case Event.Rotate:
                    {
                        float start = float.Parse(command_params[4]);

                        float end = start;

                        if (command_params.Length > 5)
                        {
                            end = float.Parse(command_params[5]);
                        }

                        cmd.Parameters = new RotateCommandParamesters(start, end);
                    }
                    break;

                case Event.Color:
                    {
                        Vec4 start = new Vec4
                        {
                            x = float.Parse(command_params[4]) / 255.0f,
                            y = float.Parse(command_params[5]) / 255.0f,
                            z = float.Parse(command_params[6]) / 255.0f
                        };

                        Vec4 end = start.clone();

                        if (command_params.Length > 7)
                        {
                            end.x = float.Parse(command_params[7]) / 255.0f;
                            end.y = float.Parse(command_params[8]) / 255.0f;
                            end.z = float.Parse(command_params[9]) / 255.0f;
                        }

                        cmd.Parameters = new ColorCommandParameters(start, end);
                    }
                    break;
                    /*
                case Event.Parameter:
                    {
                        EffectParameter efffect=EffectParameter.AdditiveBlend;

                        switch (command_params[4])
                        {
                            case "A":
                                efffect = EffectParameter.AdditiveBlend;
                                break;
                            case "H":
                                efffect = EffectParameter.HorizontalFlip;
                                break;
                            case "V":
                                efffect = EffectParameter.VerticalFlip;
                                break;
                            default:
                                break;
                        }

                        cmd.Parameters = new ParameterCommandParamester(efffect);
                    }
                    break;*/
                case Event.MoveX:
                    {
                        float start = float.Parse(command_params[4]);

                        float end = start;

                        if (command_params.Length > 5)
                        {
                            end = float.Parse(command_params[5]);
                        }

                        cmd.Parameters = new MoveXCommandParameters(start, end);
                    }
                    break;

                case Event.MoveY:
                    {
                        float start = float.Parse(command_params[4]);

                        float end = start;

                        if (command_params.Length > 5)
                        {
                            end = float.Parse(command_params[5]);
                        }

                        cmd.Parameters = new MoveYCommandParameters(start, end);
                    }
                    break;

                case Event.Loop:
                    break;
                case Event.Trigger:
                    break;
                case Event.HorizonFlip:
                    break;
                case Event.VerticalFlip:
                    break;
                case Event.AdditiveBlend:
                    break;
                default:
                    break;
            }

            #endregion

            return cmd;
        }

        public static StoryBoardObject ParseStoryBoardObject(string line)
        {
            StoryBoardObject obj = null;
            string[] sprite_param = line.Split(',');

            #region ObjectType

            switch (sprite_param[0].Trim())
            {
                case "Sprite":
                    obj = new StoryBoardObject();
                    break;

                case "Animation":
                    obj = new StoryboardAnimation();
                    break;

                default:
                    return null;
            }

            #endregion

            #region Layer

            obj.layout = (Layout)Enum.Parse(typeof(Layout), sprite_param[1]);

            #endregion

            #region Anchor

            obj.Anchor = GetAnchorVector((Anchor)Enum.Parse(typeof(Anchor), sprite_param[2]));

            #endregion

            #region Origin

            #endregion

            #region ImageFilePath

            obj.ImageFilePath = sprite_param[3].Trim(',').Trim('\"', ' ').Replace("/", "\\").ToLower();

            #endregion

            #region InitPosition

            obj.Postion = new Vector(float.Parse(sprite_param[4]), float.Parse(sprite_param[5]));

            #endregion

            if (obj is StoryboardAnimation animation)
            {
                #region Animation Setup

                #region Frame

                int dot_position = animation.ImageFilePath.LastIndexOf('.');
                animation.FrameFileExtension = animation.ImageFilePath.Substring(dot_position);
                animation.FrameBaseImagePath = animation.ImageFilePath.Replace(animation.FrameFileExtension, string.Empty);

                #endregion

                #region FrameCount

                animation.FrameCount = int.Parse(sprite_param[6]);

                #endregion

                #region FrameDelay

                animation.FrameDelay = float.Parse(sprite_param[7]);

                #endregion

                #region LoopType

                animation.LoopType = (LoopType)Enum.Parse(typeof(LoopType), sprite_param[8]);

                #endregion

                #endregion
            }

            return obj;
        }

        static Vector GetAnchorVector(Anchor anchor)
        {
            switch (anchor)
            {
                case Anchor.TopLeft:
                    return new Vector(0, 0);
                case Anchor.TopCentre:
                    return new Vector(0.5f, 0.0f);
                case Anchor.TopRight:
                    return new Vector(1.0f, 0.0f);
                case Anchor.CentreLeft:
                    return new Vector(0.0f, 0.5f);
                case Anchor.Centre:
                    return new Vector(0.5f, 0.5f);
                case Anchor.CentreRight:
                    return new Vector(1.0f, 0.5f);
                case Anchor.BottomLeft:
                    return new Vector(0.0f, 1.0f);
                case Anchor.BottomCentre:
                    return new Vector(0.5f, 1.0f);
                case Anchor.BottomRight:
                    return new Vector(1.0f, 1.0f);
                default:
                    return new Vector(0.5f,0.5f);
            }
        }

    }

    public static class StoryBoardAdjustment
    {
        static Event[] SkipEvent = new[] 
        {
            Event.Parameter,
            Event.HorizonFlip,
            Event.AdditiveBlend,
            Event.VerticalFlip
        };

        public static (Dictionary<Event,List<Command>> cmd_map,int start_time, int end_time) AdujustCommands(List<Command> command_list)
        {
            Dictionary<Event, List<Command>> result = new Dictionary<Event, List<Command>>();

            int frame_start_time = int.MinValue, frame_end_time = int.MaxValue;

            if (command_list == null)
                return (result,0,0);

            foreach (var command in command_list)
            {
                if (!result.ContainsKey(command.CommandEventType))
                {
                    result[command.CommandEventType] = new List<Command>();
                }

                result[command.CommandEventType].Add(command);

                if (command is LoopCommand)
                {
                    //特殊调整
                    AdjustLoopCommand((LoopCommand)command);
                }

                if (!SkipEvent.Contains(command.CommandEventType))
                {
                    if (int.MinValue == frame_start_time || frame_start_time > command.StartTime)
                    {
                        frame_start_time = command.StartTime;
                    }

                    if (int.MaxValue == frame_end_time || frame_end_time < command.EndTime)
                    {
                        frame_end_time = command.EndTime;
                    }

                }
            }

            foreach (var pair in result)
            {
                pair.Value.Sort((a, b)=> a.StartTime-b.StartTime);
            }

            return (result, frame_start_time, frame_end_time);
        }

        static void AdjustLoopCommand(LoopCommand loop_command)
        {
            var total_command_list = loop_command.LoopParamesters.LoopCommandList.Values.SelectMany(l => l).OrderBy(c => c.StartTime);

            int first_start_time = total_command_list.First().StartTime;
            
            int fix_start_time = total_command_list.First().StartTime;
            
            //因为Loop子命令是可以有offset的，所以在这就把那些子命令减去共同的offset
            foreach (var sub_cmd in total_command_list)
            {
                sub_cmd.StartTime -= fix_start_time;
                sub_cmd.EndTime -= fix_start_time;
            }

            /*
            for (int index = 0; index < total_command_list.Count(); index++)
            {
                var sub_command = total_command_list.ElementAt(index);
                current_end_time += sub_command.EndTime - sub_command.StartTime;

                var prev_sub_command_time = index==0?0: total_command_list.Last().EndTime;
                current_end_time += sub_command.StartTime - prev_sub_command_time;
            }
            */

            loop_command.LoopParamesters.CostTime = total_command_list.Last().EndTime - total_command_list.First().StartTime;
            var total_cast_time = loop_command.LoopParamesters.CostTime * loop_command.LoopCount;

            loop_command.StartTime += first_start_time;
            loop_command.EndTime = (int)(loop_command.StartTime + total_cast_time);
        }
    }
}
