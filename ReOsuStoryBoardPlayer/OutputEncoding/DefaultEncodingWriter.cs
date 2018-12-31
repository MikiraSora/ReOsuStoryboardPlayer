using ReOsuStoryBoardPlayer.Kernel;
using ReOsuStoryBoardPlayer.Player;
using Saar.FFmpeg.CSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ReOsuStoryBoardPlayer.OutputEncoding
{
    public class DefaultEncodingWriter : EncodingWriterBase
    {
        MediaWriter writer;
        VideoFrame video_frame;
        AudioFrame audio_frame;

        Encoder video_encoder, audio_encoder;
        AudioDecoder audio_decoder;

        MediaReader audio_reader;

        EncoderOption option;

        public override long ProcessedFrameCount => video_encoder.InputFrames;
        public override TimeSpan ProcessedTimestamp => video_encoder.InputTimestamp;

        Thread audio_encoding_thread;
        private Packet outAudioPacket = new Packet();
        private Packet outVideoPacket = new Packet();

        private void Clean()
        {
            if (writer!=null)
            {
                writer.Dispose();
                writer=null;
            }
        }

        public override void OnFinish()
        {
            lock (this)
            {
                Clean();
            }

            Log.User("Wait for audio encoding...");
            audio_encoding_thread.Join();
        }

        public override void OnNextFrame(byte[] buffer, int width, int height)
        {
            if (writer==null)
                return;

            System.Diagnostics.Debug.Assert(buffer.Length==video_frame.Format.Bytes);
            video_frame.Update(buffer);

            lock (this)
            {
                writer.Write(video_frame);
            }

            Log.User($"{video_encoder.FullName} ---> ({video_encoder.InputFrames}) {video_encoder.InputTimestamp}");
        }

        public override void OnStart(EncoderOption option)
        {
            this.option=option;

            var audio_file = StoryboardInstanceManager.ActivityInstance?.Info?.audio_file_path;
            audio_reader=new MediaReader(audio_file);

            audio_decoder=audio_reader.Decoders.OfType<AudioDecoder>().FirstOrDefault();


            #region Video Init

            var video_format = new VideoFormat(option.Width, option.Height, AVPixelFormat.Bgra);
            var video_param = new VideoEncoderParameters() { FrameRate=new Fraction(option.FPS), BitRate=option.BitRate };

            video_encoder=new VideoEncoder(option.EncoderName, video_format, video_param);

            #endregion

            writer=new MediaWriter(option.OutputPath, false).AddEncoder(video_encoder);

            if (audio_decoder!=null)
            {
                audio_encoder=new AudioEncoder(audio_decoder.ID, audio_decoder.OutFormat);
                writer.AddEncoder(audio_encoder);
            }

            writer.Initialize();

            Log.User($"Format :{video_format.ToString()}\nVideo Encoder :{video_encoder.ToString()}");

            video_frame=new VideoFrame(video_format);
            audio_frame=new AudioFrame(audio_decoder.OutFormat);

            audio_encoding_thread=new Thread(AudioEncoding);
            audio_encoding_thread.Start();
        }

        double pos;

        private void AudioEncoding()
        {
            //skip start_time if IsExplicitTimeRange=true
            int calc_start_time=0, calc_end_time= (int)MusicPlayerManager.ActivityPlayer.Length;

            if (option.IsExplicitTimeRange)
            {
                calc_end_time=option.EndTime;
                calc_start_time=option.StartTime;

                while (audio_reader.NextFrame(audio_frame, audio_decoder.StreamIndex))
                {
                    pos=audio_reader.Position.TotalMilliseconds;

                    if (pos>=option.StartTime)
                        break;
                }
            }

            while (true)
            {
                if (pos>=MusicPlayerManager.ActivityPlayer.CurrentTime)
                    continue;
                    
                if (!audio_reader.NextFrame(audio_frame, audio_decoder.StreamIndex))
                    break;

                pos=audio_reader.Position.TotalMilliseconds;

                if (option.IsExplicitTimeRange&&(pos<option.StartTime||pos>option.EndTime))
                    break;

                lock (this)
                {
                    writer.Write(audio_frame);
                }

                Log.User($"{audio_encoder.FullName} ---> ({audio_encoder.InputFrames}) {audio_encoder.InputTimestamp}");
            }

            Log.User($"Finish audio encoding...");
        }
    }
}
