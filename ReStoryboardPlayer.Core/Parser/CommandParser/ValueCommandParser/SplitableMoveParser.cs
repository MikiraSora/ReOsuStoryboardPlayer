using ReOsuStoryBoardPlayer.Core.Commands;
using System.Collections.Generic;
using System.Linq;

namespace ReOsuStoryBoardPlayer.Core.Parser.CommandParser.ValueCommandParser
{
    public class SplitableMoveCommandParser : VectorCommandParser<MoveCommand>
    {
        public override IEnumerable<Command> Parse(IEnumerable<string> data_arr)
        {
            return base.Parse(data_arr).OfType<MoveCommand>().Select(p => SplitCommand(p)).SelectMany(l => l);
        }

        private IEnumerable<Command> SplitCommand(MoveCommand move)
        {
            var x = _get<MoveXCommand>(move);
            x.StartValue=move.StartValue.X;
            x.EndValue=move.EndValue.X;
            yield return x;

            var y = _get<MoveYCommand>(move);
            y.StartValue=move.StartValue.Y;
            y.EndValue=move.EndValue.Y;
            yield return y;

            T _get<T>(ValueCommand c) where T : ValueCommand, new()
            {
                return new T()
                {
                    StartTime=c.StartTime,
                    EndTime=c.EndTime,
                    Easing=c.Easing,
                };
            }
        }
    }
}