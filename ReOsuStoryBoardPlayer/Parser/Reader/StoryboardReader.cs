using ReOsuStoryBoardPlayer.Base;
using ReOsuStoryBoardPlayer.Commands;
using ReOsuStoryBoardPlayer.Parser.Base;
using ReOsuStoryBoardPlayer.Parser.CommandParser;
using ReOsuStoryBoardPlayer.Parser.Extension;
using ReOsuStoryBoardPlayer.Parser.Stream;
using ReOsuStoryBoardPlayer.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ReOsuStoryBoardPlayer.Graphics;
using ReOsuStoryBoardPlayer.Commands.Group;

namespace ReOsuStoryBoardPlayer.Parser.Reader
{
    public class StoryboardReader : IReader<StoryBoardObject>
    {
        public EventReader Reader { get; }

        public StoryboardReader(EventReader reader)
        {
            Reader = reader;
        }

        public IEnumerable<StoryBoardObject> EnumValues()
        {
            return Reader.EnumValues().Select(p => ParsePacket(p));
        }

        private StoryBoardObject ParsePacket(StoryboardPacket packet)
        {
            //try
            {
                var storyboard_object = ParseObjectLine(packet.ObjectLine);

                if (storyboard_object != null)
                {
                    storyboard_object.FileLine=packet.ObjectFileLine;

                    BuildCommandMapAndSetup(storyboard_object, packet.CommandLines);
                }

                Reader.ReturnPacket(ref packet);
                return storyboard_object;
            }
            /*
            catch (Exception e)
            {
                Log.Debug($"Cant parse storyboard packet.{e.Message}");
                return null;
            }*/
        }

        #region Packet Parse

        public StoryBoardObject ParseObjectLine(string line)
        {
            StoryBoardObject obj = null;

            var data_arr = line.Split(',');

            if ((!Enum.TryParse<StoryboardObjectType>(data_arr[0], true, out var obj_type)) || !(obj_type == StoryboardObjectType.Background || obj_type == StoryboardObjectType.Animation || obj_type == StoryboardObjectType.Sprite))
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
                obj.layout = (Layout)Enum.Parse(typeof(Layout), data_arr[1]);

                obj.Anchor = GetAnchorVector((Anchor)Enum.Parse(typeof(Anchor), data_arr[2]));

                obj.ImageFilePath = data_arr[3].Trim().Trim('\"').ToString().Replace("/", "\\").ToLower();

                obj.BaseTransformResetAction+=(x) => x.Postion=new Vector(data_arr[4].ToSigle(), data_arr[5].ToSigle());

                if (obj is StoryboardAnimation animation)
                    ParseStoryboardAnimation(animation, data_arr);
            }
            else
            {
                //For background object
                obj.ImageFilePath = data_arr[2].Trim().Trim('\"').ToString().Replace("/", "\\").ToLower();

                var position = data_arr.Length > 4 ? new Vector(data_arr[3].ToSigle(), data_arr[4].ToSigle()) : Vector.Zero;

                if (position!=Vector.One)
                    obj.BaseTransformResetAction+=(x) => x.Postion=position+new Vector(320, 240);
            }

            return obj;
        }

        private void ParseStoryboardAnimation(StoryboardAnimation animation, string[] sprite_param)
        {
            int dot_position = animation.ImageFilePath.LastIndexOf('.');
            animation.FrameFileExtension = animation.ImageFilePath.Substring(dot_position);
            animation.FrameBaseImagePath = animation.ImageFilePath.Replace(animation.FrameFileExtension, string.Empty);

            animation.FrameCount = sprite_param[6].ToInt();

            animation.FrameDelay = sprite_param[7].ToSigle();

            animation.LoopType = (LoopType)Enum.Parse(typeof(LoopType), sprite_param[8]);
        }

        private readonly static Dictionary<Anchor, HalfVector> AnchorVectorMap = new Dictionary<Anchor, HalfVector>()
        {
            {Anchor.TopLeft,new HalfVector(-0.5f,0.5f)},
            {Anchor.TopCentre,new HalfVector(0.0f, 0.5f)},
            {Anchor.TopRight,new HalfVector(0.5f, 0.5f)},
            {Anchor.CentreLeft,new HalfVector(-0.5f, 0.0f)},
            {Anchor.Centre,new HalfVector(0.0f, 0.0f)},
            {Anchor.CentreRight,new HalfVector(0.5f, 0.0f)},
            {Anchor.BottomLeft,new HalfVector(-0.5f, -0.5f)},
            {Anchor.BottomCentre,new HalfVector(0.0f, -0.5f)},
            {Anchor.BottomRight,new HalfVector(0.5f, -0.5f)}
        };

        public static HalfVector GetAnchorVector(Anchor anchor) => AnchorVectorMap.TryGetValue(anchor, out var vector) ? vector : AnchorVectorMap[Anchor.Centre];

        private void BuildCommandMapAndSetup(StoryBoardObject obj, List<string> lines)
        {
            var list = /*lines.Count >= Setting.ParallelParseCommandLimitCount ? ParallelParseCommands(lines) : */ ParseCommands(lines,obj.FileLine);

            foreach (var cmd in list)
                obj.AddCommand(cmd);
        }

        public List<Command> ParseCommands(List<string> lines,long base_line)
        {
            List<Command> commands = new List<Command>(), cur_group_cmds = ObjectPool<List<Command>>.Instance.GetObject();

            GroupCommand current_group_command = null;

            foreach (var line in lines)
            {
                var data_arr = line.Split(',');

                var is_sub_cmd = data_arr.First().StartsWith("  ")||data_arr.First().StartsWith("__");

                foreach (var cmd in CommandParserIntance.Parse(data_arr))
                {
                    cmd.RelativeLine=base_line++;

                    if (is_sub_cmd)
                    {
                        //如果是子命令的话就要添加到当前Group
                        if (current_group_command!=null)
                            current_group_command.AddSubCommand(cmd);
                    }
                    else
                    {
                        var prev_group = current_group_command;
                        current_group_command=cmd as GroupCommand;

                        if (current_group_command!=prev_group)
                        {
                            prev_group?.UpdateSubCommand();
                        }

                        commands.Add(cmd);
                    }
                }
            }

            if (current_group_command is LoopCommand loop)
                loop.UpdateSubCommand();

            ObjectPool<List<Command>>.Instance.PutObject(cur_group_cmds);

            return commands;
        }

        public IEnumerable<Command> ParallelParseCommands(List<string> lines)
        {
            System.Collections.Concurrent.ConcurrentBag<(int index, Command[] cmds, bool is_sub)> result_list = new System.Collections.Concurrent.ConcurrentBag<(int index, Command[] cmds, bool is_sub)>();

            Parallel.For(0, lines.Count, i =>
            {
                var line = lines[i];
                var data_arr = line.Split(',');
                var is_sub_cmds = data_arr.First().StartsWith("  ") || data_arr.First().StartsWith("__");

                var temp_list = CommandParserIntance.Parse(data_arr);

                result_list.Add((i, temp_list.ToArray(), is_sub_cmds));
            });

            var result = result_list.SelectMany(p => p.cmds.Select(cmd => (p.index, cmd, p.is_sub))).OrderBy(z => z.index);
            var sub_cmds = result.Where(x => x.is_sub);
            var fin_list = result.Except(sub_cmds.Where(sub_cmd =>
            {
                var r = result.FirstOrDefault(z => z.index == sub_cmd.index - 1);

                if (r.cmd is GroupCommand group)
                    group.AddSubCommand(sub_cmd.cmd);

                return true;
            })).Select(p => p.cmd);

            return fin_list;
        }

        #endregion Packet Parse
    }
}