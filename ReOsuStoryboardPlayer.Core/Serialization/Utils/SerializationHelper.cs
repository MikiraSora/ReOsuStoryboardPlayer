using ReOsuStoryboardPlayer.Core.Utils;
using System;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;

namespace ReOsuStoryboardPlayer.Core.Serialization
{
    public static class SerializationHelper
    {
        public static void AutoSerializeByCustomAttributes<T>(T obj, SerializationInfo info, StreamingContext context, bool save)
        {
            var type = typeof(T);

            var serializable_members = type.GetMembers(BindingFlags.NonPublic|BindingFlags.Public|BindingFlags.Instance)
                .Where(x => x.GetCustomAttribute<AutoSerializableAttribute>()!=null)
                .Where(x =>
                {
                    if (x is PropertyInfo prop)
                        return prop.CanRead&&prop.CanWrite;

                    //field
                    return true;
                });

            foreach (var member in serializable_members)
            {
                var name = member.Name;
                object val = null;

                if (save)
                {
                    //serialize
                    val=GetValue(member);
                    info.AddValue(name, val);
                }
                else
                {
                    //deserialize
                    val=info.GetValue(name, GetMemberObjectType(member));
                    SetValue(member, val);
                }

                Log.Debug($"{(save ? "Serialized" : "Deserialized")} {type.Name}::{name} -> {val.ToString()}");
            }

            object GetValue(MemberInfo member_info)
            {
                switch (member_info)
                {
                    case PropertyInfo prop:
                        return prop.GetValue(obj);

                    case FieldInfo field:
                        return field.GetValue(obj);

                    default:
                        return null;
                }
            }

            void SetValue(MemberInfo member_info, object value)
            {
                switch (member_info)
                {
                    case PropertyInfo prop:
                        prop.SetValue(obj, value);
                        break;

                    case FieldInfo field:
                        field.SetValue(obj, value);
                        break;

                    default:
                        break;
                }
            }
        }

        public static Type GetMemberObjectType(MemberInfo member_info)
        {
            switch (member_info)
            {
                case PropertyInfo prop:
                    return prop.PropertyType;

                case FieldInfo field:
                    return field.FieldType;

                default:
                    return null;
            }
        }
    }
}