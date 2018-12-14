using Saar.FFmpeg.CSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReOsuStoryBoardPlayer.OutputEncoding
{
    public class DefaultEncodingWriter : EncodingWriterBase
    {
        MediaWriter writer;
        VideoFrame frame;

        private void Clean()
        {
            if (writer!=null)
            {
                writer.Dispose();
                writer=null;
            }
        }

        public override void OnAbort()
        {
            Clean();
        }

        public override void OnFinish()
        {
            Clean();
        }

        public override void OnNextFrame(byte[] buffer, int width, int height)
        {
            System.Diagnostics.Debug.Assert(width==frame.Format.Width&&height==frame.Format.Height);
            frame.Update(buffer);
            writer.Write(frame);
        }

        public override void OnStart(EncoderOption option)
        {
            var format = new VideoFormat(option.Width, option.Height, AVPixelFormat.Rgba);
            var encoder = new VideoEncoder(AVCodecID.H264, format, 
                new VideoEncoderParameters() { FrameRate=new Fraction(option.FPS),BitRate=option.BitRate });

            writer=new MediaWriter(option.OutputPath,true);
            writer.AddEncoder(encoder);
            writer.Initialize();

            Log.User($"Format :{format.ToString()}\nEncoder :{encoder.ToString()}");

            frame=new VideoFrame(format);
        }
    }
}
