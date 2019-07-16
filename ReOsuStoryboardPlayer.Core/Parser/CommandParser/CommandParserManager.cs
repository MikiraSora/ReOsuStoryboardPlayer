using ReOsuStoryboardPlayer.Core.Commands;
using ReOsuStoryboardPlayer.Core.Commands.Group;
using ReOsuStoryboardPlayer.Core.Commands.Group.Trigger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ReOsuStoryboardPlayer.Core.Parser.CommandParser
{
    public class CommandParserIntanceBuilder
    {
        private Dictionary<string, ICommandParser> ext_map=new Dictionary<string, ICommandParser>();

        public CommandParserIntanceBuilder Parser { get; private set; }

        private CommandParserIntanceBuilder() { }

        public static CommandParserIntanceBuilder CreateDefault()
        {
            var builder = new CommandParserIntanceBuilder();

            builder.AddExtraCommandParser(new (string,ICommandParser)[] {
                ("M",CommandParserIntanceDefaultImplement<MoveCommand>.Instance),
                ("S",CommandParserIntanceDefaultImplement<ScaleCommand>.Instance),
                ("V",CommandParserIntanceDefaultImplement<VectorScaleCommand>.Instance),
                ("MX",CommandParserIntanceDefaultImplement<MoveYCommand>.Instance),
                ("MY",CommandParserIntanceDefaultImplement<MoveYCommand>.Instance),
                ("R",CommandParserIntanceDefaultImplement<RotateCommand>.Instance),
                ("F",CommandParserIntanceDefaultImplement<FadeCommand>.Instance),
                ("P",CommandParserIntanceDefaultImplement<StateCommand>.Instance),
                ("L",CommandParserIntanceDefaultImplement<LoopCommand>.Instance),
                ("C",CommandParserIntanceDefaultImplement<ColorCommand>.Instance),
                ("T",CommandParserIntanceDefaultImplement<TriggerCommand>.Instance)
            });

            return builder;
        }

        public CommandParserIntanceBuilder AddExtraCommandParser(params (string,ICommandParser)[] ext_parser)
        {
            foreach (var item in ext_parser)
                ext_map.Add(item.Item1,item.Item2);

            return this;
        }

        public CommandParserIntanceBuilder AddExtraCommandParser(params IExtensionCommandParser[] ext_parser)
        {
            foreach (var item in ext_parser.Select(x => x.SupportPrefix.Select(y => (y, x)))
                .SelectMany(l => l))
                ext_map.Add(item.y, item.x);

            return this;
        }

        public CommandParserIntance Build()
        {
            return new CommandParserIntance(ext_map);
        }
    }
}
