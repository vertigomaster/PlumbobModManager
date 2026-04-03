using System;
using System.Collections.Generic;
using IDEK.Tools.ShocktroopUtils;
using System.Collections;
using System.Linq;

#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

namespace IDEK.Tools.DataStructures
{
    
    /// <summary>
    /// A list structure which automatically marks a flag (or fires a callback) once it has been mutated.
    /// Call Clean() to clear the flag in order to detect furthe rchanges.  
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Serializable]
    public class CleanableList<T> : ICleanable, IEnumerable<T>, IList<T>, IReadOnlyList<T>//, ISerializationCallbackReceiver
    {
        private List<T> elements = new();

        private StandardCleanableImpl _cleaner = new();

        [System.Flags]
        public enum ChangeMode 
        { 
            None = 0, 
            Add = 1 >> 1, 
            Remove = 1 >> 2, 
            Insert = 1 >> 3, 
            Reorder = 1 >> 4, 
            Single, Multiple, All 
        }

        public CleanableList() { }
        public CleanableList(IEnumerable<T> collection)
        {
            elements = collection.ToList();
        }
        public CleanableList(IEnumerable<T> collection, CleanableList<T> original) : this(collection)
        {
            //the "as" keyword can return null if the match fails, which is undesirable.
            _cleaner = (original._cleaner.Clone() as StandardCleanableImpl) ?? new StandardCleanableImpl();
        }

        #region ICleanable Implementation

        public Action DirtiedEvent
        {
            get => _cleaner.DirtiedEvent;
            set => _cleaner.DirtiedEvent = value;
        }

        public Action ChangedEvent
        {
            get => _cleaner.ChangedEvent;
            set => _cleaner.ChangedEvent = value;
        }

        [Obsolete("This will be removed in a future version. Use ChangeCallbacksEnabled instead.")]
        public bool ChangeCallbacksDisabled
        {
            get => !ChangeCallbacksEnabled;
            set => ChangeCallbacksEnabled = !value;
        }


        public bool ChangeCallbacksEnabled
        {
            get => _cleaner.ChangeCallbacksEnabled;
            set => _cleaner.ChangeCallbacksEnabled = value;
        }

#if ODIN_INSPECTOR
        [ShowInInspector, ReadOnly]
#endif
        public bool IsDirty => _cleaner.IsDirty;

        public bool IsClean => !_cleaner.IsDirty;

        public void Clean() => _cleaner.Clean();

        public bool TryMarkDirty() => _cleaner.TryMarkDirty();

        /// <summary>
        /// Marks as dirty, resulting in the callback
        /// </summary>
        public virtual void MarkDirty() => _cleaner.MarkDirty();

        public bool MarkDirtyIf(bool condition) => _cleaner.MarkDirtyIf(condition);

        #endregion
        
        



        public int Count => elements?.Count ?? 0;
        public int Capacity => elements?.Capacity ?? 0;

        public IReadOnlyList<T> ReadOnlyElements => Elements;

        protected List<T> Elements
        {
            get
            {
                if(elements == null) elements = new();
                return elements;
            }
        }

        public bool IsReadOnly => ((ICollection<T>)Elements).IsReadOnly;
        
        

        public T this[int index]
        {
            get => Elements[index];
            set
            {
                bool isSame = value.Equals(Elements[index]);
                Elements[index] = value;
                MarkDirtyIf(!isSame);
            }
        }

        public void Add(T item) 
        {
            Elements.Add(item); 
            MarkDirty();
        }

        public void AddRange(IEnumerable<T> collection) {
            Elements.AddRange(collection); MarkDirty();
        }

        public void Clear() {
            bool alreadyEmpty = Count == 0;
            Elements.Clear(); 
            MarkDirtyIf(!alreadyEmpty); 
        }

        public new void Insert(int index, T item)
        {
            bool insertedValueIsSame = index >= 0 && index < Count && item.Equals(this[index]);

            Elements.Insert(index, item); 
            MarkDirtyIf(!insertedValueIsSame);
        }

        public new void InsertRange(int index, IEnumerable<T> collection)
        {
            //TODO: add similar check for equivalence to avoid false positives

            Elements.InsertRange(index, collection); 
            MarkDirty();
        }

        public new bool Remove(T item) => MarkDirtyIf(Elements.Remove(item));

        public new int RemoveAll(Predicate<T> match)
        {
            int val = Elements.RemoveAll(match);
            MarkDirtyIf(val > 0);
            return val;
        }

        public new void RemoveAt(int index) {
            Elements.RemoveAt(index); MarkDirty();
        }

        public new void RemoveRange(int index, int count) {
            Elements.RemoveRange(index, count); MarkDirty();
        }

        public new void Reverse(int index, int count) {
            Elements.Reverse(index, count); MarkDirty();
        }

        public new void Reverse() { 
            Elements.Reverse(); MarkDirty();
        }

        public new void Sort(Comparison<T> comparison)
        {
            Elements.Sort(comparison); MarkDirty();
        }
        public new void Sort(int index, int count, IComparer<T> comparer) {
            Elements.Sort(index, count, comparer); MarkDirty();
        }

        public new void Sort() {
            Elements.Sort(); MarkDirty();
        }

        public new void Sort(IComparer<T> comparer) { 
            Elements.Sort(comparer); MarkDirty();
        }

        public new void TrimExcess() {
            Elements.TrimExcess(); MarkDirty();
        }

        public void SetList(IEnumerable<T> enumerable)
        {
            elements = enumerable.ToList();
            MarkDirty();
        }

        public IEnumerator<T> GetEnumerator() => Elements.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        public int IndexOf(T item) => Elements.IndexOf(item);
        public bool Contains(T item) => Elements.Contains(item);
        public void CopyTo(T[] array, int arrayIndex) => Elements.CopyTo(array, arrayIndex);
    }
}
