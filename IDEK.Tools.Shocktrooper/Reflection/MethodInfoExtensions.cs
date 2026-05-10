using System.Reflection;

namespace IDEK.Tools.ShocktroopUtils.CILAnalysis;

public static class MethodInfoExtensions
{
    public static string ParamsToString(this MethodInfo method)
    {
        return string.Join(", ", method.GetParameters().Select(p => p.ParameterType.Name));
    }
    
}