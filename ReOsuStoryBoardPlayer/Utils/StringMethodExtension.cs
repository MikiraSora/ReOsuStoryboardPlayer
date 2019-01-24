using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ReOsuStoryBoardPlayer.Utils
{
    public static class StringMethodExtension
    {
        public static double ToDouble(this string chars) => double.Parse(chars);

        public static float ToSigle(this string chars) => float.Parse(chars);

        public static int ToInt(this string chars) => int.Parse(chars);
    }
}