using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace IDEK.Tools.ShocktroopUtils
{
    public static class Sanitization
    {
        /// <summary>
        /// Returns a "sanitized" enumeration that has filtered out all <see cref="TElement"/>s that aren't also of
        /// type <see cref="TCheck"/>. All remaining <see cref="TElement"/>, if any, 
        /// are guaranteed to be pattern matchable as elements of type <see cref="TCheck"/>.
        /// If none are, a non-null but empty enumeration is returned.
        /// </summary>
        /// <typeparam name="TElement"></typeparam>
        /// <typeparam name="TCheck"></typeparam>
        /// <param name="elements"></param>
        /// <returns></returns>
        public static IEnumerable<TElement> SanitizeByType<TElement, TCheck>(this IEnumerable<TElement> elements)
        {
            return elements
                .Distinct()
                .Where(x => x != null && x is TCheck);
        }

        /// <summary>
        /// Similar to <see cref="SanitizeByType{TElement, TCheck}(IEnumerable{TElement})"/> but returns an enumeration of the <see cref="TCheck"/> type.
        /// </summary>
        /// <typeparam name="TElement"></typeparam>
        /// <typeparam name="TCheck"></typeparam>
        /// <param name="elements"></param>
        /// <returns></returns>
        public static IEnumerable<TCheck> SanitizeToType<TCheck>(this IEnumerable<object> elements) 
            where TCheck : class
        {
            return elements
                .Distinct()
                .Where(x => x != null && x is TCheck)
                .Select(x => x as TCheck);
        }

        public static IEnumerable<TCheck> Sanitize<TCheck>(this IEnumerable<TCheck> elements)
            where TCheck : class
        {
            return elements
                .Distinct()
                .Where(x => x != null);
        }
    }
}
