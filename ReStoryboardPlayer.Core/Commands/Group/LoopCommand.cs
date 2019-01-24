﻿using ReOsuStoryboardPlayer.Core.Base;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ReOsuStoryboardPlayer.Core.Commands.Group
{
    internal class LoopCommand : GroupCommand
    {
        public LoopCommand() => Event=Event.Loop;

        public int CostTime { get; private set; }

        public int LoopCount { get; set; }

        public override void Execute(StoryboardObject @object, float current_value)
        {
            //咕咕哒
        }

        public override void UpdateSubCommand()
        {
            base.UpdateSubCommand();

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
            foreach (var list in SubCommands.Values)
            {
                var cost = list.Max(x => x.EndTime)-list.Min(x => x.StartTime);

                for (int i = 0; i<LoopCount; i++)
                {
                    foreach (var cmd in list)
                    {
                        if (!(cmd is ValueCommand value_cmd))
                            throw new Exception("SubCommand is not value command");

                        var type = value_cmd.GetType();
                        var start_value_prop = type.GetProperty("StartValue");
                        var end_value_prop = type.GetProperty("EndValue");

                        var new_cmd = (ValueCommand)type.Assembly.CreateInstance(type.FullName);

                        new_cmd.Easing=value_cmd.Easing;
                        end_value_prop.SetValue(new_cmd, end_value_prop.GetValue(value_cmd));
                        start_value_prop.SetValue(new_cmd, start_value_prop.GetValue(value_cmd));

                        //calculate time
                        new_cmd.StartTime=StartTime+(cost*i)+value_cmd.StartTime;
                        new_cmd.EndTime=StartTime+(cost*i)+value_cmd.EndTime;

                        yield return new_cmd;
                    }
                }
            }
        }

        public override string ToString() => $"{base.ToString()} (Times:{LoopCount} CostPerLoop:{CostTime})";
    }
}