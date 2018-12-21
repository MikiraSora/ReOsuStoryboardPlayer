using Saar.FFmpeg.CSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace ReOsuStoryBoardPlayer.OutputEncoding
{
    public class DefaultEncodingWriter : EncodingWriterBase
    {
        MediaWriter writer;
        VideoFrame frame;
        Encoder encoder;

        public override long ProcessedFrameCount => encoder.InputFrames;
        public override TimeSpan ProcessedTimestamp => encoder.InputTimestamp;


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
            if (writer==null)
                return;

            System.Diagnostics.Debug.Assert(buffer.Length==frame.Format.Bytes);
            frame.Update(buffer);

            Log.User($"{encoder.FullName} ---> ({encoder.InputFrames}) {encoder.InputTimestamp}");
            writer.Write(frame);
        }

        public override void OnStart(EncoderOption option)
        {
            var format = new VideoFormat(option.Width, option.Height, AVPixelFormat.Bgra);
            var param = new VideoEncoderParameters() { FrameRate=new Fraction(option.FPS), BitRate=option.BitRate };

            encoder=new VideoEncoder("h264_nvenc", format, param);

            writer=new MediaWriter(option.OutputPath, false).AddEncoder(encoder).Initialize();

            Log.User($"Format :{format.ToString()}\nVideo Encoder :{encoder.ToString()}");

            frame=new VideoFrame(format);
        }
    }
}
