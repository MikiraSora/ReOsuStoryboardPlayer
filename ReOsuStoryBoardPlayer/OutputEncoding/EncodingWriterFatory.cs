using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReOsuStoryBoardPlayer.OutputEncoding
{
    public static class EncodingWriterFatory
    {
        public static EncodingWriterBase Create()
        {
            //tood
            return new DefaultEncodingWriter();
        }
    }
}
