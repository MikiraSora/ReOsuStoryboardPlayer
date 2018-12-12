using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReOsuStoryBoardPlayer.Optimzer
{
    public abstract class OptimzerBase
    {
        public abstract void Optimze(IEnumerable<StoryBoardObject> storyboard_objects);
    }
}
