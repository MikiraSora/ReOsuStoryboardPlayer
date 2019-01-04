using ReOsuStoryBoardPlayer.Parser.Stream;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReOsuStoryBoardPlayer.Parser.SimpleOsuParser
{
    /// <summary>
    /// 读取解析osu文件，获取简单的谱面数据，用来播放器自动触发Trigger命令
    /// * 因为非核心内容，移植的时候就不弄这货了
    /// </summary>
    public static class HitObjectParserHelper
    {
        public static IEnumerable<HitObject> ParseHitObjects(string path)
        {
            OsuFileReader reader = new OsuFileReader(path);

            var hitobject_reader = new HitObjectReader(reader);

            return hitobject_reader.EnumValues();
        }
    }
}
