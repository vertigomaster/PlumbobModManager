using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
#if UNITY_5_3_OR_NEWER
using UnityEngine;
#endif
using Random = System.Random;

namespace IDEK.Tools.ShocktroopExtensions
{
    public static class IEnumberableExtensions
    {
        private static Random _rng;

        public static void ForEach<T>(this IEnumerable<T> ie, Action<T> action)
        {
            foreach (T i in ie)
            {
                action(i);
            }
        }

        public static void Shuffle<T>(this IList<T> list)
        {
            if (_rng == null)
            {
                _rng = new Random();
            }

            //goes backwards, swapping the current position with a random index. 
            //It then steps forward, reducing the random range to not include the previous position(s)
            int num = list.Count;
            while (num > 1) 
            {
                //decrease range size to not include previous elements
                //also nicely initializes on first run when num initialized to the count/length
                num--;

                //get number in the range
                int index = _rng.Next(num + 1);

                //swap
                T value = list[index];
                list[index] = list[num];
                list[num] = value;
            }
        }
        
        /// <summary>
        /// gets a random value from the enumerable
        /// </summary>
        /// <param name="collection"></param>
        /// <typeparam name="T"></typeparam>
        public static T GetRandom<T>(this IEnumerable<T> ie)
        {
#if UNITY_5_3_OR_NEWER
            return ie.ElementAt(UnityEngine.Random.Range(0, ie.Count()));
#else
            var enumerable = ie as T[] ?? ie.ToArray();
            return enumerable.ElementAt(Random.Shared.Next(0, enumerable.Count()));
#endif
        }

        public static TValue GetRandomValue<TKey, TValue> (this IDictionary<TKey, TValue> dict)
        {
            return dict.GetRandom().Value;
        }

        public static Dictionary<T, U> ShallowCopy<T, U>(this IDictionary<T, U> dict) where T : notnull
        {
            return dict.ToDictionary(
                keySelector: kvp => kvp.Key, 
                elementSelector: kvp => kvp.Value);
        }

        public static T GetArbitraryOrDefault<T>(this IEnumerable<T> enumer)
        {
            IEnumerator<T> enumerator = enumer.GetEnumerator();
            return enumerator.MoveNext() ? enumerator.Current : default;
        }

        public static T GetArbitrary<T>(this IEnumerable<T> enumer)
        {
            IEnumerator<T> enumerator = enumer.GetEnumerator();
            enumerator.MoveNext(); 
            return enumerator.Current;
        }
#if UNITY_5_3_OR_NEWER
        public static void SetAllActive(this IEnumerable<GameObject> enumer, bool newActiveState, bool assumeNotNull=false)
        {
            var readyEnumer = assumeNotNull ? enumer : enumer.Where(obj => obj != null);
            readyEnumer.ForEach(obj => obj.SetActive(newActiveState));
        }

        public static void EnableAll(this IEnumerable<Behaviour> enumer, bool newActiveState, bool assumeNotNull=false)
        {
            var readyEnumer = assumeNotNull ? enumer : enumer.Where(obj => obj != null);
            readyEnumer.ForEach(obj => obj.enabled = newActiveState);
        }
#endif
    }
}
