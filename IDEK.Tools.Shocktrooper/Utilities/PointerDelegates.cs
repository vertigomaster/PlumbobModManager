//Created By: Julian Noel on 08/27/2024

namespace IDEK.Tools.ShocktroopUtils
{
    public static unsafe class PointerDelegates
    {
        ////////////////// Delegates ///////////////////
        #region Delegates
        
        #region PtrFuncs
        public delegate TOut* GetPtrFunc<TOut>() where TOut : unmanaged;
        public delegate TOut PtrFunc<T1, TOut>(T1* param1) where T1 : unmanaged;

        public delegate TOut PtrFunc<T1, T2, TOut>(T1* param1, T2* param2) 
            where T1 : unmanaged 
            where T2 : unmanaged;
        public delegate TOut PtrFunc<T1, T2, T3, TOut>(T1* param1, T2* param2, T2* param3) 
            where T1 : unmanaged 
            where T2 : unmanaged 
            where T3 : unmanaged;
        #endregion

        #region PtrActions
        public delegate void PtrAction<T1>(T1* param1) where T1 : unmanaged;

        public delegate void PtrAction<T1, T2>(T1* param1, T2* param2) 
            where T1 : unmanaged 
            where T2 : unmanaged;
        public delegate void PtrAction<T1, T2, T3, TOut>(T1* param1, T2* param2, T2* param3) 
            where T1 : unmanaged 
            where T2 : unmanaged 
            where T3 : unmanaged;
        #endregion

        //TODO: low prio, but should eventually prob make overloads up to 4 arguments (like System.Action and System.Func do)

        #endregion

        ////////////////// Private Methods //////////////////
        #region Private Methods

        #endregion
    }
}