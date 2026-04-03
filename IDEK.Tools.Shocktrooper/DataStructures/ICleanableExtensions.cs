using System.Collections.Generic;
using System.Linq;

namespace IDEK.Tools.DataStructures.Linq
{
    public static class ICleanableExtensions
    {
        //this method does not play nice with presistent data like the dirty state and callbacks
        //public static CleanableList<TSource> ToCleanableList<TSource>(this IEnumerable<TSource> source, CleanableList<TSource> original)
        //{
        //    return new CleanableList<TSource>(source, original);
        //}
    }
}
