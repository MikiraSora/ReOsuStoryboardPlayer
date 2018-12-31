using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReOsuStoryBoardPlayer.OutputEncoding
{
    public unsafe struct Frame
    {
        public byte* ImagePtr;
        public uint Width;
        public uint Height;
        
        //todo
        //public PixelFormat Type;
    }
}
