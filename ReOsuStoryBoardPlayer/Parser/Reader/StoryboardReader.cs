using ReOsuStoryBoardPlayer.Base;
using ReOsuStoryBoardPlayer.Commands;
using ReOsuStoryBoardPlayer.Parser.Base;
using ReOsuStoryBoardPlayer.Parser.Extension;
using ReOsuStoryBoardPlayer.Parser.Stream;
using ReOsuStoryBoardPlayer.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
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
        
        static internal float _parse_ave => _parse_total / _parse_c;
        static internal int _parse_c;
        static internal long _parse_max;
        static internal float _parse_total;
        static internal Stopwatch _sw = new Stopwatch();

        public IEnumerable<StoryBoardObject> GetValues()
        {
            _sw.Start();

            return Reader.GetStoryboardPackets().Select(p => 
            {
                _sw.Restart();

                var obj = ParsePacket(p);

                _parse_c++;
                _parse_total += _sw.ElapsedMilliseconds;
                _parse_max = Math.Max(_parse_max, _sw.ElapsedMilliseconds);

                return obj;
            });
        }

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

        /*
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
        */

        private readonly static byte[] SPLIT = new byte[] { 0x2c };
        
        public StoryBoardObject ParseObjectLine(ReadOnlyMemory<byte> line)
        {
            StoryBoardObject obj = null;

            var data_arr = line.Split(SPLIT);

            if ((!Enum.TryParse<StoryboardObjectType>(data_arr[0].GetContentString(), true, out var obj_type)) || !(obj_type == StoryboardObjectType.Background || obj_type == StoryboardObjectType.Animation || obj_type == StoryboardObjectType.Sprite))
                throw new Exception($"Unknown/Unsupport storyboard object type:" + data_arr[0].GetContentString());

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
                obj.layout = (Layout)Enum.Parse(typeof(Layout), data_arr[1].GetContentString());

                obj.Anchor = GetAnchorVector((Anchor)Enum.Parse(typeof(Anchor), data_arr[2].GetContentString()));

                obj.ImageFilePath = data_arr[3].GetContentString().Trim().Trim('\"').ToString().Replace("/", "\\").ToLower();

                obj.Postion = new Vector(data_arr[4].ToSigle(), data_arr[5].ToSigle());

                if (obj is StoryboardAnimation animation)
                    ParseStoryboardAnimation(animation, data_arr);
            }
            else
            {
                //For background object
                obj.ImageFilePath = data_arr[2].GetContentString().Trim().Trim('\"').ToString().Replace("/", "\\").ToLower();

                obj.Z = -1;

                var position = data_arr.Length > 4 ? new Vector(data_arr[3].ToSigle(),data_arr[4].ToSigle()) : Vector.Zero;

                if (position != Vector.One)
                    obj.Postion = position + new Vector(320, 240);
            }

            return obj;
        }
        
        private void ParseStoryboardAnimation(StoryboardAnimation animation, ReadOnlyMemory<byte>[] sprite_param)
        {
            int dot_position = animation.ImageFilePath.LastIndexOf('.');
            animation.FrameFileExtension = animation.ImageFilePath.Substring(dot_position);
            animation.FrameBaseImagePath = animation.ImageFilePath.Replace(animation.FrameFileExtension, string.Empty);

            animation.FrameCount = sprite_param[6].ToInt();

            animation.FrameDelay = sprite_param[7].ToSigle();

            animation.LoopType = (LoopType)Enum.Parse(typeof(LoopType), sprite_param[8].GetContentString());
        }

        private readonly static Dictionary<Anchor, Vector> AnchorVectorMap = new Dictionary<Anchor, Vector>()
        {
            {Anchor.TopLeft,new Vector(0,0)},
            {Anchor.TopCentre,new Vector(0.5f, 0.0f)},
            {Anchor.TopRight,new Vector(1.0f, 0.0f)},
            {Anchor.CentreLeft,new Vector(0.0f, 0.5f)},
            {Anchor.Centre,new Vector(0.5f, 0.5f)},
            {Anchor.CentreRight,new Vector(1.0f, 0.5f)},
            {Anchor.BottomLeft,new Vector(0.0f, 1.0f)},
            {Anchor.BottomCentre,new Vector(0.5f, 1.0f)},
            {Anchor.BottomRight,new Vector(1.0f, 1.0f)}
        };

        public static Vector GetAnchorVector(Anchor anchor) => AnchorVectorMap.TryGetValue(anchor, out var vector) ? vector : AnchorVectorMap[Anchor.Centre];
        
        private Dictionary<Event, CommandTimeline> BuildCommandMap(List<ReadOnlyMemory<byte>> lines)
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

        private readonly static byte[][] CMD_PREFIX = new[] { Encoding.UTF8.GetBytes("__"), Encoding.UTF8.GetBytes("  ") };
        public List<Command> ParseCommand(List<ReadOnlyMemory<byte>> lines)
        {
            List<Command> commands = new List<Command>(), temp = ObjectPool<List<Command>>.Instance.GetObject(), cur_group_cmds = ObjectPool<List<Command>>.Instance.GetObject();

            GroupCommand current_group_command = null;

            foreach (var line in lines)
            {
                var data_arr = line.Split(SPLIT);

                var is_sub_cmd = data_arr.First().StartsWith(CMD_PREFIX[0]) || data_arr.First().StartsWith(CMD_PREFIX[1]);

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