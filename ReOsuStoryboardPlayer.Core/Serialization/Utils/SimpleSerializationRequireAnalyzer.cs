using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ReOsuStoryboardPlayer.Core.Serialization
{
    public static class SimpleSerializationRequireAnalyzer
    {
        public static IEnumerable<Type> ScanNeedSerializeTypes(Type type, HashSet<Type> record = null)
        {
            if (record==null)
                record=new HashSet<Type>();

            if (type.IsGenericType)
                foreach (var t in type.GenericTypeArguments.Select(x => ScanNeedSerializeTypes(x, record)).SelectMany(l => l))
                    yield return t;

            if (type.IsArray)
                foreach (var t in ScanNeedSerializeTypes(type.GetElementType(), record))
                    yield return t;

            if (record.Contains(type))
                yield break;
            else
                record.Add(type);

            if (type.Namespace.StartsWith("System"))
                yield break;

            if (!IsSerializable(type))
                yield return type;

            foreach (var item in type.GetMembers(BindingFlags.NonPublic|BindingFlags.Public|BindingFlags.Instance)
                .Where(x => x is PropertyInfo||x is FieldInfo)
                .Where(x => x.GetCustomAttribute<NonSerializedAttribute>()==null)
                .Where(x => !record.Contains(x))
                .Where(x =>
                {
                    if (x is PropertyInfo prop&&x.GetCustomAttribute<AutoSerializableAttribute>()!=null)
                        return prop.CanRead&&prop.CanWrite;
                    return true;
                })
                .Select(x => SerializationHelper.GetMemberObjectType(x))
                .OfType<Type>()
                .SelectMany(l => ScanNeedSerializeTypes(l, record)))
                yield return item;

            var sub_types = AppDomain.CurrentDomain.GetAssemblies()
                .Select(x => x.GetTypes())
                .SelectMany(x => x)
                .Where(x => x.IsSubclassOf(type)||x.IsAssignableFrom(type))
                .SelectMany(x => ScanNeedSerializeTypes(x, record));

            foreach (var t in sub_types)
            {
                yield return t;
            }
        }

        private static bool IsSerializable(Type type) => type.Attributes.HasFlag(TypeAttributes.Serializable);
    }
}