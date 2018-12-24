using ReOsuStoryBoardPlayer.Parser.Extension;
using ReOsuStoryBoardPlayer.Parser.Reader;
using ReOsuStoryBoardPlayer.Parser.Stream;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ReOsuStoryBoardPlayer.Parser
{
    public class BeatmapFolderInfo
    {
        public string osu_file_path { get; private set; }

        public string osb_file_path { get; private set; }

        public string audio_file_path { get; private set; }

        public string folder_path { get; private set; }

        public OsuFileReader reader { get; private set; }

        public bool IsWidescreenStoryboard { get; private set; }

        private BeatmapFolderInfo() { }

        public static BeatmapFolderInfo Parse(string folder_path)
        {
            if (!Directory.Exists(folder_path))
                throw new Exception($"\"{folder_path}\" not a folder!");

            BeatmapFolderInfo info = new BeatmapFolderInfo();

            var osu_files = TryGetAnyFiles(".osu");

            //优先先选std铺面的.一些图其他模式谱面会有阻挡 53925 fripSide - Hesitation Snow
            var osu_file = osu_files.FirstOrDefault(x=> {
                var lines = File.ReadAllLines(x);

                foreach (var line in lines)
                {
                    if (line.StartsWith("Mode"))
                    {
                        try
                        {
                            var mode = line.Split(':').Last().ToInt();

                            if (mode==0)
                                return true;
                        }
                        catch{}
                    }
                }

                return false;
            });

            info.osu_file_path = osu_file==null ? osu_files.FirstOrDefault() : osu_file;

            info.osb_file_path= TryGetAnyFiles(".osb").FirstOrDefault();

            info.folder_path=folder_path;

            info.reader=new OsuFileReader(info.osu_file_path);
            var section = new SectionReader(Section.General, info.reader);

            foreach (var line in section.EnumValues())
            {
                var match = Regex.Match(line, @"AudioFilename\s*:\s*(.+)");

                if (match.Success)
                {
                    info.audio_file_path=Path.Combine(folder_path,match.Groups[1].Value);
                    Log.User($"audio file path={info.audio_file_path}");
                    break;
                }
            }

            foreach (var line in section.EnumValues())
            {
                var wideMatch = Regex.Match(line, @"WidescreenStoryboard\s*:\s*(.+)");
                if (wideMatch.Success)
                {
                    info.IsWidescreenStoryboard = (wideMatch.Groups[1].Value.ToInt() == 1);
                    break;
                }
            }

            Trace.Assert((_check(info.osu_file_path)||_check(info.osb_file_path))&&_check(info.audio_file_path));
            
            return info;

            bool _check(string file_path)
            {
                return (!string.IsNullOrWhiteSpace(file_path))&&File.Exists(file_path);
            }

            IEnumerable<string> TryGetAnyFiles(string extend_name)
            {
                return Directory.EnumerateFiles(folder_path, "*"+extend_name, SearchOption.AllDirectories);
            }
        }
    }
}
