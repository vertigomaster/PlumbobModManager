namespace IDEK.Tools.Math.Probabilities
{
    /// <summary>Angle-based yes/no predicate. The caller is in charge of LUT (Look-Up-Table) creation.</summary>
    /// <remarks>A LUT translates given angle values to probability values, which are then "rolled" like virtual dice</remarks>
    public class AnglePredicateCurve
    {
        /// <summary>
        /// Stores an approximation of an arbitrary curve function as a table of dynamic size
        /// </summary>
        protected float[] lut;

        protected float _maxAngle;
        protected float _minAngle;
        
        /// <summary>
        /// Tells us the largest angle input supported by the curve
        /// </summary>
        /// <remarks>
        /// Is used to map an input angle to the correct table index.
        /// </remarks>
        public virtual float MaxAngle
        {
            get => _maxAngle;
            protected set => _maxAngle= value;
        }
        
        /// <summary>
        /// Tells us the smallest angle input supported by the curve
        /// </summary>
        /// <remarks>
        /// Is used to map an input angle to the correct table index.
        /// </remarks>
        public virtual float MinAngle
        {
            get => _minAngle;
            protected set => _minAngle = value;
        }
        
        /// <summary>
        /// tells us how granular the data is.
        /// </summary>
        /// <remarks>
        /// Is used to map an input angle to the correct table index.
        /// </remarks>
        public int LutResolution => lut.Length;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="angleToProbabilityLookupTable">
        /// A prebuilt array of floats acting as a LUT for this helper class.
        /// </param>
        /// <param name="lutMinAngle">Together with <see cref="lutMaxAngle"/>, defines the domain of the LUT.</param>
        /// <param name="lutMaxAngle">Together with <see cref="lutMinAngle"/>, defines the domain of the LUT.</param>
        /// <exception cref="ArgumentNullException">Thrown if a non-null LUT array is supplied.</exception>
        /// <exception cref="ArgumentException">lutMinAngle must be less than lutMaxAngle to be valid.</exception>
        public AnglePredicateCurve(float[] angleToProbabilityLookupTable, float lutMinAngle, float lutMaxAngle)
        {
            lut = angleToProbabilityLookupTable ?? 
                  throw new ArgumentNullException(nameof(angleToProbabilityLookupTable));
            
            if (lutMinAngle > lutMaxAngle) throw new ArgumentException(
                "lutMinAngle must be less than lutMaxAngle to be valid");
            
            _minAngle = lutMinAngle;
            _maxAngle = lutMaxAngle;
        }

        public float GetProbability(float angleDeg)
        {
            float normalizedAngleDeg = IDEKMath.InverseLerp(MinAngle, MaxAngle, angleDeg);
            int lutIndex = System.Math.Clamp((int)System.Math.Round(normalizedAngleDeg * LutResolution), 0,
                lut.Length - 1);
            return lut[lutIndex];
        }
        
        /// <summary>
        /// Taking in your angle, rolls a virtual die using probabilities from the given LUT
        /// to determine a boolean yes/no. 
        /// </summary>
        /// <param name="angleDeg"></param>
        /// <param name="rng">
        /// A specific random number generator to use. It must return on [0,1) in order to function properly.
        /// Would generally suggest using a uniform distribution, but that's really up to you.
        /// <para/>
        /// If you supply a <c>null</c> random number generator, we fall back to creating a
        /// new <see cref="System.Random"/> object, as that is the simplest thread-safe(?)
        /// thing we can do. Highly recommend supplying your own RNG,
        /// especially if your engine has one.
        /// </param>
        /// <returns>True if success, false if failed.</returns>
        /// <remarks>
        /// If you have your own RNG/evaluation system, you can have your custom code instead
        /// call <see cref="GetProbability"/>; all <see cref="Roll"/> does is compare that probability
        /// value against a RNG value on the domain [0,1). 
        /// </remarks>
        public virtual bool Roll(float angleDeg, Func<float> rng=null)
        {
            float randomNumber = rng?.Invoke() ?? (float)(new Random().NextDouble());
            return randomNumber < GetProbability(angleDeg);
        }
        
        // private float 
    }
}