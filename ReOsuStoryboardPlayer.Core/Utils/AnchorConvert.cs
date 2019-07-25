using ReOsuStoryboardPlayer.Core.Base;
using ReOsuStoryboardPlayer.Core.PrimitiveValue;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ReOsuStoryboardPlayer.Core.Utils
{
    public static class AnchorConvert
    {
        private readonly static Dictionary<Anchor, HalfVector> AnchorVectorMap = new Dictionary<Anchor, HalfVector>()
        {
            {Anchor.TopLeft,new HalfVector(-0.5f,0.5f)},
            {Anchor.TopCentre,new HalfVector(0.0f, 0.5f)},
            {Anchor.TopRight,new HalfVector(0.5f, 0.5f)},
            {Anchor.CentreLeft,new HalfVector(-0.5f, 0.0f)},
            {Anchor.Centre,new HalfVector(0.0f, 0.0f)},
            {Anchor.CentreRight,new HalfVector(0.5f, 0.0f)},
            {Anchor.BottomLeft,new HalfVector(-0.5f, -0.5f)},
            {Anchor.BottomCentre,new HalfVector(0.0f, -0.5f)},
            {Anchor.BottomRight,new HalfVector(0.5f, -0.5f)}
        };

        public static Anchor? Convert(HalfVector offset)
        {
            return AnchorVectorMap.Where(x => x.Value == offset).Select(x => x.Key).FirstOrDefault();
        }

        public static HalfVector Convert(Anchor anchor)
        {
            return AnchorVectorMap.TryGetValue(anchor, out var hv) ? hv : Convert(Anchor.Centre);
        }
    }
}
