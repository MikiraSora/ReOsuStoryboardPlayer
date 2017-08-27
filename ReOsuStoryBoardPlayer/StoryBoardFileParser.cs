using SimpleRenderFramework;
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
        public static List<StoryBoardObject> ParseFromOsbFile(string osb_file_path)
        {
            using (var reader = File.OpenText(osb_file_path))
            {
                return ParseFromStream(reader);
            }
        }

        public static List<StoryBoardObject> ParseFromOsuFile(string osu_file_path)
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

                return ParseFromStream(reader);
            }
        }

        static List<StoryBoardObject> ParseFromStream(StreamReader reader)
        {
            List<StoryBoardObject> obj_list = new List<StoryBoardObject>();
            List<Command> current_command = new List<Command>();
            
            StoryBoardObject current_storyboard_obj = null;

            int command_count = 0;
            int frame_start_time = int.MaxValue, frame_end_time = int.MaxValue, z_order=0;
            bool isSubCommand = false;

            LoopCommand current_loop_command = null;

            while (!reader.EndOfStream)
            {
                string line = reader.ReadLine();

                if (line.StartsWith("//") || line.StartsWith("[") || string.IsNullOrWhiteSpace(line))
                    continue;

                //for osu file
                if (line == "[TimingPoints]")
                    break;

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
                            current_loop_command.LoopParamesters.LoopCommandList.Add(cmd);
                        }

                        continue;
                    }

                    if (int.MaxValue == frame_start_time || frame_start_time > cmd.StartTime)
                    {
                        frame_start_time = cmd.StartTime;
                    }

                    command_count++;

                    //Loop 中断
                    if (cmd is LoopCommand)
                    {
                        current_loop_command = (LoopCommand)cmd;
                        current_command.Add(cmd);
                        continue;
                    }

                    if (int.MaxValue == frame_end_time || frame_end_time < cmd.EndTime)
                    {
                        frame_end_time = cmd.EndTime;
                    }

                    current_command.Add(cmd);
                }
                else
                {
                    //try parse sprite/anamition obj
                    StoryBoardObject obj = ParseStoryBoardObject(line);

                    if (obj != null)
                    {
                        obj.Z = z_order++;

                        if (current_storyboard_obj != null)
                        {
                            current_storyboard_obj.CommandMap = StoryBoardAdjustment.AdujustCommands(current_command);
                            current_storyboard_obj.FrameStartTime = frame_start_time;
                            current_storyboard_obj.FrameEndTime = frame_end_time;
                        }

                        frame_end_time = frame_start_time = int.MaxValue;

                        //set current storyboard obj
                        current_storyboard_obj = obj;
                        obj_list.Add(obj);

                        current_command.Clear();
                    }
                }
            }

            if (current_storyboard_obj != null)
            {
                current_storyboard_obj.CommandMap = StoryBoardAdjustment.AdujustCommands(current_command);
                current_storyboard_obj.FrameStartTime = frame_start_time==int.MaxValue? int.MinValue : frame_start_time;
                current_storyboard_obj.FrameEndTime = frame_end_time;
            }

            Log.Debug($"parsed {obj_list.Count} objects and {command_count} commands");

            return obj_list;
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

                case "T":
                case "P":
                    return null;
                case "L":
                    var loop_cmd = new LoopCommand();
                    loop_cmd.Easing = EasingConverter.CacheEasingInterpolatorMap[Easing.Linear];
                    loop_cmd.CommandEventType = Event.Loop;
                    loop_cmd.StartTime = int.Parse(command_params[1]);
                    loop_cmd.CurrentLoopCount = int.Parse(command_params[2]);
                    loop_cmd.executor = CommandExecutor.CommandFunctionMap[Event.Loop];
                    return loop_cmd;
                default:
                    break;
            }

            cmd.executor = CommandExecutor.CommandFunctionMap[cmd.CommandEventType];

            #endregion

            #region Easing

            int easingID = int.Parse(command_params[1].Trim());

            cmd.Easing = EasingConverter.CacheEasingInterpolatorMap[((Easing)easingID)];
            
            #endregion

            #region Start-End Time

            cmd.StartTime = int.Parse(command_params[2]);

            cmd.EndTime = string.IsNullOrWhiteSpace(command_params[3]) ? cmd.StartTime : int.Parse(command_params[3]);

            #endregion

            #region Params

            switch (cmd.CommandEventType)
            {
                case Event.Move:
                    Vector start_pos = new Vector();
                    start_pos.x = float.Parse(command_params[4]);
                    start_pos.y = float.Parse(command_params[5]);

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
                        Vector start = new Vector();
                        start.x = float.Parse(command_params[4]);
                        start.y = float.Parse(command_params[5]);

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
                        Vec4 start = new Vec4();
                        start.x = float.Parse(command_params[4]);
                        start.y = float.Parse(command_params[5]);
                        start.z = float.Parse(command_params[6]);

                        Vec4 end = start.clone();

                        if (command_params.Length > 7)
                        {
                            end.x = float.Parse(command_params[7]);
                            end.y = float.Parse(command_params[8]);
                            end.z = float.Parse(command_params[9]);
                        }

                        cmd.Parameters = new ColorCommandParameters(start, end);
                    }
                    break;

                case Event.Parameter:
                    break;
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
                default:
                    break;
            }

            #endregion

            return cmd;
        }

        public static StoryBoardObject ParseStoryBoardObject(string line)
        {
            StoryBoardObject obj=null;
            string[] sprite_param = line.Split(',');

            #region ObjectType

            switch (sprite_param[0].Trim())
            {
                case "Sprite":
                    obj = new StoryBoardObject();
                    break;

                case "Animation":
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

            obj.ImageFilePath = sprite_param[3].Trim(',').Trim('\"',' ').Replace("/","\\").ToLower();

            #endregion

            #region InitPosition

            obj.Postion = new Vector(float.Parse(sprite_param[4]), float.Parse(sprite_param[5]));

            #endregion

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
        public static Dictionary<Event,List<Command>> AdujustCommands(List<Command> command_list)
        {
            Dictionary<Event, List<Command>> result = new Dictionary<Event, List<Command>>();

            if (command_list == null)
                return result;

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
            }

            foreach (var pair in result)
            {
                pair.Value.Sort((a, b)=> a.StartTime-b.StartTime);
            }

            return result;
        }

        static void AdjustLoopCommand(LoopCommand loop_command)
        {
            int current_end_time = loop_command.StartTime;

            for (int i = 0; i < loop_command.CurrentLoopCount; i++)
            {
                foreach (var sub_command in loop_command.LoopParamesters.LoopCommandList)
                {
                    current_end_time += sub_command.EndTime;
                }
            }

            loop_command.EndTime = current_end_time;
            loop_command.LoopParamesters.CostTime =(uint)((loop_command.EndTime - loop_command.StartTime) / loop_command.CurrentLoopCount);
        }
    }
}
