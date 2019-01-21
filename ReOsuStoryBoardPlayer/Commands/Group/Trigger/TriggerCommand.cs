using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace ReOsuStoryBoardPlayer.Commands.Group.Trigger
{
    public class TriggerCommand : GroupCommand
    {
        public TriggerConditionBase Condition { get; set; }

        private Dictionary<Event, TriggerSubTimelineCommand> cache_timline_wrapper = new Dictionary<Event, TriggerSubTimelineCommand>();

        private StoryBoardObject bind_object;

        public int GroupID;

        public bool Trigged { get; private set; }

        public int CostTime { get; private set; }

        private float last_trigged_time = 0;

        public TriggerCommand(TriggerConditionBase condition)
        {
            Event=Event.Trigger;
            Condition=condition??throw new ArgumentNullException(nameof(condition));
        }

        public override void Execute(StoryBoardObject @object, float time)
        {
            if (Trigged)
            {
                //executed,recovery status and reset
                if (last_trigged_time+CostTime<=time)
                {
                    Trigged=false;
                    Reset(true);
                }
            }
        }

        /// <summary>
        /// 钦定触发器绑定的物件，一次性的
        /// </summary>
        /// <param name="obj"></param>
        public void BindObject(StoryBoardObject obj)
        {
            Debug.Assert(bind_object==null, "Not allow trigger command bind more storyboard objects");

            bind_object=obj??throw new ArgumentNullException(nameof(obj));
        }

        /// <summary>
        /// 钦定触发器激活，并将子命令塞到物件里面去
        /// </summary>
        /// <param name="time"></param>
        public void Trig(float time)
        {
            Reset(true);
            last_trigged_time=time;

            AttachSubCommands(time);

            //todo:优化掉这货
            bind_object.SortCommands();

            bind_object.CalculateAndApplyBaseFrameTime();

            Trigged=true;
        }

        /// <summary>
        /// 将子命令塞到物件那里执行
        /// </summary>
        /// <param name="time"></param>
        private void AttachSubCommands(float time)
        {
            foreach (var wrapper in cache_timline_wrapper)
            {
                bind_object.InternalRemoveCommand(wrapper.Value);
                wrapper.Value.UpdateOffset((int)time);
                bind_object.InternalAddCommand(wrapper.Value);
            }
        }

        /// <summary>
        /// 清除物件内自己的子命令
        /// </summary>
        /// <param name="magic"></param>
        private void DetachSubCommands(bool magic = false)
        {
            foreach (var wrapper in cache_timline_wrapper.Where(x => !magic||(x.Value.StartTime==x.Value.EndTime&&x.Value.StartTime==0)))
                bind_object.InternalRemoveCommand(wrapper.Value);
        }

        /// <summary>
        /// 清除物件内自己的子命令，重置触发器状态
        /// </summary>
        /// <param name="magic">是否要也要清除物件内，同Group组所有Trigger子命令，
        /// 为啥是magic呢，因为我看不出这有啥实际意义，而且现在屙屎的Trigger逻辑细节和以前不同，也没文档跟上，只能按照基本法膜法一下了</param>
        public void Reset(bool magic = false)
        {
            DetachSubCommands(magic);

            last_trigged_time=0;

            Trigged=false;
        }

        /// <summary>
        /// 检查时间是否处于触发器可触发时间内
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public bool CheckTimeVaild(float time)
        {
            return StartTime<=time&&time<=EndTime;
        }

        public override void UpdateSubCommand()
        {
            base.UpdateSubCommand();

            CostTime=SubCommands.Values.SelectMany(l => l).Max(p => p.EndTime);

            cache_timline_wrapper.Clear();

            foreach (var timeline in SubCommands)
                cache_timline_wrapper[timeline.Key]=new TriggerSubTimelineCommand(this, timeline.Key);
        }

        public override string ToString() => $"{base.ToString()} {Condition} {(Trigged ? $"Trigged at {last_trigged_time} ~ end:{last_trigged_time+CostTime}" : "")}";

        /// <summary>
        /// 有触发器的物件默认不显示(之前默认显示),不懂ppy想法，magic
        /// 比如440423的歌词
        /// </summary>
        public readonly static Action<StoryBoardObject> OverrideDefaultValue = obj => obj.Color.w=0;
    }
}