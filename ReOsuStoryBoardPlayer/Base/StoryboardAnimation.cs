using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReOsuStoryBoardPlayer
{
    public class StoryboardAnimation : StoryBoardObject
    {
        public int FrameDelay, FrameCount;

        public string FrameBaseImagePath;

        public LoopType LoopType;

        public SpriteInstanceGroup[] backup_group;

        int prev_frame_index=-2857;

        public override void Update(float current_time)
        {
            base.Update(current_time);

            float current_frame_index = (current_time - FrameStartTime) / FrameDelay;

            current_frame_index = (int)(LoopType == LoopType.LoopForever ? (current_frame_index % FrameCount) : Math.Min(current_frame_index, FrameCount - 1));

            int result = Math.Max(0,(int)current_frame_index);

            if (prev_frame_index!= result)
            {
                ImageFilePath = FrameBaseImagePath + result + ".png";
                this.RenderGroup = backup_group[result];
            }

            prev_frame_index = result;
        }
    }
}
