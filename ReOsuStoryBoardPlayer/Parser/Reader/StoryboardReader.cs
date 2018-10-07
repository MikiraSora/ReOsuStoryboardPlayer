using ReOsuStoryBoardPlayer.Base;
using ReOsuStoryBoardPlayer.Commands;
using ReOsuStoryBoardPlayer.Parser.Base;
using ReOsuStoryBoardPlayer.Parser.Extension;
using ReOsuStoryBoardPlayer.Parser.Stream;
using ReOsuStoryBoardPlayer.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static ReOsuStoryBoardPlayer.Parser.Stream.EventReader;

namespace ReOsuStoryBoardPlayer.Parser.Reader
{
    public class StoryboardReader : IReader<StoryBoardObject>
    {
        public bool IsEnd => Reader.EndOfStream;

        public EventReader Reader { get; }

        public StoryboardReader(EventReader reader)
        {
            Reader = reader;
        }

        public IEnumerable<StoryBoardObject> GetValues(int thread_count)
        {
            /*
            if (thread_count == 0)
                foreach (var packet in Reader.GetStoryboardPackets())
                {
                    var o = ParsePacket(packet);
                    if (o != null)
                        yield return o;
                }
            else
            {
                Task<StoryBoardObject>[] tasks = new Task<StoryBoardObject>[thread_count];

                for (int i = 0; i < tasks.Length; i++)
                {
                    tasks[i] = new Task<StoryBoardObject>(GetPacketAndParse, TaskCreationOptions.PreferFairness);
                    tasks[i].Start();
                }

                //已经读完SB文本并且全部任务执行完毕,才给跳出循环
                while ((!Reader.EndOfStream) && (!tasks.Any(t => t.IsCompleted)))
                {
                    var completed_task = tasks.FirstOrDefault(t => t.IsCompleted);

                    if (completed_task == null)
                        continue;

                    var storyboard_obj = completed_task.Result;

                    //流都没读完，赶出去继续跑
                    if (!Reader.EndOfStream)
                        completed_task.Start();

                    if (storyboard_obj != null)
                        yield return storyboard_obj;
                }
            }
            */

            return Reader.GetStoryboardPackets().AsParallel().Select(p => ParsePacket(p));
        }

        private StoryBoardObject GetPacketAndParse()
        {
            //maybe be locked
            var packet = Reader.GetStoryboardPacket();

            return ParsePacket(packet);
        }

        public IEnumerable<StoryBoardObject> GetValues() => GetValues(0);

        private StoryBoardObject ParsePacket(StoryboardPacket packet)
        {
            try
            {
                var storyboard_object = ParseObjectLine(packet.ObjectLine);

                if (storyboard_object != null)
                {
                    var commands = BuildCommandMap(packet.CommandLines);

                    if (commands == null)
                        Log.Warn($"Storyboard object {packet.ObjectLine.ToString()} in section offset {packet.ObjectFileLine} not exist any commands.");
                    else
                        foreach (var command in commands.Values)
                            storyboard_object.AddCommand(command);

                    storyboard_object.FileLine = packet.ObjectFileLine;

                    storyboard_object.UpdateObjectFrameTime();
                }

                Reader.ReturnPacket(ref packet);
                return storyboard_object;
            }
            catch (Exception e)
            {
                Log.Debug($"Cant parse storyboard packet.{e.Message}");
                return null;
            }
        }

        #region Packet Parse

        public StoryBoardObject ParseObjectLine(string line)
        {
            StoryBoardObject obj = null;

            var data_arr = line.Split(',');

            if ((!Enum.TryParse<StoryboardObjectType>(data_arr[0].ToString(), true, out var obj_type)) || !(obj_type == StoryboardObjectType.Background||obj_type == StoryboardObjectType.Animation || obj_type == StoryboardObjectType.Sprite))
                throw new Exception($"Unknown/Unsupport storyboard object type:" + data_arr[0]);

            switch (obj_type)
            {
                case StoryboardObjectType.Background:
                    obj = new StoryboardBackgroundObject();
                    break;

                case StoryboardObjectType.Sprite:
                    obj = new StoryBoardObject();
                    break;

                case StoryboardObjectType.Animation:
                    obj = new StoryboardAnimation();
                    break;

                default:
                    break;
            }

            if (!(obj is StoryboardBackgroundObject))
            {
                obj.layout = (Layout)Enum.Parse(typeof(Layout), data_arr[1].ToString());

                obj.Anchor = GetAnchorVector((Anchor)Enum.Parse(typeof(Anchor), data_arr[2].ToString()));

                obj.ImageFilePath = data_arr[3].Trim().Trim('\"').ToString().Replace("/", "\\").ToLower();

                obj.Postion = new Vector(float.Parse(data_arr[4].ToString()), float.Parse(data_arr[5].ToString()));

                if (obj is StoryboardAnimation animation)
                    ParseStoryboardAnimation(animation, data_arr);
            }
            else
            {
                //For background object
                obj.ImageFilePath = data_arr[2].Trim().Trim('\"').ToString().Replace("/", "\\").ToLower();

                obj.Z = -1;

                var position= data_arr.Length>4? new Vector(float.Parse(data_arr[3].ToString()), float.Parse(data_arr[4].ToString())):Vector.Zero;

                if (position != Vector.One)
                    obj.Postion = position + new Vector(320, 240);
            }

            return obj;
        }

        private void ParseStoryboardAnimation(StoryboardAnimation animation, string[] sprite_param)
        {
            int dot_position = animation.ImageFilePath.LastIndexOf('.');
            animation.FrameFileExtension = animation.ImageFilePath.Substring(dot_position);
            animation.FrameBaseImagePath = animation.ImageFilePath.Replace(animation.FrameFileExtension, string.Empty);

            animation.FrameCount = int.Parse(sprite_param[6].ToString());

            animation.FrameDelay = float.Parse(sprite_param[7].ToString());

            animation.LoopType = (LoopType)Enum.Parse(typeof(LoopType), sprite_param[8].ToString());
        }

        public static Vector GetAnchorVector(Anchor anchor)
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
                    return new Vector(0.5f, 0.5f);
            }
        }

        private Dictionary<Event, CommandTimeline> BuildCommandMap(List<string> lines)
        {
            var list = ParseCommand(lines);

            Dictionary<Event, CommandTimeline> map = new Dictionary<Event, CommandTimeline>();

            foreach (var cmd in list)
            {
                if (!map.ContainsKey(cmd.Event))
                    map[cmd.Event] = cmd.Event == Event.Loop ? new LoopCommandTimeline() : new CommandTimeline();
                map[cmd.Event].Add(cmd);
            }

            return map;
        }

        public List<Command> ParseCommand(List<string> lines)
        {
            List<Command> commands = new List<Command>(), temp = ObjectPool<List<Command>>.Instance.GetObject(), cur_group_cmds = ObjectPool<List<Command>>.Instance.GetObject();

            GroupCommand current_group_command = null;

            foreach (var line in lines)
            {
                var data_arr = line.ToString().Split(',');

                var is_sub_cmd = data_arr.First().StartsWith("  ") || data_arr.First().StartsWith("__");

                foreach (var cmd in CommandParserIntance.Parse(data_arr, temp))
                {
                    if (is_sub_cmd)
                    {
                        //如果是子命令的话就要添加到当前Group
                        if (current_group_command != null)
                        {
                            Log.Debug($"add subCommand \"{cmd.ToString()}\" to Loop \"{current_group_command.ToString()}\"");
                            current_group_command.AddSubCommand(cmd);
                        }
                    }
                    else
                    {
                        var prev_group = current_group_command;
                        current_group_command = cmd as GroupCommand;

                        if (current_group_command != prev_group &&
                            prev_group is LoopCommand loopc)
                        {
                            loopc.UpdateParam();
                        }

                        commands.Add(cmd);
                    }
                }

                temp.Clear();
            }

            if (current_group_command is LoopCommand loop)
                loop.UpdateParam();

            ObjectPool<List<Command>>.Instance.PutObject(temp);
            ObjectPool<List<Command>>.Instance.PutObject(cur_group_cmds);

            return commands;
        }

        #endregion Packet Parse
    }
}