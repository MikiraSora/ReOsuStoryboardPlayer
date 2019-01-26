using System;

namespace ReOsuStoryboardPlayer.Core.Base
{
    public class StoryboardAnimation : StoryboardObject
    {
        public int FrameCount;

        public float FrameDelay;

        public string FrameBaseImagePath, FrameFileExtension;

        public LoopType LoopType;

        private int prev_frame_index = -2857;

        public override void Update(float current_time)
        {
            base.Update(current_time);

            float current_frame_index = (current_time-FrameStartTime)/FrameDelay;

            current_frame_index=(int)(LoopType==LoopType.LoopForever ? (current_frame_index%FrameCount) : Math.Min(current_frame_index, FrameCount-1));

            int result = Math.Max(0, (int)current_frame_index);

            if (prev_frame_index!=result)
            {
                ImageFilePath=FrameBaseImagePath+result+FrameFileExtension;
            }

            prev_frame_index=result;
        }
    }
}