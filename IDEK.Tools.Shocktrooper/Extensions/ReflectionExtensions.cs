using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using IDEK.Tools.ErrorHandling;
using IDEK.Tools.Logging;

#if UNITY_5_3_OR_NEWER
using IDEK.Tools.UI.Modals;
#endif

namespace IDEK.Tools.ShocktroopExtensions
{
    public static class ReflectionExtensions
    {
        public static object Cast(this Type Type, object data)
        {
            var DataParam = Expression.Parameter(typeof(object), "data");
            var Body = Expression.Block(Expression.Convert(Expression.Convert(DataParam, data.GetType()), Type));

            var Run = Expression.Lambda(Body, DataParam).Compile();
            var ret = Run.DynamicInvoke(data);
            return ret;
        }

        public static Type GetEnumerableType(this Type type)
        {
            if (type == null)
                throw new ArgumentNullException("type");

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                return type.GetGenericArguments()[0];

            var iface = (from i in type.GetInterfaces()
                         where i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>)
                         select i).FirstOrDefault();

            if (iface == null)
                throw new ArgumentException("Does not represent an enumerable type.", "type");

            return GetEnumerableType(iface);
        }

        public static bool IsGenericList(this Type oType)
        {
            return (oType.IsGenericType && (oType.GetGenericTypeDefinition() == typeof(List<>)));
        }

        public static Type GetMemberFieldPropertyType(this MemberInfo info)
        {
            switch(info.MemberType)
            {
                case MemberTypes.Field:
                {
                    return (info as FieldInfo).FieldType;
                }
                case MemberTypes.Property:
                {
                    return (info as PropertyInfo).PropertyType;
                }
                default:
                {
                    return null;
                }
            }
        }

        public static IEnumerable<MemberInfo> GetAllMembers(this Type t, BindingFlags flags)
        {
            if(t == null) return Enumerable.Empty<MemberInfo>();
            return t.GetFields(flags).Concat(GetAllMembers(t.BaseType, flags));
        }

        /// <summary>
        /// Scans each relevant assembly for classes that are descendants of the given type <see cref="t"/>.
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public static List<Type> GetValidSubclasses(this Type baseType, bool showErrorUI=false, bool isFailureFatal=false)
        {
            List<Type> descendantTypes = new();

            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    // Check if the assembly can reference the base type
                    if (!assembly.GetReferencedAssemblies().Any(a => a.FullName == baseType.Assembly.FullName))
                        continue;

                    foreach (Type type in assembly.GetTypes())
                    {
                        if (type.IsSubclassOf(baseType))
                            descendantTypes.Add(type);
                    }
                }
                catch (ReflectionTypeLoadException e)
                {
                    string errorDesc = $"Ran Into a {e.GetType().FullName} when retrieving subclasses of {baseType.FullName} "
                        + "from assembly {assembly.FullName}. You should fix that.";
                    IDEKException dhException = new(errorDesc, e, isFailureFatal);

#if UNITY_5_3_OR_NEWER
                    if(showErrorUI)
                    {
                        GameErrorModal.ThrowGameError(dhException);
                    }
                    else
#endif
                    {
                        if(isFailureFatal)
                            throw dhException;
                        else
                            ConsoleLog.LogError(dhException);
                    }
                }
            }

            return descendantTypes;
        }

        public static List<Type> GetValidSubclasses(this object o, bool showErrorUI=false, bool isFailureFatal=false) 
            => o.GetType().GetValidSubclasses();

        /// <summary>
        /// Tries to get the first "leaf" subclass 
        /// (a desecandant class that no other class inherits from) within the inheritance tree of type T.
        /// This "First" is not an ordered first, unless there's an order to the Types returned by assembly.GetTypes(). 
        /// It simply returns the first valid class that it finds.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public static Type GetFirstLeafSubclassOrBase(Type baseClassType, bool allowAbstract) 
        {
            //try to find a subclass of gamesettings
            Type settingsType = baseClassType;

            List<Type> subclasses = baseClassType.GetValidSubclasses(false, true);
            IEnumerable<Type> nonAbstractSubclasses = subclasses.Where(subclass => !subclass.IsAbstract);

            if (nonAbstractSubclasses.Count() == 1)
            {
                settingsType = nonAbstractSubclasses.FirstOrDefault() ?? settingsType;
            }
            else if (subclasses.Count() > 1)
            {
                //get only subclasses that are themsevles not inherited from 
                //(the leaves of the GameSettings inheritance tree)
                //have to separately check for abstract bc we want to check the complete hierarchy and avoid breaking any inheritance links
                IEnumerable<Type> leaves = subclasses.Where(subclassA => !subclassA.IsAbstract && !subclasses.Any(subclassB => subclassB.IsSubclassOf(subclassA)));
                Type firstLeaf = leaves.FirstOrDefault();
                settingsType = firstLeaf ?? settingsType;

                if (leaves.Count() > 1)
                {
                    ConsoleLog.LogWarning("Found multiple leaf types descending from GameSettings! Creating an asset using " + leaves.FirstOrDefault());
                }
            }

            return settingsType;
        }

        public static Type GetFirstLeafSubclassOrBase(this object o) => o.GetType().GetFirstLeafSubclassOrBase();

        public static object GetValue(this MemberInfo info, object obj)
        {
            switch (info.MemberType)
            {
                case MemberTypes.Field:
                    {
                        return (info as FieldInfo).GetValue(obj);
                    }
                case MemberTypes.Property:
                    {
                        return (info as PropertyInfo).GetValue(obj, null);
                    }
                default:
                    {
                        return null;
                    }
            }
        }

        public static void SetValue(this MemberInfo info, object obj, object value)
        {
            switch (info.MemberType)
            {
                case MemberTypes.Field:
                    {
                        (info as FieldInfo).SetValue(obj, value);
                        break;
                    }
                case MemberTypes.Property:
                    {
                        (info as PropertyInfo).SetValue(obj, value);
                        break;
                    }
                default:
                    {
                        break;
                    }
            }
        }

        public static bool CanWrite(this MemberInfo info)
        {
            switch (info.MemberType)
            {
                case MemberTypes.Field:
                    {
                        return (info as FieldInfo).IsPublic;
                    }
                case MemberTypes.Property:
                    {
                        return (info as PropertyInfo).CanWrite;
                    }
                default:
                    {
                        return false;
                    }
            }
        }

        public static bool CanRead(this MemberInfo info)
        {
            switch (info.MemberType)
            {
                case MemberTypes.Field:
                    {
                        return (info as FieldInfo).IsPublic;
                    }
                case MemberTypes.Property:
                    {
                        return (info as PropertyInfo).CanRead;
                    }
                default:
                    {
                        return false;
                    }
            }
        }
    }
}