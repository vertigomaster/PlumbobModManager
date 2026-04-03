using System;

namespace IDEK.Tools.ShocktroopUtils.Cleanables
{
    [Obsolete("Completely obsolete now. Will remove this class in a future release. Use CleanableValue<bool> instead. Can use CleanableValue<T> for all sorts of type wrappers too!")]
    public class CleanBool : CleanableValue<bool> { }
}