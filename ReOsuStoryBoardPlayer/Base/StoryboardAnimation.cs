using System;

namespace ReOsuStoryBoardPlayer
{
    public class StoryboardAnimation : StoryBoardObject
    {
        public int FrameCount;

        public float FrameDelay;

        public string FrameBaseImagePath, FrameFileExtension;

        public LoopType LoopType;

        public SpriteInstanceGroup[] backup_group;

        private int prev_frame_index = -2857;

        public override void Update(float current_time)
        {
            base.Update(current_time);

            float current_frame_index = (current_time - FrameStartTime) / FrameDelay;

            current_frame_index = (int)(LoopType == LoopType.LoopForever ? (current_frame_index % FrameCount) : Math.Min(current_frame_index, FrameCount - 1));

            int result = Math.Max(0, (int)current_frame_index);

            if (prev_frame_index != result)
            {
                ImageFilePath = FrameBaseImagePath + result + FrameFileExtension;
                this.RenderGroup = backup_group[result];
            }

            prev_frame_index = result;
        }
    }
}