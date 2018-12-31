using ReOsuStoryBoardPlayer.Parser.Extension;
using ReOsuStoryBoardPlayer.ProgramCommandParser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReOsuStoryBoardPlayer.OutputEncoding
{
    public struct EncoderOption
    {
        //0 mean raw
        public int BitRate;

        public int Width;
        public int Height;
        public int FPS;

        public string OutputPath;
        public string EncoderName;

        public int StartTime;
        public int EndTime;

        public bool IsExplicitTimeRange => !(StartTime==EndTime&&StartTime==0);
        public bool RawRequire => BitRate==0;

        public EncoderOption(Parameters args)
        {
            Width=Setting.Width;
            Height=Setting.Height;

            FPS=args.TryGetArg(out var fps, "encoding_fps") ? fps.ToInt() : 60;

            StartTime=args.TryGetArg(out var start_time, "encoding_start") ? start_time.ToInt() : 0;
            EndTime=args.TryGetArg(out var end_time, "encoding_end") ? end_time.ToInt() : 0;

            BitRate=(args.TryGetArg(out var bit_rate, "encoding_bitrate") ? bit_rate.ToInt() : 12_0000)*1024;

            EncoderName=args.TryGetArg(out var encoder_name, "encoding_encoder_name") ? encoder_name : "libx264";
            OutputPath=args.TryGetArg(out var output_path, "encoding_output_path") ? output_path : "output.mp4";
        }
    }
}
