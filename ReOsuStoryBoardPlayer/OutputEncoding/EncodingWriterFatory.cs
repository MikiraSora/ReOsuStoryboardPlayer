﻿namespace ReOsuStoryboardPlayer.OutputEncoding
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