using System;

namespace ReOsuStoryBoardPlayer.OutputEncoding
{
    public abstract class EncodingWriterBase
    {
        public abstract long ProcessedFrameCount { get; }
        public abstract TimeSpan ProcessedTimestamp { get; }

        public abstract void OnStart(EncoderOption option);

        public abstract void OnFinish();

        public unsafe abstract void OnNextFrame(byte[] buffer, int width, int height);
    }
}