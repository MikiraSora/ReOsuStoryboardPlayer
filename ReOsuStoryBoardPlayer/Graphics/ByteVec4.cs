using System;

namespace ReOsuStoryBoardPlayer
{
    [Serializable]
    public struct ByteVec4
    {
        public byte x { get; set; }
        public byte y { get; set; }
        public byte z { get; set; }
        public byte w { get; set; }

        public static ByteVec4 Zero { get { return new ByteVec4(0,0,0,0); } }
        public static ByteVec4 One { get { return new ByteVec4(1,1,1,1); } }

        public ByteVec4(byte x, byte y,byte z,byte w)
        {
            this.x =x;
            this.y = y;
            this.z=z;
            this.w=w;
        }

        public ByteVec4 clone()
        {
            return (ByteVec4)MemberwiseClone();
        }

        public ByteVec4 add(ByteVec4 vec)
        {
            return new ByteVec4((byte)(x + vec.x), (byte)(y + vec.y), (byte)(z+vec.z), (byte)(w+vec.w));
        }

        public static ByteVec4 lerp(ByteVec4 value1, ByteVec4 value2, float amount)
        {
            return new ByteVec4(
                (byte)lerp(value1.x, value2.x, amount),
                (byte)lerp(value1.y, value2.y, amount),
                (byte)lerp(value1.z, value2.z, amount),
                (byte)lerp(value1.w, value2.w, amount));
        }

        public static ByteVec4 operator -(ByteVec4 value)
        {
            value.x =(byte)-value.x;
            value.y =(byte)-value.y;
            value.w=(byte)-value.w;
            value.z=(byte)-value.z;
            return value;
        }

        public static bool operator ==(ByteVec4 value1, ByteVec4 value2)
        {
            return value1.x == value2.x && value1.y == value2.y && value1.w==value2.w && value1.z==value2.z;
        }

        public static bool operator !=(ByteVec4 value1, ByteVec4 value2)
        {
            return value1.x != value2.x || value1.y != value2.y ||value1.w!=value2.w||value1.z!=value2.z;
        }

        public static ByteVec4 operator /(ByteVec4 value1, ByteVec4 value2)
        {
            value1.x /= value2.x;
            value1.y /= value2.y;
            value1.z/=value2.z;
            value1.w/=value2.w;
            return value1;
        }

        public static ByteVec4 operator +(ByteVec4 value1, ByteVec4 value2)
        {
            value1.x += value2.x;
            value1.y += value2.y;
            value1.w+=value2.w;
            value1.z+=value2.z;
            return value1;
        }

        public static ByteVec4 operator -(ByteVec4 value1, ByteVec4 value2)
        {
            value1.x -= value2.x;
            value1.y -= value2.y;
            value1.w-=value2.w;
            value1.z-=value2.z;
            return value1;
        }

        public static ByteVec4 operator /(ByteVec4 value1, float divider)
        {
            float factor = 1 / divider;
            value1.x = (byte)(value1.x*factor);
            value1.y=(byte)(value1.y*factor);
            value1.w=(byte)(value1.w*factor);
            value1.z=(byte)(value1.z*factor);
            return value1;
        }

        public static ByteVec4 operator *(ByteVec4 value1, ByteVec4 value2)
        {
            value1.x *= value2.x;
            value1.y *= value2.y;
            value1.w*=value2.w;
            value1.z*=value2.z;
            return value1;
        }

        public static ByteVec4 operator *(float scaleFactor, ByteVec4 value)
        {
            value.x=(byte)(value.x*scaleFactor);
            value.y=(byte)(value.y*scaleFactor);
            value.w=(byte)(value.w*scaleFactor);
            value.z=(byte)(value.z*scaleFactor);
            
            return value;
        }

        public static ByteVec4 operator *(ByteVec4 value, float scaleFactor)
        {
            value.x=(byte)(value.x*scaleFactor);
            value.y=(byte)(value.y*scaleFactor);
            value.w=(byte)(value.w*scaleFactor);
            value.z=(byte)(value.z*scaleFactor);

            return value;
        }

        public override string ToString()
        {
            return $"[{x},{y},{z},{w}]";
        }

        public static double lerp(double value1, double value2, double amount)
        {
            return value1 + (value2 - value1) * amount;
        }
    }
}