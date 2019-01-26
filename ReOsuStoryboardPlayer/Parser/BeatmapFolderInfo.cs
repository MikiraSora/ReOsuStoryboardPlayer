using ReOsuStoryboardPlayer.Core.Parser;
using ReOsuStoryboardPlayer.Core.Parser.Reader;
using ReOsuStoryboardPlayer.Core.Parser.Stream;
using ReOsuStoryboardPlayer.Core.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace ReOsuStoryboardPlayer.Parser
{
    public class BeatmapFolderInfo
    {
        public string osu_file_path { get; private set; }

        public string osb_file_path { get; private set; }

        public string audio_file_path { get; private set; }

        public string folder_path { get; private set; }

        public OsuFileReader reader { get; private set; }

        public bool IsWidescreenStoryboard { get; private set; }

        private BeatmapFolderInfo()
        {
        }

        public static BeatmapFolderInfo Parse(string folder_path, ProgramCommandParser.Parameters args)
        {
            if (!Directory.Exists(folder_path))
                throw new Exception($"\"{folder_path}\" not a folder!");

            BeatmapFolderInfo info = new BeatmapFolderInfo();

            var osu_files = TryGetAnyFiles(".osu");

            string explicitly_osu_diff_name = string.Empty;
            if (args!=null&&args.TryGetArg("diff", out var diff_name))
                explicitly_osu_diff_name=diff_name;

            if (!string.IsNullOrWhiteSpace(explicitly_osu_diff_name))
            {
                int index = -1;
                index=int.TryParse(explicitly_osu_diff_name, out index) ? index : -1;

                if (index>0)
                {
                    info.osu_file_path=osu_files.OrderBy(x => x).FirstOrDefault();
                }
                else
                {
                    var fix_pattern = Regex.Escape(explicitly_osu_diff_name);
                    Regex regex = new Regex(@"\[.*"+fix_pattern+@".*\]\.osu", RegexOptions.IgnoreCase);

                    info.osu_file_path=osu_files.Where(x => regex.IsMatch(x)).OrderBy(x => x.Length).FirstOrDefault();
                }
            }
            else
            {
                //优先先选std铺面的.一些图其他模式谱面会有阻挡 53925 fripSide - Hesitation Snow
                info.osu_file_path=osu_files.FirstOrDefault(x =>
                {
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
                            catch { }
                        }
                    }

                    return false;
                });

                if (!File.Exists(info.osu_file_path))
                    info.osu_file_path=osu_files.FirstOrDefault();
            }

            if (string.IsNullOrWhiteSpace(info.osu_file_path)||!File.Exists(info.osu_file_path))
                Log.Warn("No .osu load");

            info.osb_file_path=TryGetAnyFiles(".osb").FirstOrDefault();

            info.folder_path=folder_path;

            if ((!string.IsNullOrWhiteSpace(info.osu_file_path))&&File.Exists(info.osu_file_path))
            {
                info.reader=new OsuFileReader(info.osu_file_path);
                var section = new SectionReader(Section.General, info.reader);

                info.audio_file_path=Path.Combine(folder_path, section.ReadProperty("AudioFilename"));
                Log.User($"audio file path={info.audio_file_path}");

                var wideMatch = section.ReadProperty("WidescreenStoryboard");

                if (!string.IsNullOrWhiteSpace(wideMatch))
                    info.IsWidescreenStoryboard=wideMatch.ToInt()==1;
            }

            if (string.IsNullOrWhiteSpace(info.osu_file_path)||(!File.Exists(info.osu_file_path)))
            {
                info.audio_file_path=Directory
                    .GetFiles(info.folder_path, "*.mp3")
                    .Select(x => new FileInfo(x))
                    .OrderByDescending(x => x.Length)
                    .FirstOrDefault()
                    .FullName;
            }

            Trace.Assert(((_check(info.osu_file_path)||_check(info.osb_file_path)))&&_check(info.audio_file_path));

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

        ~BeatmapFolderInfo()
        {
            reader.Dispose();
        }
    }
}