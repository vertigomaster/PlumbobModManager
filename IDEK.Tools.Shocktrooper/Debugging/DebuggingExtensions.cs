//Created By: Julian Noel

using IDEK.Tools.ShocktroopExtensions;
using IDEK.Tools.ShocktroopUtils;

namespace IDEK.Tools.Debugging
{
    public static class DebuggingExtensions
    {
        /// <summary>
        /// Uses reflection to try and automatically ToString() all of the given object's members.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="thing"></param>
        /// <returns></returns>
        public static string ToStringMembers<T>(this T thing)
        {
            string dump = $"{nameof(T)} {nameof(thing)} {{";
            foreach(System.Reflection.MemberInfo v in typeof(T).GetMembers())
            {
                object memberObj = v.GetValue(thing);

                string runtimeTypeString = memberObj?.GetType().ToString() ?? "NULL";
                string runtimeValueString = memberObj?.ToString() ?? "NULL";

                dump += $"\n\t{v.MemberType} | TYPE: {runtimeTypeString} (via {v.DeclaringType}) | {v.Name}: \n\t\t\"{runtimeValueString}\"";
            }
            dump += "\n}";

            return dump;
        }
    }
}