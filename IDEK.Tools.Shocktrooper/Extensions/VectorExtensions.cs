//Created by: Julian Noel

using System.Numerics;
using System.Runtime.CompilerServices;
using IDEK.Tools.Math;

namespace IDEK.Tools.ShocktroopExtensions
{
    public static class VectorExtensions
    {
        #region Update Single Component (Self)
        #region Vec 2
        
        /// <summary>
        /// Modifies the variable's own X component. 
        /// Does not work with properties. use WithX() instead
        /// To preserve the vector.
        /// </summary>
        /// <param name="vec"></param>
        /// <param name="newX"></param>
        public static void SetX(this ref Vector2 vec, float newX) 
        {
            vec = new Vector2(newX, vec.Y);
        }

        /// <summary>
        /// Modifies the variable's own Y component. 
        /// Does not work with properties. use WithY() instead
        /// To preserve the vector.
        /// </summary>
        /// <param name="vec"></param>
        /// <param name="newY"></param>
        public static void SetY(this ref Vector2 vec, float newY) 
        {
            vec = new Vector2(vec.X, newY);
        }

        #endregion Vec 2
        #region Vec 3

        //TODO: aggressive inlining
        /// <summary>
        /// Modifies the variable's own X component. 
        /// Does not work with properties. use WithX() instead
        /// To preserve the vector.
        /// </summary>
        /// <param name="vec"></param>
        /// <param name="newX"></param>
        public static void SetX(this ref Vector3 vec, float newX) 
        {
            vec = new Vector3(newX, vec.Y, vec.Z);
        }


        /// <summary>
        /// Modifies the variable's own Y component. 
        /// Does not work with properties. use WithY() instead
        /// To preserve the vector.
        /// </summary>
        /// <param name="vec"></param>
        /// <param name="newY"></param>
        public static void SetY(this ref Vector3 vec, float newY) 
        {
            vec = new Vector3(vec.X, newY, vec.Z);
        }

        /// <summary>
        /// Modifies the variable's own Y component. 
        /// Does not work with properties. use WithY() instead
        /// To preserve the vector.
        /// </summary>
        /// <param name="vec"></param>
        /// <param name="newY"></param>
        public static void SetZ(this ref Vector3 vec, float newZ) 
        {
            vec = new Vector3(vec.X, vec.Y, newZ);
        }

        #endregion Vec3
        #region Vec4

        //vec4 overloads

        /// <summary>
        /// Modifies the variable's own X component. 
        /// Does not work with properties. use WithX() instead
        /// To preserve the vector.
        /// </summary>
        /// <param name="vec"></param>
        /// <param name="newX"></param>
        public static void SetX(this ref Vector4 vec, float newX) 
        {
            vec = new Vector4(newX, vec.Y, vec.Z, vec.W);
        }

        /// <summary>
        /// Modifies the variable's own Y component. 
        /// Does not work with properties. use WithY() instead
        /// To preserve the vector.
        /// </summary>
        /// <param name="vec"></param>
        /// <param name="newY"></param>
        public static void SetY(this ref Vector4 vec, float newY) 
        {
            vec = new Vector4(vec.X, newY, vec.Z, vec.W);
        }

        /// <summary>
        /// Modifies the variable's own Z component. 
        /// Does not work with properties. use WithZ() instead
        /// To preserve the vector.
        /// </summary>
        /// <param name="vec"></param>
        /// <param name="newZ"></param>
        public static void SetZ(this ref Vector4 vec, float newZ) 
        {
            vec = new Vector4(vec.X, vec.Y, newZ, vec.W);
        }

        /// <summary>
        /// Modifies the variable's own W component. 
        /// Does not work with properties. use WithW() instead
        /// To preserve the vector.
        /// </summary>
        /// <param name="vec"></param>
        /// <param name="newW"></param>
        public static void SetW(this ref Vector4 vec, float newW) 
        {
            vec = new Vector4(vec.X, vec.Y, vec.Z, newW);
        }

        #endregion Vec 4
        #endregion Update Single Component (Self)

        #region Non Destructive Single Component Updates 
        #region Vec 2
        
        /// <summary>
        /// returns the vector with with modified component.
        /// </summary>
        public static Vector2 WithX(this Vector2 vec, float newX) 
        {
            vec.SetX(newX);
            return vec;
        }

        /// <summary>
        /// returns the vector with with modified component.
        /// </summary>
        public static Vector2 WithY(this Vector2 vec, float newY) 
        {
            vec.SetY(newY);
            return vec;
        }

        #endregion Vec 2
        #region Vec 3

        /// <summary>
        /// returns the vector with with modified component.
        /// </summary>
        public static Vector3 WithX(this Vector3 vec, float newX) 
        {
            vec.SetX(newX);
            return vec;
        }

        /// <summary>
        /// returns the vector with with modified component.
        /// </summary>
        public static Vector3 WithY(this Vector3 vec, float newY) 
        {
            vec.SetY(newY);
            return vec;
        }

        /// <summary>
        /// returns the vector with with modified component.
        /// </summary>
        public static Vector3 WithZ(this Vector3 vec, float newZ) 
        {
            vec.SetZ(newZ);
            return vec;
        }

        #endregion Vec3
        #region Vec4

        /// <summary>
        /// returns the vector with with modified component.
        /// </summary>
        public static Vector4 WithX(this Vector4 vec, float newX) 
        {
            vec.SetX(newX);
            return vec;
        }

        /// <summary>
        /// returns the vector with with modified component.
        /// </summary>
        public static Vector4 WithY(this Vector4 vec, float newY) 
        {
            vec.SetY(newY);
            return vec;
        }

        /// <summary>
        /// returns the vector with with modified component.
        /// </summary>
        public static Vector4 WithZ(this Vector4 vec, float newZ) 
        {
            vec.SetZ(newZ);
            return vec;        
        }

        /// <summary>
        /// returns the vector with with modified component.
        /// </summary>
        public static Vector4 WithW(this Vector4 vec, float newW) 
        {
            vec.SetW(newW);
            return vec;        
        }

        #endregion Vec 4
        #endregion Non Destructive Single Component Updates 

        #region Inverse Vectors
        
        /// <summary>
        /// Returns the component-wise inverse of the vector
        /// </summary>
        public static Vector2 Inverse(this Vector2 v)
        {
            return new Vector2(1 / v.X, 1 / v.Y);
        }

        /// <summary>
        /// Returns the component-wise inverse of the vector
        /// </summary>
        public static Vector3 Inverse(this Vector3 v)
        {
            return new Vector3(1 / v.X, 1 / v.Y, 1 / v.Z);
        }

        /// <summary>
        /// Returns the component-wise inverse of the vector
        /// </summary>
        public static Vector4 Inverse(this Vector4 v)
        {
            return new Vector4(1 / v.X, 1 / v.Y, 1 / v.Z, 1 / v.W);
        }
        #endregion Inverse Vectors

        #region Mult

        /// <summary>
        /// Component-wise multiplies a and b, like Scale() does, but returns a new copy.
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public static Vector2 Mult(this Vector2 a, Vector2 b)
        {
            return new(a.X * b.X, a.Y * b.Y);
        }

        /// <summary>
        /// Component-wise multiplies a and b, like Scale() does, but returns a new copy.
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public static Vector3 Mult(this Vector3 a, Vector3 b)
        {
            return new(a.X * b.X, a.Y * b.Y, a.Z * b.Z);
        }


        /// <summary>
        /// Component-wise multiplies a and b, like Scale() does, but returns a new copy.
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public static Vector4 Mult(this Vector4 a, Vector4 b)
        {
            return new(a.X * b.X, a.Y * b.Y, a.Z * b.Z, a.W * b.W);
        }

        #endregion

        #region MaxComponent

        public static float MaxComponent(this Vector2 vec)
        {
            return System.Math.Max(vec.X, vec.Y);
        }

        public static float MaxComponent(this Vector3 vec)
        {
            return System.Math.Max(System.Math.Max(vec.X, vec.Y), vec.Z);
        }

        public static float MaxComponent(this Vector4 vec)
        {
            return System.Math.Max(System.Math.Max(vec.X, vec.Y), System.Math.Max(vec.Z, vec.W));
        }

        #endregion MaxComponent

        #region MinComponent

        public static float MinComponent(this Vector2 vec)
        {
            return System.Math.Min(vec.X, vec.Y);
        }

        public static float MinComponent(this Vector3 vec)
        {
            return System.Math.Min(
                vec.X, 
                System.Math.Min(vec.Y, vec.Z));
        }

        public static float MinComponent(this Vector4 vec)
        {
            return System.Math.Min(
                System.Math.Min(vec.X, vec.Y), 
                System.Math.Min(vec.Z, vec.W));
        }

        #endregion MinComponent

        #region Abs

        public static Vector2 Abs(this Vector2 vec)
        {
            vec.X = System.Math.Abs(vec.X);
            vec.Y = System.Math.Abs(vec.Y);
            return vec; //it's already a copied value
        }

        public static Vector3 Abs(this Vector3 vec)
        {
            vec.X = System.Math.Abs(vec.X);
            vec.Y = System.Math.Abs(vec.Y);
            vec.Z = System.Math.Abs(vec.Z);
            return vec; //it's already a copied value
        }

        public static Vector4 Abs(this Vector4 vec)
        {
            vec.X = System.Math.Abs(vec.X);
            vec.Y = System.Math.Abs(vec.Y);
            vec.Z = System.Math.Abs(vec.Z);
            vec.W = System.Math.Abs(vec.W);
            return vec; //it's already a copied value
        }

        #endregion MaxComponent

        #region Average

        public static Vector3 Average(params Vector3[] vectors)
        {
            Vector3 sum = Vector3.Zero;

            for(int i = 0; i < vectors.Length; i++)
            {
                sum += vectors[i];
            }

            return sum / vectors.Length;
        }

        #endregion Average

        #region Scale To Magnitude
        
        public static Vector2 ScaleToMagnitude(this Vector2 vec, float newMagnitude)
        {
            return Vector2.Normalize(vec) * newMagnitude;
        }

        public static Vector3 ScaleToMagnitude(this Vector3 vec, float newMagnitude)
        {
            return Vector3.Normalize(vec) * newMagnitude;
        }
        
        public static Vector4 ScaleToMagnitude(this Vector4 vec, float newMagnitude)
        {
            return Vector4.Normalize(vec) * newMagnitude;
        }

        #endregion Scale To Magnitude

        #region Clamp To Magnitude

        public static Vector2 ClampMagnitude(this Vector2 vector, float maxMagnitude = 1f, float minMagnitude = 0f)
        {
            float sqrMag = vector.LengthSquared();

            if(minMagnitude > 0f && sqrMag < minMagnitude * minMagnitude)
            {
                return vector.ScaleToMagnitude(minMagnitude);
            }
            else if(sqrMag > maxMagnitude * maxMagnitude)
            {
                return vector.ScaleToMagnitude(maxMagnitude);
            }

            return vector;
        }

        public static Vector3 ClampMagnitude(this Vector3 vector, float maxMagnitude = 1f, float minMagnitude = 0f)
        {
            float sqrMag = vector.LengthSquared();

            if(minMagnitude > 0f && sqrMag < minMagnitude * minMagnitude)
            {
                return vector.ScaleToMagnitude(minMagnitude);
            }
            else if(sqrMag > maxMagnitude * maxMagnitude)
            {
                return vector.ScaleToMagnitude(maxMagnitude);
            }

            return vector;
        }

        public static Vector4 ClampMagnitude(this Vector4 vector, float maxMagnitude = 1f, float minMagnitude = 0f)
        {
            float sqrMag = vector.LengthSquared();

            if(minMagnitude > 0f && sqrMag < minMagnitude * minMagnitude)
            {
                return vector.ScaleToMagnitude(minMagnitude);
            }
            else if(sqrMag > maxMagnitude * maxMagnitude)
            {
                return vector.ScaleToMagnitude(maxMagnitude);
            }

            return vector;
        }

        #endregion Clamp To Magnitude

        #region Component Products

        /// <summary> abs(x * y)</summary>
        public static float UnsignedComponentProduct(this Vector2 vec) => System.Math.Abs(vec.X * vec.Y);

        /// <summary> abs(x * y * z)</summary>
        public static float UnsignedComponentProduct(this Vector3 vec) => System.Math.Abs(vec.X * vec.Y * vec.Z);

        /// <summary> abs(x * y * z * w)</summary>
        public static float UnsignedComponentProduct(this Vector4 vec) => System.Math.Abs(vec.X * vec.Y * vec.Z * vec.W);

        /// <summary> x * y </summary>
        public static float SignedComponentProduct(this Vector2 vec) => vec.X * vec.Y;

        /// <summary> x * y * z </summary>
        public static float SignedComponentProduct(this Vector3 vec) => vec.X * vec.Y * vec.Z;

        /// <summary> x * y * z * w</summary>
        public static float SignedComponentProduct(this Vector4 vec) => vec.X * vec.Y * vec.Z * vec.W;

        #endregion Component Products

        #region Areas and Volumes

        /// <summary>
        /// The SIGNED area of a parallelogram/rectangle with the this vector's width/height
        /// </summary>
        /// <param name="vec"></param>
        /// <returns></returns>
        public static float SignedArea(this Vector2 vec) => SignedComponentProduct(vec);

        /// <summary>
        /// The SIGNED volume of a cuboid with the this vector's width/height/depth
        /// </summary>
        /// <param name="vec"></param>
        /// <returns></returns>
        public static float SignedVolume(this Vector3 vec) => SignedComponentProduct(vec);

        /// <summary>
        /// The SIGNED measure/4-volume/"hypervolume" of a 4-cuboid with the this vector's scale
        /// </summary>
        /// <param name="vec"></param>
        /// <returns></returns>
        public static float SignedHypervolume(this Vector4 vec) => SignedComponentProduct(vec);

        /// <summary>
        /// The SIGNED area of a parallelogram/rectangle with the this vector's width/height
        /// </summary>
        /// <param name="vec"></param>
        /// <returns></returns>
        public static float UnsignedArea(this Vector2 vec) => UnsignedComponentProduct(vec);

        /// <summary>
        /// The SIGNED volume of a cuboid with the this vector's width/height/depth
        /// </summary>
        /// <param name="vec"></param>
        /// <returns></returns>
        public static float UnsignedVolume(this Vector3 vec) => UnsignedComponentProduct(vec);

        /// <summary>
        /// The SIGNED measure/4-volume/"hypervolume" of a 4-cuboid with the this vector's scale
        /// </summary>
        /// <param name="vec"></param>
        /// <returns></returns>
        public static float UnsignedHyperVolume(this Vector4 vec) => UnsignedComponentProduct(vec);
        
        #endregion Areas and Volumes
        
        #region ProjectOnVector

        /// <summary>
        /// Extension Wrapper for Vector2.Project().
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 Project(this Vector2 vector, Vector2 ontoVector) 
        {
            //apparently v2 doesn't have it
            float num = Vector2.Dot(ontoVector, ontoVector);
            if (num < float.Epsilon)
            {
                return Vector2.Zero;
            }

            float num2 = Vector2.Dot(vector, ontoVector);
            return new Vector2(
                ontoVector.X * num2 / num, 
                ontoVector.Y * num2 / num);
        }

        // /// <summary>
        // /// Extension Wrapper for Vector3.Project().
        // /// </summary>
        // [MethodImpl(MethodImplOptions.AggressiveInlining)]
        // public static Vector3 Project(this Vector3 vector, Vector3 onNormal) => Vector3.Project(vector, onNormal);
        //
        // /// <summary>
        // /// Extension Wrapper for Vector4.Project().
        // /// </summary>
        // [MethodImpl(MethodImplOptions.AggressiveInlining)]
        // public static Vector4 Project(this Vector4 vector, Vector4 onNormal) => Vector4.Project(vector, onNormal);

        #endregion ProjectOnVector
        
        #region RectifiedProjectOnVector

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Rectify(this float num) => System.Math.Max(0f, num);

        /// <summary>
        /// this approach applies something analogous to a rectify operator when processing the projection
        /// i.e., only the components contributing to the direction are considered
        /// so if Dot(vector, ontoVector) <= 0, the rectified projection will be the zero vector.
        /// </summary>
        /// <param name="vector"></param>
        /// <param name="ontoVector"></param>
        /// <returns></returns>
        /// <remarks>
        /// This method was originally written to account for gamified terminal velocity calculations
        /// Wherein only the magnitude of the DOWNWARD velocity would be considered,
        /// so a simple Projection was insuficient. While a dot product check could be added,
        /// the code was on a hot path, so it was worth considering that
        /// the underlying projection calculations already produce a Dot product, so doing so
        /// after Project() would perform another redundant dot product.
        /// This implementation manually performs the underlying projection math instead of directly handing the extension off to Unity's VectorX libraries. 
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 RectifiedProject(this Vector3 vector, Vector3 onNormal)
        {
            //if onNormal is practically zero, projection is also zero.
            float normalSqrMag = Vector3.Dot(onNormal, onNormal);
            if (normalSqrMag < float.Epsilon)
                return Vector3.Zero;
            
            //dot = ||a|| * ||b|| * cos(angle between). Positive is constructive
            //if the vectors are opposing, this value will be negative, which we don't want.
            float recifiedMagProduct = Vector3.Dot(vector, onNormal).Rectify();
            //to rectify the projection, we only consider constructive interference
            //and zero out any destructive interference.
            return new Vector3(
                onNormal.X * recifiedMagProduct / normalSqrMag, 
                onNormal.Y * recifiedMagProduct / normalSqrMag, 
                onNormal.Z * recifiedMagProduct / normalSqrMag);
        }
        
        #endregion RectifiedProjectOnVector

        #region ProjectOnPlane

        /// <summary>
        /// Extension for projecting onto a line defined by a normal (same thing as Vector2.Project(vector, somethingOrthonalToTheNormal))
        /// </summary>
        public static Vector2 ProjectOnLine(this Vector2 vector, Vector2 lineNormal) 
        {
            float num = Vector2.Dot(lineNormal, lineNormal);
            if (num < float.Epsilon)
            {
                return vector;
            }

            float num2 = Vector2.Dot(vector, lineNormal);
            return new Vector2(
                vector.X - lineNormal.X * num2 / num, 
                vector.Y - lineNormal.Y * num2 / num);
        }
        //
        // /// <summary>
        // /// Extension Wrapper for Vector3.ProjectOnPlane().
        // /// </summary>
        // public static Vector3 ProjectOnPlane(this Vector3 vector, Vector3 planeNormal) => 
        //     Vector3.ProjectOnPlane(vector, planeNormal);

        /// <summary>
        /// Extension for projecting a vector4 onto a line defined by a hyperplane 
        /// </summary>
        /// <remarks>
        /// "WTF is a Hyperplane?": An oriented 3D "subspace" that divides 4D space into two parts, perfectly analogous to an infinite plane in 3D space;
        /// <br/>
        /// A plane in 3D space is an oriented 2D "subspace" that divides 3D space into two parts.
        /// </remarks>
        public static Vector4 ProjectOnHyperplane(this Vector4 vector, Vector4 hyperPlaneNormal)
        {
            float num = Vector4.Dot(hyperPlaneNormal, hyperPlaneNormal);
            if (num < float.Epsilon)
            {
                return vector;
            }

            float num2 = Vector4.Dot(vector, hyperPlaneNormal);
            return new Vector4(
                vector.X - hyperPlaneNormal.X * num2 / num, 
                vector.Y - hyperPlaneNormal.Y * num2 / num, 
                vector.Z - hyperPlaneNormal.Z * num2 / num, 
                vector.W - hyperPlaneNormal.W * num2 / num);
        }

        #endregion ProjectOnPlane
        
        #region RandomInRange

        /// <summary>
        /// Returns a random number between x and y using UnityEngine.Random
        /// </summary>
        public static float GetRandomInRange(this Vector2 vector)
        {
            float t = Random.Shared.NextSingle();
            return IDEKMath.Lerp(vector.X, vector.Y, t);            
        }
        
        #endregion RandomInRange
    }
}
