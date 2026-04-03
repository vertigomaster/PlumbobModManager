using System.Text.Json;

namespace IDEK.Tools.ShocktroopUtils
{
    public static class CloneUtility
    {
        public static T? DeepCopy<T>(T source)
        {
            // Unity’s JsonUtility is fast & preserves UnityEngine.Object refs.
            // Newtonsoft may still work better with more complex types though...
            var json = JsonSerializer.Serialize(source);
            return JsonSerializer.Deserialize<T>(json);
        }
    }
}