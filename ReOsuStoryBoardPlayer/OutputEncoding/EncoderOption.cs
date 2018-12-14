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
        public int Width;
        public int Height;
        public int BitRate;
        public int FPS;

        public string OutputPath;

        public EncoderOption(Parameters args)
        {
            Width=Setting.Width;
            Height=Setting.Height;

            FPS=args.TryGetArg(out var fps, "encoding_fps") ? fps.ToInt() : 120;
            BitRate=args.TryGetArg(out var bit_rate, "encoding_bitrate") ? bit_rate.ToInt() : 12_0000*1024;

            OutputPath=args.TryGetArg(out var output_path, "encoding_output_path") ? output_path : "output.mp4";
        }
    }
}
