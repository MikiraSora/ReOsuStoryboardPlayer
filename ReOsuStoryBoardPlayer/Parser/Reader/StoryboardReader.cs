using ReOsuStoryBoardPlayer.Parser.Base;
using ReOsuStoryBoardPlayer.Parser.Extension;
using ReOsuStoryBoardPlayer.Parser.Stream;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Buffers.Binary;
using System.Buffers.Text;
using System.Buffers;
using static ReOsuStoryBoardPlayer.Parser.Stream.EventReader;

namespace ReOsuStoryBoardPlayer.Parser.Reader
{
    public class StoryboardReader : IReader<StoryBoardObject>
    {
        private readonly static Event[] SkipEvent = new[]
        {
            Event.Parameter,
            Event.HorizonFlip,
            Event.AdditiveBlend,
            Event.VerticalFlip
        };

        public bool IsEnd => Reader.EndOfStream;

        public EventReader Reader { get; }

        public StoryboardReader(EventReader reader)
        {
            Reader = reader;
        }

        public IEnumerable<StoryBoardObject> GetValues(int thread_count)
        {
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
                while ((!Reader.EndOfStream)&&(!tasks.Any(t=>t.IsCompleted)))
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
        }
        
        private StoryBoardObject GetPacketAndParse()
        {
            //maybe be locked
            var packet=Reader.GetStoryboardPacket();

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
                    storyboard_object.CommandMap = commands ?? throw new Exception($"Storyboard object {packet.ObjectLine.ToString()} in section offset {packet.ObjectFileLine} not exist any commands. ignore.");

                    storyboard_object.FileLine = packet.ObjectFileLine;

                    var vaild_commands = commands.Where(v => !SkipEvent.Contains(v.Key)).SelectMany(l => l.Value);

                    storyboard_object.FrameStartTime = vaild_commands.Min(p => p.StartTime);
                    storyboard_object.FrameEndTime = vaild_commands.Max(p => p.EndTime);
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

        public StoryBoardObject ParseObjectLine(ReadOnlyMemory<char> line)
        {
            StoryBoardObject obj = null;

            var data_arr = line.Split(',');

            if ((!Enum.TryParse<StoryboardObjectType>(data_arr[0].ToString(), true, out var obj_type) ) || !(obj_type == StoryboardObjectType.Animation || obj_type == StoryboardObjectType.Sprite)) 
                throw new Exception($"Unknown/Unsupport storyboard object type:" + data_arr[0]);

            switch (obj_type)
            {
                case StoryboardObjectType.Sprite:
                    obj = new StoryBoardObject();
                    break;
                case StoryboardObjectType.Animation:
                    obj = new StoryboardAnimation();
                    break;
                default:
                    break;
            }
            
            obj.layout = (Layout)Enum.Parse(typeof(Layout), data_arr[1].ToString());

            obj.Anchor = GetAnchorVector((Anchor)Enum.Parse(typeof(Anchor), data_arr[2].ToString()));

            obj.ImageFilePath = data_arr[3].Span.Trim().Trim('\"').ToString().Replace("/", "\\").ToLower();

            obj.Postion = new Vector(float.Parse(data_arr[4].ToString()), float.Parse(data_arr[5].ToString()));

            if (obj is StoryboardAnimation animation)
                ParseStoryboardAnimation(animation, data_arr);

            return obj;
        }

        private void ParseStoryboardAnimation(StoryboardAnimation animation, ReadOnlyMemory<char>[] sprite_param)
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

        private Dictionary<Event,List<Command>> BuildCommandMap(List<ReadOnlyMemory<char>> lines)
        {
            return null;
        }

        public (bool is_sub_cmd,Command command) ParseCommand(List<ReadOnlyMemory<char>> line)
        {
            var data_arr = line.ToString().Split(',');
            (bool is_sub_cmd, Command command) result;

            result.is_sub_cmd = data_arr.First().StartsWith("  ") || data_arr.First().StartsWith("__");

            return default;
        }

        #endregion
    }
}
