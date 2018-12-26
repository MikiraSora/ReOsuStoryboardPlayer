using ReOsuStoryBoardPlayer.Commands;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using OpenTK;
using ReOsuStoryBoardPlayer.Graphics;

namespace ReOsuStoryBoardPlayer
{
    public class StoryBoardObject
    {
        internal Dictionary<Event, CommandTimeline> CommandMap = new Dictionary<Event, CommandTimeline>();

        public string ImageFilePath;

        public int FrameStartTime, FrameEndTime;

        public SpriteInstanceGroup RenderGroup;

        public Layout layout;

        public int Z = -1;

        public bool IsVisible { get; set; }

        #region Transform

        public Vector Postion = new Vector(320, 240), Scale = new Vector(1, 1);

        public ByteVec4 Color = new ByteVec4(255,255,255,255);

        public float Rotate = 0;

        public HalfVector Anchor = new HalfVector(0f, 0f);

        public bool IsAdditive = false, IsHorizonFlip = false, IsVerticalFlip = false;

        #endregion Transform

        #region Add Command

        public void AddCommand(Command command)
        {
            if (command is LoopCommand loop)
            {
                AddLoopCommand(loop);
                //这里不用return是因为还要再Visualizer显示这个Loop命令，方便调试，Loop::Execute(...)已被架空
            }

            if (!CommandMap.TryGetValue(command.Event, out var timeline))
                timeline = CommandMap[command.Event] = new CommandTimeline();
            timeline.Add(command);
        }

        private void AddLoopCommand(LoopCommand loop_command)
        {
            if (Setting.EnableLoopCommandExpand)
            {
                //将Loop命令各个类型的子命令时间轴封装成一个命令，并添加到物件本体各个时间轴上
                foreach (var cmd in loop_command.SubCommandExpand())
                    AddCommand(cmd);
            }
            else
            {
                //将Loop命令里面的子命令展开
                foreach (var @event in loop_command.SubCommands.Keys)
                {
                    var sub_command_wrapper = new LoopSubTimelineCommand(loop_command, @event);
                    AddCommand(sub_command_wrapper);
                }
            }

        }

        public void SortCommands()
        {
            foreach (var time in CommandMap.Values)
                time.Sort((x,y) => 
                {
                    var z = x.StartTime-y.StartTime;

                    if (z!=0)
                        return z;

                    return x.EndTime-y.EndTime;
                });
        }

        #endregion

        public virtual void Update(float current_time)
        {
#if DEBUG
            ExecutedCommands.ForEach(c => c.IsExecuted=false);
            ExecutedCommands.Clear();
#endif
            foreach (var timeline in CommandMap.Values)
            {
                var command = timeline.PickCommand(current_time);

                command.Execute(this, current_time);
#if DEBUG
                MarkCommandExecuted(command);
#endif
            }

            IsVisible = (Color.w != 0);
            //if (IsVisible) IsVisible = CalculateVisible(); //没必要了
        }

        private const float DEG2RAD = 0.017453292519943295f;
        private bool CalculateVisible()
        {
            if (RenderGroup == null) return false;

            float offset = (StoryboardWindow.CurrentWindow.ViewWidth - StoryboardWindow.SB_WIDTH) * 0.5f;

            float xstart = -offset;
            float xend = StoryboardWindow.SB_WIDTH + offset;

            float w = RenderGroup.Texture.Width * Scale.x;
            float h = RenderGroup.Texture.Height * Scale.y;
            Vector2 anchor = new Vector2(Anchor.x + 0.5f, Anchor.y + 0.5f);
            anchor.X *= w;
            anchor.Y *= h;

            Vector2[] vertices = new Vector2[4];

            vertices[0].X = 0;
            vertices[0].Y = 0;

            vertices[1].X = w;
            vertices[1].Y = 0;

            vertices[2].X = w;
            vertices[2].Y = h;

            vertices[3].X = 0;
            vertices[3].Y = h;

            float rotate = Rotate * DEG2RAD;
            float cosa = (float)Math.Cos(rotate);
            float sina = (float)Math.Sin(rotate);

            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i].X -= anchor.X;
                vertices[i].Y -= anchor.Y;

                vertices[i].X = vertices[i].X * cosa + vertices[i].Y * sina;
                vertices[i].Y = vertices[i].X * sina - vertices[i].Y * cosa;

                vertices[i].X += Postion.x;
                vertices[i].Y += Postion.y;
            }

            //构造AABB
            float minX = float.MaxValue;
            float minY = float.MaxValue;
            float maxX = float.MinValue;
            float maxY = float.MinValue;

            foreach (var v in vertices)
            {
                minX = v.X < minX ? v.X : minX;
                minY = v.Y < minY ? v.Y : minY;
                maxX = v.X > maxX ? v.X : maxX;
                maxY = v.Y > maxY ? v.Y : maxY;
            }

            bool collisionX = maxX >= xstart && xend >= minX;
            bool collisionY = maxY >= 0 && StoryboardWindow.SB_HEIGHT >= minY;

            return collisionX && collisionY;
        }

#if DEBUG

        internal List<Command> ExecutedCommands = new List<Command>();

        internal bool DebugShow = true;

        internal void MarkCommandExecuted(Command command, bool is_exec = true)
        {
            if (is_exec)
                ExecutedCommands.Add(command);
            else
                ExecutedCommands.Remove(command);

            command.IsExecuted = is_exec;
        }

#endif

        public void UpdateObjectFrameTime()
        {
            var commands = CommandMap.SelectMany(l => l.Value);

            if (commands.Count() == 0)
                return;

            FrameStartTime = commands.Min(p => p.StartTime);
            FrameEndTime = commands.Max(p => p.EndTime);
        }

        public long FileLine { get; set; }

        public override string ToString() => $"line {FileLine} ({layout.ToString()} {Z}): {ImageFilePath} : {FrameStartTime}~{FrameEndTime}";
    }
}
 