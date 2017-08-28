using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Reflection;

namespace ReOsuStoryBoardPlayer
{
    [Serializable]
    public struct Vector
    {
        private float __x;
        private float __y;

        public float x { get { return __x; } set { __x = value; } }
        public float y { get { return __y; } set { __y = value; } }

        public static Vector zero { get { return new Vector(0, 0); } }

        public Vector(float _x, float _y)
        {
            __x = _x;
            __y = _y;
        }

        public Vector clone()
        {
            return (Vector)MemberwiseClone();
        }

        public Vector add(Vector vec)
        {
            return new Vector(x + vec.x, y + vec.y);
        }

        public static Vector lerp(Vector value1, Vector value2, float amount)
        {
            return new Vector(
                (float)lerp(value1.x, value2.x, amount),
                (float)lerp(value1.y, value2.y, amount));
        }

        public static Vector operator -(Vector value)
        {
            value.x = -value.x;
            value.y = -value.y;
            return value;
        }

        public static bool operator ==(Vector value1, Vector value2)
        {
            return value1.x == value2.x && value1.y == value2.y;
        }

        public static bool operator !=(Vector value1, Vector value2)
        {
            return value1.x != value2.x || value1.y != value2.y;
        }

        public static Vector operator /(Vector value1, Vector value2)
        {
            value1.x /= value2.x;
            value1.y /= value2.y;
            return value1;
        }

        public static Vector operator +(Vector value1, Vector value2)
        {
            value1.x += value2.x;
            value1.y += value2.y;
            return value1;
        }

        public static Vector operator -(Vector value1, Vector value2)
        {
            value1.x -= value2.x;
            value1.y -= value2.y;
            return value1;
        }

        public static Vector operator /(Vector value1, float divider)
        {
            float factor = 1 / divider;
            value1.x *= factor;
            value1.y *= factor;
            return value1;
        }

        public static Vector operator *(Vector value1, Vector value2)
        {
            value1.x *= value2.x;
            value1.y *= value2.y;
            return value1;
        }

        public static Vector operator *(float scaleFactor, Vector value)
        {
            value.x *= scaleFactor;
            value.y *= scaleFactor;
            return value;
        }

        public static Vector operator *(Vector value, float scaleFactor)
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