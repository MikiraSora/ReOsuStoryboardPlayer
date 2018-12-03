using ReOsuStoryBoardPlayer.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReOsuStoryBoardPlayer.Parser.CommandParser.ValueCommandParser
{
    public class SplitableMoveCommandParser: VectorCommandParser<MoveCommand>
    {
        public override IEnumerable<Command> Parse(IEnumerable<string> data_arr)
        {
            return base.Parse(data_arr).OfType<MoveCommand>().Select(p=>SplitCommand(p)).SelectMany(l=>l);
        }

        private IEnumerable<Command> SplitCommand(MoveCommand move)
        {
            var x = _get<MoveXCommand>(move);
            x.StartValue=move.StartValue.x;
            x.EndValue=move.EndValue.x;
            yield return x;

            var y = _get<MoveYCommand>(move);
            y.StartValue=move.StartValue.y;
            y.EndValue=move.EndValue.y;
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
