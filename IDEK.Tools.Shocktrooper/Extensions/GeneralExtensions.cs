// Created by: Trevor T.
// Edited by: Bill D., Julian Noel

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using IDEK.Tools.Logging;

namespace IDEK.Tools.ShocktroopExtensions
{

    public static class GeneralExtensions
    {

        public static bool IsNullOrDestroyed(this System.Object obj)
        {
            //check against the raw C# reference without any overloads getting in the way
            if (object.ReferenceEquals(obj, null)) return true; 

#if UNITY_5_3_OR_NEWER
            //Unity objects
            if (obj is UnityEngine.Object) return (obj as UnityEngine.Object) == null;
#endif
            return false;
        }
#if UNITY_5_3_OR_NEWER
        public static Transform ResetLocalOffsets(this Transform trans)
        {
            trans.localPosition = Vector3.zero;
            trans.localRotation = Quaternion.identity;
            trans.localScale = Vector3.one;
            return trans;
        }

        [Obsolete("Use ToTexture2D() instead")]
        public static Texture2D toTexture2D(this RenderTexture rTex) => ToTexture2D(rTex);

        public static Texture2D ToTexture2D(this RenderTexture rTex)
        {
            Texture2D tex = new Texture2D(rTex.width, rTex.height, TextureFormat.RGB24, false);
            RenderTexture old_rt = RenderTexture.active;
            RenderTexture.active = rTex;

            tex.ReadPixels(new Rect(0, 0, rTex.width, rTex.height), 0, 0);
            tex.Apply();

            RenderTexture.active = old_rt;
            return tex;
        }
#endif

        #region Do Function

#if UNITY_5_3_OR_NEWER
        public static void DoFunctionToTree(this Transform root, System.Action<Transform> function)
        {
            function.Invoke(root);
            foreach (Transform child in root)
                child.DoFunctionToTree(function);
        }

        //TODO: TaskRoutine overhaul
        public static void RunFunctionOnDelay(this MonoBehaviour mb, System.Action function, YieldInstruction delay)
        {
            IEnumerator DoFunction()
            {
                yield return delay;
                function.Invoke();
            }
            mb.StartCoroutine(DoFunction());
        }

        //TODO: TaskRoutine overhaul
        public static void RunFunctionOnDelay(this MonoBehaviour mb, System.Action function, CustomYieldInstruction delay)
        {
            IEnumerator DoFunction()
            {
                yield return delay;
                function.Invoke();
            }
            mb.StartCoroutine(DoFunction());
        }
#endif

        #endregion Do Function

        public static bool IsBitSet(this int b, int pos)
        {
            return (b & (1 << pos)) != 0;
        }
#if UNITY_5_3_OR_NEWER
        public static void ToggleActive(this GameObject gameObject)
        {
            if (gameObject.activeSelf)
                gameObject.SetActive(false);
            else
                gameObject.SetActive(true);
        }
#endif

        #region List Retreival
        public static bool TryGetIndexOf<T>(this List<T> list, T element, out int outputIndex)
        {
            outputIndex = list.IndexOf(element);
            return outputIndex >= 0;
        }

        /// <summary>
        /// Like LINQ's ElementAtOrDefault but gives null instead of a default
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public static object ElementAtOrNull<T>(this List<T> list, int index) where T : class
        {
            if (index >= 0 && index < list.Count)
            {
                return list[index];
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Like LINQ's ElementAtOrDefault but gives null instead of a default
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public static bool TryGetElementAt<T>(this List<T> list, int index, out T outputElement)
        {
            ConsoleLog.Log("trying to get element, received adjusted index => " + index);

            bool indexIsValid = index >= 0 && index < list.Count;

            ConsoleLog.Log("index >= 0 => " + (index >= 0));
            ConsoleLog.Log("index < list.Count => " + (index < list.Count));

            outputElement = indexIsValid ? list[index] : default(T);

            ConsoleLog.Log("tryget success = " + indexIsValid);

            return indexIsValid;
        }

        #endregion List Retreival

        #region Enum Movements
        /// <summary>
        /// Grabs the next valid enum value. If looping is disabled and it goes out of bound, it returns the original input (bc structs can't be null).
        /// </summary>
        /// <typeparam name="SomeEnum"></typeparam>
        /// <param name="sourceEnumValue"></param>
        /// <returns></returns>
        public static SomeEnum Next<SomeEnum>(this SomeEnum sourceEnumValue, bool loopAtEnd = false) where SomeEnum : System.Enum
        {
            //Get list of all the enum's values (skips gaps)
            SomeEnum[] orderedEnumValues = (SomeEnum[])Enum.GetValues(sourceEnumValue.GetType());

            //get the array index of enum value that's right after it in the list
            //This is NOT the actual integer associated with the enum value itself, just the index in orderedEnumValues
            int nextEnumIndex = Array.IndexOf<SomeEnum>(orderedEnumValues, sourceEnumValue) + 1;

            if (nextEnumIndex == orderedEnumValues.Length) //if out of bounds...
            {
                //loop around or return original input
                return (loopAtEnd) ? orderedEnumValues[0] : sourceEnumValue;
            }
            else
            {
                return orderedEnumValues[nextEnumIndex];
            }
        }

        /// <summary>
        /// Wrapper for enum Next() function that tries to determine if a next value was actually found
        /// </summary>
        /// <typeparam name="SomeEnum"></typeparam>
        /// <param name="sourceEnumValue"></param>
        /// <param name="nextValue"></param>
        /// <param name="loopAtEnd"></param>
        /// <returns></returns>
        public static bool TryGetNext<SomeEnum>(this SomeEnum sourceEnumValue, out SomeEnum nextValue, bool loopAtEnd = false) where SomeEnum : System.Enum
        {
            nextValue = sourceEnumValue.Next(false);
            return !nextValue.Equals(sourceEnumValue); //if the next value is the same, it did not advance. Failed to actually get next
        }

        /// <summary>
        /// Grabs the previous valid enum value. If looping is disabled and it goes out of bound, it returns the original input (bc structs can't be null).
        /// </summary>
        /// <typeparam name="SomeEnum"></typeparam>
        /// <param name="sourceEnumValue"></param>
        /// <returns></returns>
        public static SomeEnum Prev<SomeEnum>(this SomeEnum sourceEnumValue, bool loopAtStart = false) where SomeEnum : System.Enum
        {
            //Get list of all the enum's values (skips gaps)
            SomeEnum[] orderedEnumValues = (SomeEnum[])Enum.GetValues(sourceEnumValue.GetType());

            //get the array index of enum value that's right after it in the list
            //This is NOT the actual integer associated with the enum value itself, just the index in orderedEnumValues
            int nextEnumIndex = Array.IndexOf<SomeEnum>(orderedEnumValues, sourceEnumValue) - 1;

            if (nextEnumIndex < 0) //if out of bounds...
            {
                //loop around or return original input
                return (loopAtStart) ? orderedEnumValues.Last() : sourceEnumValue;
            }
            else
            {
                return orderedEnumValues[nextEnumIndex];
            }
        }

        /// <summary>
        /// Wrapper for enum Prev() function that tries to determine if a previous value was actually found
        /// </summary>
        /// <typeparam name="SomeEnum"></typeparam>
        /// <param name="sourceEnumValue"></param>
        /// <param name="nextValue"></param>
        /// <param name="loopAtEnd"></param>
        /// <returns></returns>
        public static bool TryGetPrev<SomeEnum>(this SomeEnum sourceEnumValue, out SomeEnum nextValue, bool loopAtEnd = false) where SomeEnum : System.Enum
        {
            nextValue = sourceEnumValue.Prev(false);
            return !nextValue.Equals(sourceEnumValue); //if the next value is the same, it did not advance. Failed to actually get next
        }

        /// <summary>
        /// Grabs the first valid enum value. Due to explicit enum values, this cannot be assumed to simply be the enum value at 0
        /// </summary>
        /// <typeparam name="SomeEnum"></typeparam>
        /// <param name="sourceEnumValue"></param>
        /// <returns></returns>
        public static SomeEnum First<SomeEnum>(this SomeEnum sourceEnumValue) where SomeEnum : System.Enum
        {
            //Get list of all the enum's values (skips gaps)
            SomeEnum[] orderedEnumValues = (SomeEnum[])Enum.GetValues(sourceEnumValue.GetType());

            return orderedEnumValues[0];
        }

        /// <summary>
        /// Grabs the last valid enum value.
        /// </summary>
        /// <typeparam name="SomeEnum"></typeparam>
        /// <param name="sourceEnumValue"></param>
        /// <returns></returns>
        public static SomeEnum Last<SomeEnum>(this SomeEnum sourceEnumValue) where SomeEnum : System.Enum
        {
            //Get list of all the enum's values (skips gaps)
            SomeEnum[] orderedEnumValues = (SomeEnum[])Enum.GetValues(sourceEnumValue.GetType());

            return orderedEnumValues.Last();
        }

        #endregion Enum Movements

        #region DeepClear
        /// <summary>
        /// Bottom Recursive Layer for recursively running DeepClear() on nested collections.
        /// </summary>
        public static void DeepClear<T>(this ICollection<T> collection)
        {
            collection.Clear();
        }

        /// <summary>
        /// Recursively runs Clear on nested collections.
        /// </summary>
        public static void DeepClear<T>(this ICollection<ICollection<T>> collection)
        {
            foreach (ICollection<T> item in collection)
            {
                item.DeepClear();
            }

            collection.Clear();
        }

        /// <summary>
        /// KeyValuePair overload for recursively running Clear() on nested collections.
        /// </summary>
        public static void DeepClear<T, U>(this ICollection<KeyValuePair<T, ICollection<U>>> dictionary)
        {
            foreach (KeyValuePair<T, ICollection<U>> item in dictionary)
            {
                item.Value.DeepClear();
            }

            dictionary.Clear();
        }

        #endregion DeepClear

        #region Arrays
        /// <summary>
        /// Fully clears the array like you would a List or IEnumerable.
        /// </summary>
        /// <remarks>
        /// Wasn't able to find a Clear extension method for arrays, and System.Array.Clear() is needlessly annoying for clearing the whole array.
        /// </remarks>
        /// <typeparam name="T"></typeparam>
        /// <param name="array"></param>
        public static void Clear<T>(this T[] array)
        {
            System.Array.Clear(array, 0, array.Length);
        }
        #endregion
    }
}
