using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ReOsuStoryboardPlayer.Core.Serialization
{
    public static class SystemTypeSerializationExtension
    {
        public static void OnSerialize(this bool i, BinaryWriter writer) => writer.Write(i);
        public static void OnSerialize(this int i, BinaryWriter writer) => writer.Write(i);
        public static void OnSerialize(this ushort i, BinaryWriter writer) => writer.Write(i);
        public static void OnSerialize(this string i, BinaryWriter writer) => writer.Write(i);
        public static void OnSerialize(this byte i, BinaryWriter writer) => writer.Write(i);
        public static void OnSerialize(this float i, BinaryWriter writer) => writer.Write(i);
        public static void OnSerialize(this double i, BinaryWriter writer) => writer.Write(i);
        public static void OnSerialize(this long i, BinaryWriter writer) => writer.Write(i);

        public static void OnDeserialize(ref this long i, BinaryReader writer) => i=writer.ReadInt64();
        public static void OnDeserialize(ref this bool i, BinaryReader writer) => i=writer.ReadBoolean();
        public static void OnDeserialize(ref this int i, BinaryReader writer) => i=writer.ReadInt32();
        public static void OnDeserialize(ref this byte i, BinaryReader writer) => i=writer.ReadByte();
        public static void OnDeserialize(ref this ushort i, BinaryReader writer) => i=writer.ReadUInt16();
        public static void OnDeserialize(ref this float i, BinaryReader writer) => i=writer.ReadSingle();
        public static void OnDeserialize(ref this double i, BinaryReader writer) => i=writer.ReadDouble();
    }
}
