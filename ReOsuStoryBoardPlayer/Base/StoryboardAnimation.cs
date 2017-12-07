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

        public override void Update(float current_time)
        {
            base.Update(current_time);

            int current_frame_index = 0;

            switch (LoopType)
            {
                case LoopType.LoopOnce:
                    {
                        if (current_time >= (FrameStartTime + FrameCount * FrameDelay))
                        {
                            current_frame_index = FrameCount;
                        }
                        else
                        {
                            var offset_time = (FrameStartTime + FrameCount * FrameDelay) - current_time;
                            current_frame_index = FrameCount-(int)(offset_time / FrameDelay)-1;
                        }
                    }
                    break;
                case LoopType.LoopForever:
                    {
                        if (current_time<FrameStartTime)
                        {
                            current_frame_index = 0;
                        }
                        else
                        {
                            var offset_time = current_time - FrameStartTime;
                            offset_time %= FrameCount * FrameDelay;
                            current_frame_index = (int)(offset_time / FrameDelay);
                        }
                    }
                    break;
                default:
                    break;
            }

            ImageFilePath = FrameBaseImagePath + current_frame_index + ".png";
        }
    }
}
