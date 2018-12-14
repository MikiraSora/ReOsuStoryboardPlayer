using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReOsuStoryBoardPlayer.OutputEncoding
{
    public abstract class EncodingWriterBase
    {
        public abstract void OnStart(EncoderOption option);
        public abstract void OnFinish();
        public abstract void OnAbort();
        public unsafe abstract void OnNextFrame(byte[] buffer,int width,int height);
    }
}
