using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReOsuStoryBoardPlayer.Kernel
{
    public static class StoryboardInstanceManager
    {
        public static StoryBoardInstance ActivityInstance { get; private set; }

        public static void ApplyInstance(StoryBoardInstance instance)
        {
            ActivityInstance=instance;
        }
    }
}
