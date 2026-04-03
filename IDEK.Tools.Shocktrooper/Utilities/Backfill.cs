//Created By: Julian Noel on 07/29/2024

using System;
using static IDEK.Tools.ShocktroopUtils.PointerDelegates;

namespace IDEK.Tools.ShocktroopUtils
{
    public static class Backfill
    {
        public static unsafe class Unsafe
        {
            public static unsafe TResult NullPtrCheck<TCheck, TResult>(TCheck* whatToCheck, PtrFunc<TCheck, TResult> getExpected, TResult nullFallback=default) where TCheck : unmanaged
            {
                return whatToCheck != null ? getExpected(whatToCheck) : nullFallback;
            }

            public static unsafe TCheck* BackfillIfNullPtr<TCheck>(TCheck* originalVal, GetPtrFunc<TCheck> deferredBackfillGenerator) where TCheck : unmanaged
            {
                return originalVal == null ? deferredBackfillGenerator() : originalVal;
            }

            public static unsafe TCheck BackfillIfNullPtr<TCheck>(TCheck* originalVal, Func<TCheck> deferredBackfillGenerator) where TCheck : unmanaged
            {
                return originalVal == null ? deferredBackfillGenerator() : *originalVal;
            }

            public static unsafe TCheck* BackfillIfNullPtr<TCheck>(TCheck* originalVal, TCheck* backfillValue) where TCheck : unmanaged
            {
                return originalVal == null ? backfillValue : originalVal;
            }
            public static unsafe TCheck BackfillIfNullPtr<TCheck>(TCheck* originalVal, TCheck backfillValue) where TCheck : unmanaged
            {
                return originalVal == null ? backfillValue : *originalVal;
            }
            public static unsafe void IfNotNullPtr<TCheck>(TCheck* whatToCheck, PtrAction<TCheck> useExpectedValue) where TCheck : unmanaged
            {
                if(whatToCheck != null) useExpectedValue(whatToCheck);
            }
        }

        ///////////////////// Constants /////////////////////
        #region Constants
        #endregion

        ////////////////////// Fields ///////////////////////
        #region Fields
        #endregion

        //////////////////// Properties /////////////////////
        #region Properties
        #endregion

        ////////////////// Public Methods ///////////////////
        #region Public Methods

        
        /// <summary>
        /// Compacted/abstracted handling of nullable objects of type <see cref="T"/>
        /// that either cannot utilize null coalescing (i.e. anything inheriting 
        /// from UnityEngine.Object) or which need to return some 
        /// alternative default value.
        /// </summary>
        /// <typeparam name="TCheck">
        /// Anything nullable will do, as we compare it against <c>null</c>.
        /// </typeparam>
        /// <typeparam name="TResult">
        /// Type of the object being returned. Anything will do.
        /// </typeparam>
        /// <param name="whatToCheck">
        /// What object to perform a <c>null</c> check against.
        /// </param>
        /// <param name="getExpected">
        /// What to return if <see cref="whatToCheck"/> is not <c>null</c>, 
        /// like a member of the checked value, for example.
        /// </param>
        /// <param name="nullFallback">
        /// What to return (instead of <c>null</c>) when <see cref="whatToCheck"/> 
        /// evaluates to <c>null</c>.
        /// </param>
        /// <returns>
        /// Either the value returned from <see cref="getExpected"/> 
        /// or the <see cref="nullFallback"/> value.
        /// </returns> 
        public static TResult NullCheck<TCheck, TResult>(this TCheck whatToCheck, Func<TCheck, TResult> getExpected, TResult nullFallback=default)
        {
            return whatToCheck != null ? getExpected(whatToCheck) : nullFallback;
        }

        public static TCheck BackfillIfNull<TCheck>(this TCheck originalVal, Func<TCheck> deferredBackfillGenerator)
        {
            return originalVal == null ? deferredBackfillGenerator() : originalVal;
        }

        public static TCheck BackfillIfNull<TCheck>(this TCheck originalVal, TCheck backfillValue)
        {
            return originalVal == null ? backfillValue : originalVal;
        }

        public static void IfNotNull<TCheck>(this TCheck whatToCheck, Action<TCheck> useExpectedValue)
        {
            if(whatToCheck != null) useExpectedValue(whatToCheck);
        }


        [Obsolete("Use Backfill.IfNotNull<TCheck>()")]
        public static void NullCheck<TCheck>(this TCheck whatToCheck, Action<TCheck> useExpectedValue) where TCheck : class
        {
            if(whatToCheck != null) useExpectedValue(whatToCheck);
        }

        /// <summary>
        /// Compacted/abstracted handling of values of type <see cref="T"/> that should use a different default value than their type's <c>default</c>.
        /// </summary>
        /// <param name="whatToCheck">
        /// What object to perform a <c>default</c> check against.
        /// </param>
        /// <param name="getExpected">
        /// What to return if <see cref="whatToCheck"/> <c>!= default</c>, 
        /// like a member of the checked value, for example.
        /// </param>
        /// <param name="fallback">
        /// What to return (instead of <c>default</c>) when <see cref="whatToCheck"/> 
        /// evaluates to <c>default</c>.
        /// <para/>
        /// Must define something other than the actual <c>default</c> value, 
        /// otherwise this function is pointless. 
        /// </param>
        /// <typeparam name="TCheck">
        /// Anything nullable will do, as we compare it against <c>null</c>.
        /// </typeparam>
        /// <typeparam name="TResult">
        /// Type of the object being returned. Anything will do.
        /// </typeparam>
        /// <returns>
        /// Either the value returned from <see cref="getExpected"/> 
        /// or the <see cref="fallback"/> value.
        /// </returns> 
        [Obsolete("Use Backfill.BackfillIfDefault<TCheck>()")]
        public static TResult DefaultCheck<TCheck, TResult>(this TCheck whatToCheck, Func<TCheck, TResult> getExpected, TResult fallback) where TCheck : unmanaged
        {
            return Equals(whatToCheck, default) ? getExpected(whatToCheck) : fallback;
        }

        public static TCheck BackfillIfDefault<TCheck>(this TCheck whatToCheck, Func<TCheck, TCheck> deferredBackfillGenerator) where TCheck : unmanaged
        {
            return Equals(whatToCheck, default) ? deferredBackfillGenerator(whatToCheck) : whatToCheck;
        }

        public static TCheck BackfillIfDefault<TCheck>(this TCheck whatToCheck, TCheck backfillValue) where TCheck : unmanaged
        {
            return Equals(whatToCheck, default) ? backfillValue : whatToCheck;
        }

        public static void IfNotDefault<TCheck>(this TCheck whatToCheck, Action<TCheck> useExpectedValue) where TCheck : unmanaged
        {
            if(!Equals(whatToCheck, default)) useExpectedValue(whatToCheck);
        }
        [Obsolete("Use Backfill.IfNotDefault<TCheck>()")]
        public static void DefaultCheck<TCheck>(this TCheck whatToCheck, Action<TCheck> useExpectedValue) where TCheck : unmanaged
        {
            if(Equals(whatToCheck, default)) useExpectedValue(whatToCheck);
        }

        /// <summary>
        /// Compacted/abstracted handling of values of type <see cref="T"/> that should use a different default value than their type's <c>default</c>.
        /// </summary>
        /// <param name="whatToCheck">
        /// What object to perform a <c>default</c> check against.
        /// </param>
        /// <param name="getExpected">
        /// What to return if <see cref="whatToCheck"/> <c>!= default</c>, 
        /// like a member of the checked value, for example.
        /// </param>
        /// <param name="fallback">
        /// What to return (instead of <c>default</c>) when <see cref="whatToCheck"/> 
        /// evaluates to <c>default</c>.
        /// <para/>
        /// Must define something other than the actual <c>default</c> value, 
        /// otherwise this function is pointless. 
        /// </param>
        /// <typeparam name="TCheck">
        /// Anything nullable will do, as we compare it against <c>null</c>.
        /// </typeparam>
        /// <typeparam name="TResult">
        /// Type of the object being returned. Anything will do.
        /// </typeparam>
        /// <returns>
        /// Either the value returned from <see cref="getExpected"/> 
        /// or the <see cref="fallback"/> value.
        /// </returns> 
        public static TResult StructCheck<TCheckStruct, TResult>(this TCheckStruct whatToCheck, Func<TCheckStruct, TResult> getExpected, TResult fallback) where TCheckStruct : struct
        {
            return Equals(whatToCheck, new TCheckStruct()) ? getExpected(whatToCheck) : fallback;
        }

        public static void StructCheck<TCheckStruct>(this TCheckStruct whatToCheck, Action<TCheckStruct> useExpectedValue) where TCheckStruct : struct
        {
            if(Equals(whatToCheck, new TCheckStruct())) useExpectedValue(whatToCheck);
        }

        #endregion

        ////////////////// Private Methods //////////////////
        #region Private Methods
        #endregion

        /////////////////////// Debug ///////////////////////
        #region Debug
        #endregion
    }
}