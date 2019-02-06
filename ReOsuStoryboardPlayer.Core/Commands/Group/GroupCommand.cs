﻿using ReOsuStoryboardPlayer.Core.Base;
using ReOsuStoryboardPlayer.Core.Serialization;
using ReOsuStoryboardPlayer.Core.Serialization.DeserializationFactory;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ReOsuStoryboardPlayer.Core.Commands.Group
{
    public abstract class GroupCommand : Command
    {
        public Dictionary<Event, CommandTimeline> SubCommands { get; set; } = new Dictionary<Event, CommandTimeline>();

        public int CostTime;

        public virtual void AddSubCommand(Command command)
        {
            if (!SubCommands.ContainsKey(command.Event))
                SubCommands[command.Event]=new CommandTimeline();
            SubCommands[command.Event].Add(command);
        }

        public void AddSubCommand(IEnumerable<Command> commands)
        {
            foreach (var command in commands)
                AddSubCommand(command);
        }

        public abstract void UpdateSubCommand();

        public override void OnSerialize(BinaryWriter stream, Dictionary<string,uint> map)
        {
            base.OnSerialize(stream,map);

            var commands = SubCommands.Values.SelectMany(l => l);

            commands.Count().OnSerialize(stream);

            foreach (var command in commands)
                command.OnSerialize(stream,map);
        }

        public override void OnDeserialize(BinaryReader stream, Dictionary<uint, string> map)
        {
            base.OnDeserialize(stream,map);

            var count = stream.ReadInt32();

            for (int i = 0; i<count; i++)
            {
                var command = CommandDeserializtionFactory.Create(stream,map);
                AddSubCommand(command);
            }
        }

        public override bool Equals(Command command)
        {
            return base.Equals(command)
                && command is GroupCommand group
                && group.CostTime==CostTime
                //确保所有子命令命令都对应
                && group.SubCommands.Values.SelectMany(l=>l).All(x=>SubCommands.Values.SelectMany(l=>l).Any(y=>y.Equals(x)));
        }
    }
}