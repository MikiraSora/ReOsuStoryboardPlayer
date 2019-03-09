using ReOsuStoryboardPlayer.Core.Base;
using ReOsuStoryboardPlayer.Core.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ReOsuStoryboardPlayer.Core.Commands.Group
{
    public class LoopCommand : GroupCommand
    {
        public LoopCommand() => Event=Event.Loop;

        public int LoopCount { get; set; }

        public override void Execute(StoryboardObject @object, float current_value)
        {
            //咕咕哒
        }

        public override void UpdateSubCommand()
        {
            var commands = SubCommands.SelectMany(l => l.Value);

            var offset = commands.Count()==0 ? 0 : commands.Min(x => x.StartTime);
            StartTime+=offset;
            foreach (var command in commands)
            {
                command.StartTime-=offset;
                command.EndTime-=offset;
            }

            CostTime=commands.Count()==0 ? 0 : commands.Max(c => c.EndTime);
            var total_cast_time = CostTime*LoopCount;
            EndTime=StartTime+total_cast_time;
        }

        public IEnumerable<Command> SubCommandExpand()
        {
            var offset = SubCommands.SelectMany(l => l.Value).Min(x => x.StartTime);

            foreach (var timeline in SubCommands.Values)
            {
                var cost_time=CostTime-offset;
                var start_time=StartTime+offset;

                for (int i = 0; i<LoopCount; i++)
                {
                    foreach (var cmd in timeline)
                    {
                        var type = cmd.GetType();
                        var new_cmd = (Command)type.Assembly.CreateInstance(type.FullName);

                        if (cmd is ValueCommand value_cmd)
                        {
                            var start_value_prop = type.GetField("StartValue");
                            var end_value_prop = type.GetField("EndValue");

                            ((ValueCommand)new_cmd).Easing=value_cmd.Easing;
                            end_value_prop.SetValue(new_cmd, end_value_prop.GetValue(value_cmd));
                            start_value_prop.SetValue(new_cmd, start_value_prop.GetValue(value_cmd));
                        }

                        //calculate time
                        new_cmd.StartTime=start_time+(cost_time*i)+cmd.StartTime;
                        new_cmd.EndTime=start_time+(cost_time*i)+cmd.EndTime;

                        new_cmd.RelativeLine=cmd.RelativeLine;

                        yield return new_cmd;
                    }
                }
            }
        }

        public override string ToString() => $"{base.ToString()} (Times:{LoopCount} CostPerLoop:{CostTime})";

        public override void OnSerialize(BinaryWriter stream, StringCacheTable cache)
        {
            base.OnSerialize(stream, cache);

            CostTime.OnSerialize(stream);
            LoopCount.OnSerialize(stream);
        }

        public override void OnDeserialize(BinaryReader stream, StringCacheTable cache)
        {
            base.OnDeserialize(stream, cache);

            var x = CostTime;
            x.OnDeserialize(stream); CostTime=x;
            x.OnDeserialize(stream); LoopCount=x;

            UpdateSubCommand();
        }

        public override bool Equals(Command command)
        {
            return base.Equals(command)
                &&command is LoopCommand loop
                &&loop.LoopCount==LoopCount;
        }
    }
}