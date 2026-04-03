//Created By: Julian Noel

using System.Diagnostics;
using System.Linq;
using System.Reflection;

using IDEK.Tools.ShocktroopExtensions;
using IDEK.Tools.ShocktroopUtils;

namespace IDEK.Tools.Debugging
{
    public static class DHDebug
    {
        // public static void DrawArrow(Vector3 start, Vector3 dir, Color color, float duration = 0f,
        //     float arrowHeadScale = 1f, float arrowAngleDegrees = 30f)
        // {
        //     Vector3 tail = start + dir;
        //     Vector3 scaledTail = -dir.ScaleToMagnitude(arrowHeadScale);
        //     UnityEngine.Debug.DrawRay(start, dir, color, duration);
        //     UnityEngine.Debug.DrawRay(tail, Quaternion.AngleAxis(30f, Vector3.up) * scaledTail, color, duration);
        //     UnityEngine.Debug.DrawRay(tail, Quaternion.AngleAxis(-30f, Vector3.up) * scaledTail, color, duration);
        // }

        public static string GetStackTrace(bool includeDebugCall = false)
        {
            return new StackTrace().GetFrames()
                .Skip(includeDebugCall ? 0 : 1) //skip first (the BuildStackTrace() method calling this) if param true
                .Aggregate("", (accumString, entry) =>
                {
                    //reduce map to string
                    MethodBase method = entry.GetMethod();

                    string piece = method == null ? "NULL" : method.DeclaringType.FullName + "." + method.Name;

                    return accumString + piece + "\n";
                });
        }
    }
}