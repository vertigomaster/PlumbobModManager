//Created By: Julian Noel on 07/31/2024
using System;

namespace IDEK.Tools.ShocktroopUtils
{
    public static class EnumUtils
    {
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

        public static TEnum GetRandomVal<TEnum>() where TEnum : Enum
        {
            // Get all possible values of the enum
            Array values = Enum.GetValues(typeof(TEnum));

            // Select a random value from the array
            return (TEnum)values.GetValue(Random.Shared.Next(0, values.Length))! 
                ?? throw new InvalidOperationException();
        }

        #endregion

        ////////////////// Private Methods //////////////////
        #region Private Methods
        #endregion
    }
}