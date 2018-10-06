using ReOsuStoryBoardPlayer.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReOsuStoryBoardPlayer.Base
{
    class StoryboardBackgroundObject : StoryBoardObject
    {
        public StoryboardBackgroundObject()
        {
            AddCommand(new FadeCommand() {
                StartTime=int.MinValue,
                EndTime = int.MinValue,
                StartValue= 1,
                EndValue = 1
            });

            /*todo
            AddCommand(new ScaleCommand()
            {
                StartTime = int.MinValue,
                EndTime = int.MinValue,
                StartValue = 1,
                EndValue = 1
            });
            */
        }
    }
}
