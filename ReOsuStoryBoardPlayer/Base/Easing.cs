using SimpleRenderFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReOsuStoryBoardPlayer
{
    public enum Easing
    {
        Linear = 0,

        EasingOut = 1,
        EasingIn = 2,

        QuadIn = 3,
        QuadOut = 4,
        QuadInOut = 5,

        CubicIn = 6,
        CubicOut = 7,
        CubicInOut = 8,

        QuantIn = 9,
        QuantOut = 10,
        QuantInOut = 11,

        QuintIn = 12,
        QuintOut = 13,
        QuintInOut = 14,

        SineIn = 15,
        SineOut = 16,
        SineInOut = 17,

        ExpoIn = 18,
        ExpoOut = 19,
        ExpoInOut = 20,

        CircIn = 21,
        CircOut = 22,
        CircInOut = 23,

        ElasticIn = 24,
        ElasticOut = 25,
        ElasticInOut = 28,

        ElasticHalfOut = 26,
        ElasticQuarterOut = 27,

        BackIn = 29,
        BackOut = 30,
        BackInOut = 31,

        BounceIn = 32,
        BounceOut = 33,
        BounceInOut = 34
    }

    public static class EasingConverter
    {
        public static Dictionary<Easing, EasingInterpolator> CacheEasingInterpolatorMap { private set; get; }

        public static EasingInterpolator DefaultInterpolator { get; private set; } = new EasingInterpolator(EasingInterpolator.EaseType.Linear);

        static EasingConverter()
        {
            CacheEasingInterpolatorMap = new Dictionary<Easing, EasingInterpolator>();

            foreach (Easing easing in Enum.GetValues(typeof(Easing)))
            {
                CacheEasingInterpolatorMap.Add(easing, GetEasingInterpolator(easing));
            }
        }

        public static EasingInterpolator GetEasingInterpolator(Easing easing)
        {
            switch (easing)
            {
                case Easing.Linear:
                    return new EasingInterpolator(EasingInterpolator.EaseType.Linear);

                case Easing.EasingOut:
                case Easing.EasingIn:
                    Log.Warn("not support easing type {0},return linear.", easing.ToString());
                    return new EasingInterpolator(EasingInterpolator.EaseType.Linear);

                case Easing.QuadIn:
                    return new EasingInterpolator(EasingInterpolator.EaseType.QuadEaseIn);

                case Easing.QuadOut:
                    return new EasingInterpolator(EasingInterpolator.EaseType.QuadEaseOut);

                case Easing.QuadInOut:
                    return new EasingInterpolator(EasingInterpolator.EaseType.QuadEaseInOut);

                case Easing.CubicIn:
                    return new EasingInterpolator(EasingInterpolator.EaseType.CircEaseIn);

                case Easing.CubicOut:
                    return new EasingInterpolator(EasingInterpolator.EaseType.CircEaseInOut);

                case Easing.CubicInOut:
                    return new EasingInterpolator(EasingInterpolator.EaseType.CircEaseInOut);

                case Easing.QuantIn:
                    return new EasingInterpolator(EasingInterpolator.EaseType.QuadEaseIn);

                case Easing.QuantOut:
                    return new EasingInterpolator(EasingInterpolator.EaseType.QuartEaseOut);

                case Easing.QuantInOut:
                    return new EasingInterpolator(EasingInterpolator.EaseType.QuartEaseInOut);

                case Easing.QuintIn:
                    return new EasingInterpolator(EasingInterpolator.EaseType.QuintEaseIn);

                case Easing.QuintOut:
                    return new EasingInterpolator(EasingInterpolator.EaseType.QuintEaseOut);

                case Easing.QuintInOut:
                    return new EasingInterpolator(EasingInterpolator.EaseType.QuintEaseInOut);

                case Easing.SineIn:
                    return new EasingInterpolator(EasingInterpolator.EaseType.SineEaseIn);

                case Easing.SineOut:
                    return new EasingInterpolator(EasingInterpolator.EaseType.SineEaseOut);

                case Easing.SineInOut:
                    return new EasingInterpolator(EasingInterpolator.EaseType.SineEaseInOut);

                case Easing.ExpoIn:
                    return new EasingInterpolator(EasingInterpolator.EaseType.ExpoEaseIn);

                case Easing.ExpoOut:
                    return new EasingInterpolator(EasingInterpolator.EaseType.ExpoEaseOut);

                case Easing.ExpoInOut:
                    return new EasingInterpolator(EasingInterpolator.EaseType.ExpoEaseInOut);

                case Easing.CircIn:
                    return new EasingInterpolator(EasingInterpolator.EaseType.CircEaseIn);

                case Easing.CircOut:
                    return new EasingInterpolator(EasingInterpolator.EaseType.CircEaseOut);

                case Easing.CircInOut:
                    return new EasingInterpolator(EasingInterpolator.EaseType.CircEaseInOut);

                case Easing.ElasticIn:
                    return new EasingInterpolator(EasingInterpolator.EaseType.ElasticEaseIn);

                case Easing.ElasticOut:
                    return new EasingInterpolator(EasingInterpolator.EaseType.ElasticEaseOut);

                case Easing.ElasticInOut:
                    return new EasingInterpolator(EasingInterpolator.EaseType.ElasticEaseInOut);

                case Easing.ElasticHalfOut:
                case Easing.ElasticQuarterOut:
                    Log.Warn("not support easing type {0}", easing.ToString());
                    return new EasingInterpolator(EasingInterpolator.EaseType.Linear);

                case Easing.BackIn:
                    return new EasingInterpolator(EasingInterpolator.EaseType.BackEaseIn);

                case Easing.BackOut:
                    return new EasingInterpolator(EasingInterpolator.EaseType.BackEaseOut);

                case Easing.BackInOut:
                    return new EasingInterpolator(EasingInterpolator.EaseType.BackEaseInOut);

                case Easing.BounceIn:
                    return new EasingInterpolator(EasingInterpolator.EaseType.BounceEaseIn);

                case Easing.BounceOut:
                    return new EasingInterpolator(EasingInterpolator.EaseType.BounceEaseOut);

                case Easing.BounceInOut:
                    return new EasingInterpolator(EasingInterpolator.EaseType.BounceEaseInOut);

                default:
                    Log.Warn("unknown easing type {0},return linear.", easing.ToString());
                    return new EasingInterpolator(EasingInterpolator.EaseType.Linear);
            }
        }
    }
}
