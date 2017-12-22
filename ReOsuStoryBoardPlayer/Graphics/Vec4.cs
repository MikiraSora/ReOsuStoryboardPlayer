using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.ComponentModel;

namespace ReOsuStoryBoardPlayer
{
    public struct Vec4
    {
        public static Vec4 zero = new Vec4();

        private float __x;
        private float __y;
        private float __z;
        private float __w;
        public float x { get { return __x; } set { __x = value; } }
        public float y { get { return __y; } set { __y = value; } }
        public float z { get { return __z; } set { __z = value; } }
        public float w { get { return __w; } set { __w = value; } }

        public Vec4(int _x, int _y, int _z)
        {
            __x = _x;
            __y = _y;
            __z = _z;
            __w = 1;
        }

        public Vec4(int _x, int _y, int _z, int _w)
        {
            __x = _x;
            __y = _y;
            __z = _z;
            __w = _w;
        }

        public Vec4(float _x, float _y, float _z)
        {
            __x = _x;
            __y = _y;
            __z = _z;
            __w = 1;
        }

        public Vec4(float _x, float _y, float _z, float _w)
        {
            __x = _x;
            __y = _y;
            __z = _z;
            __w = _w;
        }

        public Vec4 clone()
        {
            return (Vec4)MemberwiseClone();
        }

        public override string ToString()
        {
            return x + ", " + y + ", " + z + ", " + w;
        }
    }
}
