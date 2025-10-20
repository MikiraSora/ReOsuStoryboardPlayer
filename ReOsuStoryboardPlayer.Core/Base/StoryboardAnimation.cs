using System;
using System.IO;
using CommunityToolkit.HighPerformance.Buffers;
using ReOsuStoryboardPlayer.Core.Serialization;

namespace ReOsuStoryboardPlayer.Core.Base
{
    public class StoryboardAnimation : StoryboardObject
    {
        public string FrameBaseImagePath, FrameFileExtension;
        public int FrameCount;

        public float FrameDelay;

        public LoopType LoopType;

        private int prev_frame_index = -2857;

        public override void Update(float current_time)
        {
            base.Update(current_time);

            var current_frame_index = (current_time - FrameStartTime) / FrameDelay;

            current_frame_index = (int) (LoopType == LoopType.LoopForever
                ? current_frame_index % FrameCount
                : Math.Min(current_frame_index, FrameCount - 1));

            var result = Math.Max(0, (int) current_frame_index);

            if (prev_frame_index != result)
                ImageFilePath = StringPool.Shared.GetOrAdd(FrameBaseImagePath + result + FrameFileExtension);

            prev_frame_index = result;
        }

        public override void OnSerialize(BinaryWriter stream, StringCacheTable cache)
        {
            base.OnSerialize(stream, cache);

            FrameCount.OnSerialize(stream);
            FrameDelay.OnSerialize(stream);
            FrameBaseImagePath.OnSerialize(stream);
            FrameFileExtension.OnSerialize(stream);
            ((byte) LoopType).OnSerialize(stream);
        }

        public override void OnDeserialize(BinaryReader stream, StringCacheTable cache)
        {
            base.OnDeserialize(stream, cache);

            FrameCount.OnDeserialize(stream);
            FrameDelay.OnDeserialize(stream);
            FrameBaseImagePath = StringPool.Shared.GetOrAdd(stream.ReadString());
            FrameFileExtension = StringPool.Shared.GetOrAdd(stream.ReadString());
            LoopType = (LoopType) stream.ReadByte();
        }

        public override bool Equals(StoryboardObject other)
        {
            return base.Equals(other) && other is StoryboardAnimation;
        }
    }
}