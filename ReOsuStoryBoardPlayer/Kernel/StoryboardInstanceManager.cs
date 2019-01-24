using ReOsuStoryBoardPlayer.Core.Kernel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReOsuStoryBoardPlayer.Kernel
{
    public static class StoryboardInstanceManager
    {
        public static StoryboardInstance ActivityInstance { get; private set; }

        public static void ApplyInstance(StoryboardInstance instance)
        {
            ActivityInstance=instance;
        }
    }
}
