using System;

namespace ReOsuStoryboardPlayer.Core.Serialization
{
    [AttributeUsage(AttributeTargets.Property|AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public class AutoSerializableAttribute : Attribute
    {
    }
}