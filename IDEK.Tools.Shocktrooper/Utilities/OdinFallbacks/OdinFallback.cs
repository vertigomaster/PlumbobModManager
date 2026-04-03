using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;

namespace IDEK.Tools.ShocktroopUtils.OdinFallbacks
{
    /// <summary>
    /// Because sometimes it's easier to make dummy code than it is to go back over every script and
    /// add inline preprocessors and brancing logic regarding the presence of Odin Inspector
    /// </summary>
    public static class OdinFallback
    {
        public static IEnumerable<System.Type> GetBaseClasses(this System.Type type, bool includeSelf = false)
        {
            if (!(type == (System.Type)null) && !(type.BaseType == (System.Type)null))
            {
                if (includeSelf)
                    yield return type;
                for (System.Type current = type.BaseType; current != (System.Type)null; current = current.BaseType)
                    yield return current;
            }
        }

        public static bool IsStatic(this MemberInfo member)
        {
            FieldInfo fieldInfo = member as FieldInfo;
            if (fieldInfo != (FieldInfo)null)
                return fieldInfo.IsStatic;
            PropertyInfo propertyInfo = member as PropertyInfo;
            if (propertyInfo != (PropertyInfo)null)
                return !propertyInfo.CanRead
                    ? propertyInfo.GetSetMethod(true).IsStatic
                    : propertyInfo.GetGetMethod(true).IsStatic;
            MethodBase methodBase = member as MethodBase;
            if (methodBase != (MethodBase)null)
                return methodBase.IsStatic;
            EventInfo eventInfo = member as EventInfo;
            if (eventInfo != (EventInfo)null)
                return eventInfo.GetRaiseMethod(true).IsStatic;
            Type type = member as Type;
            if (!(type != (Type)null))
                throw new NotSupportedException(string.Format((IFormatProvider)CultureInfo.InvariantCulture,
                    "Unable to determine IsStatic for member {0}.{1}MemberType was {2} but only fields, properties, methods, events and types are supported.",
                    (object)member.DeclaringType.FullName, (object)member.Name, (object)member.GetType().FullName));
            return type.IsSealed && type.IsAbstract;
        }
    }
}