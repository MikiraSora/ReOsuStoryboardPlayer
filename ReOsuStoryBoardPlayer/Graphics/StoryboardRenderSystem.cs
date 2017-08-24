using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SimpleRenderFramework;

namespace ReOsuStoryBoardPlayer
{
    public class StoryboardRenderSystem : SystemBase, IRenderableSystem
    {
        StoryBoardInstance instance;

        public StoryboardRenderSystem(StoryBoardInstance instance) : base(10)
        {
            this.instance = instance;
        }

        public void onDraw(float dt)
        {
            instance.PostDrawStoryBoard();
        }
    }
}
