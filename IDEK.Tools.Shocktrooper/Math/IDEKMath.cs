//Created by: Julian Noel

using System.Linq;
using System;

using IDEK.Tools.Logging;
using IDEK.Tools.ShocktroopExtensions;

namespace IDEK.Tools.Math
{
    /// <summary>
    /// IDEK in-house math library
    /// </summary>
    public static class IDEKMath
    {
        public const int MATRIX_X_FILTER_ROW = 0;
        public const int MATRIX_Y_FILTER_ROW = 1;
        public const int MATRIX_Z_FILTER_ROW = 2;
        public const int MATRIX_W_FILTER_ROW = 3;
        
        public const int MATRIX_X_BASIS_COLUMN = 0;
        public const int MATRIX_Y_BASIS_COLUMN = 1;
        public const int MATRIX_Z_BASIS_COLUMN = 2;
        public const int MATRIX_W_BASIS_COLUMN = 3;
        
        public enum NegativeValueHandler { MakePositive, KeepNegative, Truncate, ZeroOut }

        public const int INT_BIT_COUNT = 32;
        public const int LONG_BIT_COUNT = 64;
        public static float Deg2Rad => (float)(System.Math.PI / 180.0);

        #region Inverse Lerp Unclamped
        public static float InverseLerpUnclamped(float a, float b, float value)
        {
            if (a != b)
            {
                return (value - a) / (b - a);
            }

            return 0f;
        }

        #endregion Inverse Lerp Unclamped
        
        #region Bit Rotations

        /// <summary>
        /// Returns a new unsigned 32-bit integer that has been rotated left by a given number of bits. 
        /// (generally making it larger)
        /// </summary> 
        public static UInt32 RotatedLeft(this UInt32 value, int amount)
        {
            if(amount >= INT_BIT_COUNT)
            {
                ConsoleLog.LogError("Attempted to rotate by more bits than are in the value.");
                return 0;
            }
            else
            {
                return (value << amount) | (value >> (INT_BIT_COUNT - amount)); //bitwise OR to wrap the discarded bits
            }
        }

        /// <summary>
        /// Returns a new unsigned 32-bit integer that has been rotated right by a given number of bits. 
        /// (generally making it smaller)
        /// </summary> 
        public static UInt32 RotatedRight(this UInt32 value, int amount)
        {
            if(amount >= INT_BIT_COUNT)
            {
                ConsoleLog.LogError("Attempted to rotate by more bits than are in the value.");
                return 0;
            }
            else
            {
                return (value >> amount) | (value << (INT_BIT_COUNT - amount)); //bitwise OR to wrap the discarded bits
            }
        }

        /// <summary>
        /// Returns a new unsigned 64-bit integer that has been rotated left by a given number of bits. 
        /// (generally making it larger)
        /// </summary> 
        public static UInt64 RotatedLeft(this UInt64 value, int amount)
        {
            if(amount >= LONG_BIT_COUNT)
            {
                ConsoleLog.LogError("Attempted to rotate by more bits than are in the value.");
                return 0;
            }
            else
            {
                return (value << amount) | (value >> (LONG_BIT_COUNT - amount)); //bitwise OR to wrap the discarded bits
            }
        }

        /// <summary>
        /// Returns a new unsigned 64-bit integer that has been rotated right by a given number of bits. 
        /// (generally making it smaller)
        /// </summary> 
        public static UInt64 RotatedRight(this UInt64 value, int amount)
        {
            if(amount >= LONG_BIT_COUNT)
            {
                ConsoleLog.LogError("Attempted to rotate by more bits than are in the value.");
                return 0;
            }
            else
            {
                return (value >> amount) | (value << (LONG_BIT_COUNT - amount)); //bitwise OR to wrap the discarded bits
            }
        }

        #endregion
        
        #region Fun Functions

        /// <summary>
        /// Calculates the sigmoid function. Returns on the range (0, 1). By default it is centered about 0, which defines the mid point. 
        /// </summary>
        /// <param name="x">The input value.</param>
        /// <param name="center">The center value for the sigmoid function. Default is 0.</param>
        /// <returns>The result of the sigmoid function. If x == center, or if steepness == 0f, this function returns 0.5f.</returns>
        public static float Sigmoid(float x, float steepness, float center = 0f)
        {
            if(steepness == 0f)
            {
                throw new DivideByZeroException("Steepness cannot be 0; In addition to being a divide by zero error, its limits diverge; " +
                    "approaching 0 from the left and from the right give contradictory results, and as such, no value can be determined. Please avoid setting steepness to 0!");
            }

            if(float.IsInfinity(steepness)) return 0.5f; //lim of sigmoid as steepness -> +/-infinity is 0.5f in both cases!

            return 1f / (1f + (float)System.Math.Exp((center - x)/steepness));
        }
        
        public static float Lerp(float a, float b, float t) => a + (b - a) * System.Math.Clamp(t, 0f, 1f);

        // public static float FastSmoothLerp(float a, float b, float x) => Mathf.Lerp(a, b, CubicSmooth(x));
        //inlined a little for speed (no extra stack overhead)
        /// <summary>
        /// Performs a FASTER smooth "lerp" from a -> b at x% by using cubic easing
        /// instead of quintic easing - fewer math operations.
        /// <br/>
        /// Can also achieve this with <c>Mathf.Lerp(a, b, CubicSmooth(x)).</c>
        /// </summary>
        /// <remarks>
        /// Unclear exactly how much faster this actually is; the overhead from function
        /// calls may very well be dwarfing the actual different in computation time, but hey,
        /// it's still technically faster.
        /// <para/>
        /// Not technically a "lerp" since that is a LINEAR interpolation and
        /// this is using a nice segment of a cubic curve, not a line.
        /// <br/>
        /// Unfortunately CubicInterp() is long and less clear, and Cerp() just sounds like crap
        /// (and also isn't clear).
        /// </remarks>
        /// <param name="a">start</param>
        /// <param name="b">end</param>
        /// <param name="x">progress % to interpolate between a and b.</param>
        /// <returns></returns>
        public static float FastSmoothLerp(float a, float b, float x) => 
            Lerp(a, b, x * x * (3f - 2f * x));

        public static float CubicSmooth(float x) =>
            x * x * (3f - 2f * x);

        // public static float SmoothLerp(float a, float b, float x) => Mathf.Lerp(a, b, QuinticSmooth(x));
        //inlined a little for speed (no extra stack overhead)
        /// <summary>
        /// Performs a smooth "lerp" from a -> b at x% with quintic (x^5 polynomial) easing.
        /// <br/>
        /// Can also achieve this with <c>Mathf.Lerp(a, b, QuinticSmooth(x))</c>.
        /// </summary>
        /// <remarks>
        /// Not technically a "lerp" since that is a LINEAR interpolation and
        /// this is using a nice segment of a quintic curve, not a line.
        /// <br/>
        /// Unfortunately QuinticInterp() is long and less clear, and Qerp() sounds dumb
        /// (and also like it's maybe Quaternion related instead).
        /// </remarks>
        /// <param name="a">start</param>
        /// <param name="b">end</param>
        /// <param name="x">progress % to interpolate between a and b.</param>
        /// <returns></returns>
        public static float SmoothLerp(float a, float b, float x) => 
            Lerp(a, b, x * x * x * (x * (x * 6f - 15f) + 10f));
        
        public static float QuinticSmooth(float x) => 
            x * x * x * (x * (x * 6f - 15f) + 10f);
        
        #endregion Fun Functions

        #region Statistics

        public static float InsertIntoAverage(float newValue, float originalAverage, int originalCount)
        {
            var newCount = (originalAverage * originalCount) + newValue;
            return newCount / (originalCount + 1);
        }
        public static double InsertIntoAverage(double newValue, double originalAverage, int originalCount)
        {
            var newCount = (originalAverage * originalCount) + newValue;
            return newCount / (originalCount + 1);
        }

        #endregion
            
        //eventually we'll probably want a version of ShocktroopUtils that works outside of Unity. These functions are already prepared for that
        #region Unity-Agnostic Lerp

        public static float LerpUnclamped(float a, float b, float t) => (a + (b - a) * t);

        public static float InverseLerp(float a, float b, float value) => System.Math.Clamp(
            InverseLerpUnclamped(a, b, value), 0f, 1f);

        #endregion
    }
}
