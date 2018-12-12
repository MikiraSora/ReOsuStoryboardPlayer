using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace ReOsuStoryBoardPlayer.Graphics
{
    [Serializable]
    public struct HalfVector
    {
        private Half __x;
        private Half __y;

        public Half x { get { return __x; } set { __x = value; } }
        public Half y { get { return __y; } set { __y = value; } }

        public static HalfVector Zero { get { return new HalfVector(new Half(0f), new Half(0f)); } }
        public static HalfVector One { get { return new HalfVector(new Half(0f), new Half(0f)); } }

        public HalfVector(Half _x, Half _y)
        {
            __x = _x;
            __y = _y;
        }

        public HalfVector(float _x, float _y)
        {
            __x = (Half)_x;
            __y = (Half)_y;
        }

        public HalfVector clone()
        {
            return (HalfVector)MemberwiseClone();
        }

        public HalfVector add(HalfVector vec)
        {
            return new HalfVector((Half)(x + vec.x), (Half)(y + vec.y));
        }

        public static HalfVector lerp(HalfVector value1, HalfVector value2, Half amount)
        {
            return new HalfVector(
                (Half)lerp(value1.x, value2.x, amount),
                (Half)lerp(value1.y, value2.y, amount));
        }

        public static HalfVector operator -(HalfVector value)
        {
            value.x = (Half)(-value.x);
            value.y = (Half)(-value.y);
            return value;
        }

        public static bool operator ==(HalfVector value1, HalfVector value2)
        {
            return value1.x == value2.x && value1.y == value2.y;
        }

        public static bool operator !=(HalfVector value1, HalfVector value2)
        {
            return value1.x != value2.x || value1.y != value2.y;
        }

        public static HalfVector operator /(HalfVector value1, HalfVector value2)
        {
            value1.x /= value2.x;
            value1.y /= value2.y;
            return value1;
        }

        public static HalfVector operator +(HalfVector value1, HalfVector value2)
        {
            value1.x += value2.x;
            value1.y += value2.y;
            return value1;
        }

        public static HalfVector operator -(HalfVector value1, HalfVector value2)
        {
            value1.x -= value2.x;
            value1.y -= value2.y;
            return value1;
        }

        public static HalfVector operator /(HalfVector value1, Half divider)
        {
            Half factor = (Half)(1.0f / divider);
            value1.x *= factor;
            value1.y *= factor;
            return value1;
        }

        public static HalfVector operator *(HalfVector value1, HalfVector value2)
        {
            value1.x *= value2.x;
            value1.y *= value2.y;
            return value1;
        }

        public static HalfVector operator *(Half scaleFactor, HalfVector value)
        {
            value.x *= scaleFactor;
            value.y *= scaleFactor;
            return value;
        }

        public static HalfVector operator *(HalfVector value, Half scaleFactor)
        {
            value.x *= scaleFactor;
            value.y *= scaleFactor;
            return value;
        }

        public override string ToString()
        {
            return "[" + x + ";" + y + "]";
        }

        public static double lerp(double value1, double value2, double amount)
        {
            return value1 + (value2 - value1) * amount;
        }
    }
}
