using ReOsuStoryboardPlayer.Core.Parser;
using ReOsuStoryboardPlayer.Core.Parser.Reader;
using ReOsuStoryboardPlayer.Core.Parser.Stream;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace ReOsuStoryboardPlayer.Core.Utils
{
    /// <summary>
    /// 方便从谱面文件夹解析得到必要的内容，比如osu文件路径,osb文件路径等
    /// </summary>
    public class BeatmapFolderInfo
    {
        private static readonly Regex regex = new Regex(@"\[(.*)\]\.osu", RegexOptions.IgnoreCase);

        public string osb_file_path { get; set; }

        public string folder_path { get; set; }

        public OsuFileReader reader { get; set; }

        public bool IsWidescreenStoryboard { get; set; }

        public Dictionary<string, string> DifficultFiles { get; protected set; } = new Dictionary<string, string>();

        protected BeatmapFolderInfo()
        {

        }

        public static BeatmapFolderInfo Parse(string folder_path) => Parse<BeatmapFolderInfo>(folder_path);

        /// <summary>
        /// 根据文件夹路径解析谱面文件夹的数据
        /// </summary>
        /// <param name="folder_path"></param>
        /// <param name="explicitly_osu_diff_name">指定的难度名</param>
        /// <returns></returns>
        public static T Parse<T>(string folder_path) where T : BeatmapFolderInfo
        {
            if (!Directory.Exists(folder_path))
                throw new Exception($"\"{folder_path}\" not a folder!");

            var info = typeof(T).GetConstructors(BindingFlags.NonPublic|BindingFlags.Instance).FirstOrDefault().Invoke(new object[] { }) as T;

            foreach (var osu_file in TryGetAnyFiles(".osu"))
            {
                var match = regex.Match(osu_file);

                if (!match.Success)
                    continue;

                info.DifficultFiles[match.Groups[1].Value] = osu_file;
            }

            info.osb_file_path = TryGetAnyFiles(".osb").FirstOrDefault();

            info.folder_path = folder_path;

            if (!(info.DifficultFiles.All(x=>_check(x.Value)) || _check(info.osb_file_path)))
                throw new Exception($"missing files such as .osu/.osb and audio file which is registered in .osu");

            return info;

            bool _check(string file_path)
            {
                return (!string.IsNullOrWhiteSpace(file_path)) && File.Exists(file_path);
            }

            IEnumerable<string> TryGetAnyFiles(string extend_name)
            {
                return Directory.EnumerateFiles(folder_path, "*" + extend_name, SearchOption.AllDirectories);
            }
        }


        ~BeatmapFolderInfo()
        {
            reader?.Dispose();
        }
    }
}
