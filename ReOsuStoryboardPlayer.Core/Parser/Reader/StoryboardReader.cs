using ReOsuStoryboardPlayer.Core.Base;
using ReOsuStoryboardPlayer.Core.Commands;
using ReOsuStoryboardPlayer.Core.Commands.Group;
using ReOsuStoryboardPlayer.Core.Parser.Base;
using ReOsuStoryboardPlayer.Core.Parser.CommandParser;
using ReOsuStoryboardPlayer.Core.PrimitiveValue;
using ReOsuStoryboardPlayer.Core.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ReOsuStoryboardPlayer.Core.Parser.Reader
{
    public class StoryboardReader : IReader<StoryboardObject>
    {
        public EventReader Reader { get; }
        public CommandParserIntance Parser { get; }

        /// <summary>
        /// 读取文本并解析成对应的SB物件/命令
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="manager">会默认新建</param>
        public StoryboardReader(EventReader reader,CommandParserIntance parser=null)
        {
            Reader=reader;
            Parser = parser ?? CommandParserIntanceBuilder.CreateDefault().Build();
        }

        public IEnumerable<StoryboardObject> EnumValues()
        {
            return Reader.EnumValues().Select(p => ParsePacket(p));
        }

        private ParallelOptions parallel_options = new ParallelOptions() { MaxDegreeOfParallelism=Setting.UpdateThreadCount };

        private StoryboardObject ParsePacket(StoryboardPacket packet)
        {
            //try
            {
                var Storyboard_object = ParseObjectLine(packet.ObjectLine);

                if (Storyboard_object!=null)
                {
                    Storyboard_object.FileLine=packet.ObjectFileLine;

                    BuildCommandMapAndSetup(Storyboard_object, packet.CommandLines);
                }

                Reader.ReturnPacket(ref packet);
                return Storyboard_object;
            }
            /*
            catch (Exception e)
            {
                Log.Debug($"Cant parse Storyboard packet.{e.Message}");
                return null;
            }*/
        }

        #region Packet Parse

        public StoryboardObject ParseObjectLine(string line)
        {
            StoryboardObject obj = null;

            var data_arr = line.Split(',');

            if ((!Enum.TryParse<StoryboardObjectType>(data_arr[0], true, out var obj_type))||!(obj_type==StoryboardObjectType.Background||obj_type==StoryboardObjectType.Animation||obj_type==StoryboardObjectType.Sprite))
            {
                Log.Warn($"Unknown/Unsupport Storyboard object type:"+data_arr[0]);
                return null;
            }

            switch (obj_type)
            {
                case StoryboardObjectType.Background:
                    obj=new StoryboardBackgroundObject();
                    break;

                case StoryboardObjectType.Sprite:
                    obj=new StoryboardObject();
                    break;

                case StoryboardObjectType.Animation:
                    obj=new StoryboardAnimation();
                    break;

                default:
                    break;
            }

            if (!(obj is StoryboardBackgroundObject))
            {
                obj.layout=(Layout)Enum.Parse(typeof(Layout), data_arr[1]);

                obj.Anchor=GetAnchorVector((Anchor)Enum.Parse(typeof(Anchor), data_arr[2]));

                obj.ImageFilePath=data_arr[3].Trim().Trim('\"').ToString().Replace("/", "\\").ToLower();

                obj.BaseTransformResetAction+=(x) => x.Postion=new Vector(data_arr[4].ToSigle(), data_arr[5].ToSigle());

                if (obj is StoryboardAnimation animation)
                    ParseStoryboardAnimation(animation, data_arr);
            }
            else
            {
                //For background object
                obj.ImageFilePath=data_arr[2].Trim().Trim('\"').ToString().Replace("/", "\\").ToLower();

                var position = data_arr.Length>4 ? new Vector(data_arr[3].ToSigle(), data_arr[4].ToSigle()) : Vector.Zero;

                if (position!=Vector.One)
                    obj.BaseTransformResetAction+=(x) => x.Postion=position+new Vector(320, 240);
            }

            return obj;
        }

        private void ParseStoryboardAnimation(StoryboardAnimation animation, string[] sprite_param)
        {
            int dot_position = animation.ImageFilePath.LastIndexOf('.');
            animation.FrameFileExtension=animation.ImageFilePath.Substring(dot_position);
            animation.FrameBaseImagePath=animation.ImageFilePath.Replace(animation.FrameFileExtension, string.Empty);

            animation.FrameCount=sprite_param[6].ToInt();

            animation.FrameDelay=sprite_param[7].ToSigle();

            animation.LoopType=(LoopType)Enum.Parse(typeof(LoopType), sprite_param[8]);
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

        private void BuildCommandMapAndSetup(StoryboardObject obj, List<string> lines)
        {
            var list = lines.Count>=Setting.ParallelParseCommandLimitCount&&Setting.ParallelParseCommandLimitCount!=0 ?
                ParallelParseCommands(lines, obj.FileLine) :
                ParseCommands(lines, obj.FileLine);

            obj.AddCommandRange(list);
        }

        public List<Command> ParseCommands(List<string> lines, long base_line)
        {
            List<Command> commands = new List<Command>(), cur_group_cmds = ObjectPool<List<Command>>.Instance.GetObject();

            GroupCommand current_group_command = null;

            foreach (var line in lines)
            {
                var data_arr = line.Split(',');

                var is_sub_cmd = data_arr.First().StartsWith("  ")||data_arr.First().StartsWith("__");

                foreach (var cmd in Parser.Parse(data_arr))
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

            if (current_group_command is GroupCommand loop)
                loop.UpdateSubCommand();

            ObjectPool<List<Command>>.Instance.PutObject(cur_group_cmds);

            return commands;
        }

        public IEnumerable<Command> ParallelParseCommands(List<string> lines, long base_line)
        {
            ConcurrentBag<(long index, Command[] cmds, bool is_sub)> result_list = new ConcurrentBag<(long index, Command[] cmds, bool is_sub)>();

            Parallel.For(0, lines.Count, parallel_options, i =>
            {
                var file_line = i+base_line;
                var line = lines[i];
                var data_arr = line.Split(',');
                var is_sub_cmds = data_arr.First().StartsWith("  ")||data_arr.First().StartsWith("__");

                var temp_list = Parser.Parse(data_arr);

                foreach (var c in temp_list)
                    c.RelativeLine=file_line;

                result_list.Add((file_line, temp_list.ToArray(), is_sub_cmds));
            });

            var result = result_list.SelectMany(p => p.cmds.Select(cmd => (p.index, cmd, p.is_sub))).OrderBy(z => z.index);
            var sub_cmds = result.Where(x => x.is_sub);
            var groups = result.Where(x => x.cmd is GroupCommand&&!x.is_sub).Select(x => x.cmd).OfType<GroupCommand>();
            var fin_list = result.Except(sub_cmds.Where(sub_cmd =>
            {
                var r = groups.LastOrDefault(z => z.RelativeLine<sub_cmd.index);

                r?.AddSubCommand(sub_cmd.cmd);

                //return true and this sub_command will be removed (from main commands)
                return true;
            })).Select(p => p.cmd).ToList();

            foreach (var group in groups)
                group.UpdateSubCommand();

            return fin_list;
        }

        #endregion Packet Parse
    }
}