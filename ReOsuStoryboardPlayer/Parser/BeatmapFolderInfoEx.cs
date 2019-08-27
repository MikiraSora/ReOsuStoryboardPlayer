using ReOsuStoryboardPlayer.Core.Parser;
using ReOsuStoryboardPlayer.Core.Parser.Reader;
using ReOsuStoryboardPlayer.Core.Parser.Stream;
using ReOsuStoryboardPlayer.Core.Utils;
using ReOsuStoryboardPlayer.ProgramCommandParser;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReOsuStoryBoardPlayer.Parser
{
    public class BeatmapFolderInfoEx : BeatmapFolderInfo
    {
        public string osu_file_path { get; private set; }

        public string audio_file_path { get; protected set; }

        protected BeatmapFolderInfoEx()
        {

        }

        public static BeatmapFolderInfoEx Parse(string folder_path, Parameters args)
        {
            string explicitly_osu_diff_name = "";

            if (args != null && args.TryGetArg("diff", out var diff_name))
                explicitly_osu_diff_name = diff_name;

            var info = BeatmapFolderInfo.Parse<BeatmapFolderInfoEx>(folder_path);

            if (!string.IsNullOrWhiteSpace(explicitly_osu_diff_name))
            {
                int index = -1;
                index = int.TryParse(explicitly_osu_diff_name, out index) ? index : -1;

                if (index > 0)
                {
                    info.osu_file_path = info.DifficultFiles.OrderBy(x => x.Key).FirstOrDefault().Value;
                }
                else
                {
                    info.osu_file_path = info.DifficultFiles.Where(x => x.Key.Contains(explicitly_osu_diff_name)).OrderBy(x => x.Key.Length).FirstOrDefault().Value;
                }
            }
            else
            {
                //优先选std铺面的.一些图其他模式谱面会有阻挡 53925 fripSide - Hesitation Snow
                info.osu_file_path = info.DifficultFiles.FirstOrDefault(x =>
                {
                    var lines = File.ReadAllLines(x.Value);

                    foreach (var line in lines)
                    {
                        if (line.StartsWith("Mode"))
                        {
                            try
                            {
                                var mode = line.Split(':').Last().ToInt();

                                if (mode == 0)
                                    return true;
                            }
                            catch { }
                        }
                    }

                    return false;
                }).Value;

                if (!File.Exists(info.osu_file_path))
                    info.osu_file_path = info.DifficultFiles.FirstOrDefault().Value;
            }

            if ((!string.IsNullOrWhiteSpace(info.osu_file_path)) && File.Exists(info.osu_file_path))
            {
                info.reader = new OsuFileReader(info.osu_file_path);
                var section = new SectionReader(Section.General, info.reader);

                info.audio_file_path = Path.Combine(folder_path, section.ReadProperty("AudioFilename"));
                Log.User($"audio file path={info.audio_file_path}");

                var wideMatch = section.ReadProperty("WidescreenStoryboard");

                if (!string.IsNullOrWhiteSpace(wideMatch))
                    info.IsWidescreenStoryboard = wideMatch.ToInt() == 1;
            }

            /*
            if (string.IsNullOrWhiteSpace(info.osu_file_path) || (!File.Exists(info.osu_file_path)))
            {
                info.audio_file_path = Directory
                    .GetFiles(info.folder_path, "*.mp3")
                    .Select(x => new FileInfo(x))
                    .OrderByDescending(x => x.Length)
                    .FirstOrDefault()
                    ?.FullName;
            }
            */

            if (string.IsNullOrWhiteSpace(info.osu_file_path) || !File.Exists(info.osu_file_path))
                Log.Warn("No .osu load");

            if (string.IsNullOrWhiteSpace(info.audio_file_path) || !File.Exists(info.audio_file_path))
                throw new Exception("Audio file not found.");

            return info;
        }
    }
}
