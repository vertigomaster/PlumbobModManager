//Edited By: Julian Noel

using System.Collections;

namespace IDEK.Tools.DataStructures.Probability
{
    /// <summary>
    /// Given a list of <see cref="WeightedChanceEntry{T}"/>, returns a random
    /// entry value using each value's weight.
    /// </summary>
    /// <typeparam name="T">Type of object to be returned</typeparam>
    [Serializable]
    public class WeightedChance<T> : IEnumerable<WeightedChanceEntry<T>> where T : notnull
    {

#if ODIN_INSPECTOR
        [Sirenix.OdinInspector.ReadOnly]
#endif
        private float totalWeight;

#if ODIN_INSPECTOR
        [Sirenix.OdinInspector.TableList]
#endif
        private List<WeightedChanceEntry<T>> entries;

        private bool hasInitializedPercents = false;
        private Dictionary<T, WeightedChanceEntry<T>> _entryMap = new Dictionary<T, WeightedChanceEntry<T>>();

        public int Count => entries.Count;

        // ------------------------------------------------------------------------------------

        #region Constructors

        public WeightedChance()
        {
            totalWeight = 0;
            entries = new List<WeightedChanceEntry<T>>();
        }

        public WeightedChance(List<WeightedChanceEntry<T>> entryList)
        {
            entries = entryList;

            foreach (WeightedChanceEntry<T> entry in entries)
            {
                _entryMap[entry.value] = entry;
            }

            Initialize();
        }

        #endregion

        // ---

        #region Initialize

        private void Initialize()
        {
            Validate();
        }

        protected void CalculateTotalWeight()
        {
            totalWeight = 0;
            foreach (WeightedChanceEntry<T> entry in entries)
                totalWeight += entry.weight;
        }

        protected void CalculatePercents()
        {
            foreach (WeightedChanceEntry<T> entry in entries)
                entry.Percent = entry.weight / totalWeight;
            hasInitializedPercents = true;
        }

        /// <summary>
        /// Validates and initializes chance percents.
        /// Intended to be called from OnValidate
        /// </summary>
        public void Validate()
        {
            CalculateTotalWeight();
            CalculatePercents();
        }

        #endregion

        // ------------------------------------------------------------------------------------

        #region Info

        public float GetWeight(T item)
        {
            foreach (WeightedChanceEntry<T> entry in entries)
            {
                if (entry.value.Equals(item))
                {
                    return entry.weight;
                }
            }
            return 0;
        }

        public float GetPercent(T item)
        {
            foreach (WeightedChanceEntry<T> entry in entries)
            {
                if (entry.value.Equals(item))
                {
                    return entry.Percent;
                }
            }
            return 0;
        }

        #endregion

        // ------------------------------------------------------------------------------------

        #region Add & Update Entries

        public virtual void Add(T item, float weight)
        {
            WeightedChanceEntry<T> newEntry = new WeightedChanceEntry<T>(weight, item);
            Add(newEntry);
        }

        public void Add(WeightedChanceEntry<T> newEntry)
        {
            entries.Add(newEntry);
            _entryMap[newEntry.value] = newEntry;
            totalWeight += newEntry.weight;
            CalculatePercents();
        }

        /// <summary>
        /// Updates given entry to a new weight
        /// </summary>
        /// <param name="item">Weighted entry value to be updated. Must already exist in the weighted chance</param>
        /// <param name="weight">New weight to be used</param>
        /// <param name="additive">Whether the new weight is applied additively or overwrites the old value</param>
        /// <param name="zeroClamp">If additive, whether or not the new weight should be clamped at zero. 
        ///         If false, will throw an error if the new weight is negative </param>
        /// <exception cref="InvalidOperationException">If given item does not exist or new weight would be negative</exception>
        public void Update(T item, float weight, bool additive = false, bool zeroClamp = true)
        {
            if (_entryMap.TryGetValue(item, out WeightedChanceEntry<T> entry))
            {
                if (additive)
                {
                    if (!zeroClamp && entry.weight + weight < 0)
                    {
                        throw new InvalidOperationException($"Weight cannot be a negative number (new weight calculated was {entry.weight + weight})");
                    }

                    entry.weight += weight;

                    if (entry.weight < 0)
                    {
                        entry.weight = 0;
                    }
                }
                else
                {
                    if (weight < 0)
                    {
                        throw new InvalidOperationException($"Weight cannot be a negative number (new weight given was {weight})");
                    }

                    entry.weight = weight;
                }

                Validate();
                return;
            }

            throw new InvalidOperationException("Item provided does not have an entry to update");
        }

        /// <summary>
        /// Updates given entry with new weight, or adds it if it does not exist
        /// </summary>
        /// <param name="item">Weighted entry value to be updated or add.</param>
        /// <param name="weight">New weight to be used</param>
        /// <param name="additive">Whether the new weight is applied additively or overwrites the old value</param>
        /// <param name="zeroClamp">If additive, whether or not the new weight should be clamped at zero. 
        ///         If false, will throw an error if the new weight is negative </param>
        public void UpdateOrAdd(T item, float weight, bool additive = false, bool zeroClamp = true)
        {
            if (_entryMap.TryGetValue(item, out _))
            {
                Update(item, weight, additive, zeroClamp);
            }
            else
            {
                Add(item, weight);
            }
        }

        #endregion

        // ------------------------------------------------------------------------------------

        #region Get Entries

        /// <summary>
        /// Gets a weighted random entry from the available entries, using a RANDOM seed.
        /// </summary>
        /// <returns>Returns a weighted random entry.</returns>
        public T GetRandomEntry()
        {
            System.Random random = new System.Random();
            return GetRandomEntry(random);
        }

        /// <summary>
        /// Gets a weighted random entry from the available entries, using a GIVEN seed.
        /// </summary>
        /// <param name="random">Random seed to use when selecting an entry.</param>
        /// <returns>Returns a weighted random entry.</returns>
        public T GetRandomEntry(System.Random random)
        {
            if (!hasInitializedPercents)
            {
                CalculateTotalWeight();
                CalculatePercents();
            }

            if (entries.Count == 0)
                throw new InvalidOperationException("Entry list must contain at least one value");
            else if (totalWeight <= 0)
                throw new InvalidOperationException("Total weight of all entries must be greater than zero");

            float chance = (float)random.NextDouble();
            foreach (WeightedChanceEntry<T> entry in entries)
            {
                if (chance <= entry.Percent)
                    return entry.value;
                else
                    chance -= entry.Percent;
            }

            throw new IndexOutOfRangeException();
        }

        /// <summary> Determines whether or not the specified object can be found here.</summary>
        public bool ContainsEntry(T item) => _entryMap.TryGetValue(item, out _);

        #endregion

        #region Operators

        /// <summary>
        /// Enables using [] syntax to retrieve or add WeightedChanceEntry objects. Does not support removal.
        /// </summary>
        public WeightedChanceEntry<T> this[int index]
        {
            get => entries[index];
        }

        /// <summary>
        /// Enables using [] syntax to retrieve or add WeightedChanceEntry objects. Does not support removal.
        /// </summary>
        public WeightedChanceEntry<T> this[T item]
        {
            get
            {
                //Returns null instead of an exception when nothing is found
                _entryMap.TryGetValue(item, out WeightedChanceEntry<T> toReturn);
                return toReturn;
            }
        }

        #endregion Operators

        #region IEnumerable
        public IEnumerator<WeightedChanceEntry<T>> GetEnumerator() => ((IEnumerable<WeightedChanceEntry<T>>)entries).GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)entries).GetEnumerator();

        #endregion

        #region Implementation of ISerializationCallbackReceiver

        /// <inheritdoc />
        public void OnBeforeSerialize()
        {
            Validate();
        }

        /// <inheritdoc />
        public void OnAfterDeserialize()
        {
            Validate();
        }

        #endregion
    }

    [Serializable]
    public class WeightedChanceEntry<T>
    {

#if ODIN_INSPECTOR
        [field: Sirenix.OdinInspector.ReadOnly, Sirenix.OdinInspector.TableColumnWidth(80, resizable: false)]
        [field: HideInInspector]
#endif
        public float Percent { get; set; }

#if ODIN_INSPECTOR
        [Sirenix.OdinInspector.ReadOnly, Sirenix.OdinInspector.ShowInInspector, 
         Sirenix.OdinInspector.PropertyOrder(-100), Sirenix.OdinInspector.TableColumnWidth(80, resizable: false)]
#endif
        public string Probability => Percent.ToString("0.00%");

#if ODIN_INSPECTOR
        [Sirenix.OdinInspector.TableColumnWidth(-100)]
#endif
        public float weight;
        public T value;

        public WeightedChanceEntry(float w, T val)
        {
            if (w < 0)
                throw new InvalidOperationException($"Weight cannot be a negative number (weight given was {w})");

            weight = w;
            value = val;
        }

    }

    /* Example:
     * 
     * WeightedChance<Func<float>> weightedChance = new WeightedChance<Func<float>>(new List<WeightedChanceEntry<Func<float>>> {
     *     new WeightedChanceEntry<Func<float>>(1,    () => { return x; }),
     *     new WeightedChanceEntry<Func<float>>(1,    () => { return x + 2; }),
     *     new WeightedChanceEntry<Func<float>>(2,    () => { return x - 2; }),
     * });
     * return weightedChance.GetRandomEntry().Invoke();
     *  
     */
}