using ReOsuStoryBoardPlayer.Commands.Group.Trigger.TriggerCondition;
using ReOsuStoryBoardPlayer.Parser.Reader;
using ReOsuStoryBoardPlayer.Parser.Stream;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReOsuStoryBoardPlayer.Parser.SimpleOsuParser
{
    public class HitObjectReader : IReader<HitObject>
    {
        private readonly SectionReader reader;

        public HitObjectReader(OsuFileReader reader)
        {
            this.reader=new SectionReader(Section.HitObjects,reader);
        }

        public IEnumerable<HitObject> EnumValues()
        {
            return reader.EnumValues().Select(x =>
            {
                try
                {
                    return ParseHitObjectLine(x);
                }
                catch (Exception)
                {
                    return null;
                }
            }
            ).OfType<HitObject>();
        }

        private HitObject? ParseHitObjectLine(string line)
        {
            if (line.StartsWith(@"//")||line.StartsWith(" "))
                return null;
            string[] split = line.Trim().Split(',');

            HitObject hit_object = default;

            hit_object.Type=(HitObjectType)(int.Parse(split[3]))&~HitObjectType.ColourHax;
            hit_object.HitSoundType=(HitObjectSoundType)int.Parse(split[4]);

            hit_object.Time=int.Parse(split[2]);

            if (hit_object.Type==HitObjectType.Normal)
            {
                ApplyHitSoundSetup(5);

            }
            else if (hit_object.Type==HitObjectType.Spinner)
            {
                ApplyHitSoundSetup(6);

            }
            else if (hit_object.Type==HitObjectType.Slider)
            {
                ApplyHitSoundSetup(10);

            }

            return hit_object;

            void ApplyHitSoundSetup(int split_index)
            {
                if (split.Length>split_index)
                {
                    string[] ss = split[split_index].Split(':');
                    hit_object.SampleSet=(SampleSetType)Convert.ToInt32(ss[0]);
                    hit_object.AdditionSampleSet=(SampleSetType)Convert.ToInt32(ss[1]);
                    hit_object.CustomSampleSet=ss.Length>2 ? (CustomSampleSetType)Convert.ToInt32(ss[2]) : CustomSampleSetType.Default;
                }
            }
        }
    }
}
