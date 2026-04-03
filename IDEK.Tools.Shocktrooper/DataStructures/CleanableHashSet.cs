using IDEK.Tools.ShocktroopUtils;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
// using JetBrains.Annotations;

namespace IDEK.Tools.DataStructures
{
    public class CleanableHashSet<T> : HashSet<T>, ICleanable
    {
        private StandardCleanableImpl _cleaner = new();

        #region ICleanable
        /// <inheritdoc />
        public Action ChangedEvent
        {
            get => _cleaner.ChangedEvent;
            set => _cleaner.ChangedEvent = value;
        }

        /// <inheritdoc />
        public Action DirtiedEvent
        {
            get => _cleaner.DirtiedEvent;
            set => _cleaner.DirtiedEvent = value;
        }

        /// <inheritdoc />
        public bool IsDirty => _cleaner.IsDirty;

        /// <inheritdoc />
        public void Clean() { _cleaner.Clean(); }

        /// <inheritdoc />
        public bool ChangeCallbacksEnabled { get; protected set; } = true;

        /// <inheritdoc />
        public bool TryMarkDirty() { return _cleaner.TryMarkDirty(); }

        /// <inheritdoc />
        public void MarkDirty() { _cleaner.MarkDirty(); }

        /// <inheritdoc />
        public bool MarkDirtyIf(bool condition) { return _cleaner.MarkDirtyIf(condition); }

        #endregion
        
        #region HashSet Implementation

        public CleanableHashSet() { }

        public CleanableHashSet([NotNull] IEnumerable<T> collection) : base(collection) { }

        public CleanableHashSet([NotNull] IEnumerable<T> collection, IEqualityComparer<T> comparer) : base(collection, comparer) { }

        public CleanableHashSet(IEqualityComparer<T> comparer) : base(comparer) { }

        public CleanableHashSet(int capacity) : base(capacity) { }

        public CleanableHashSet(int capacity, IEqualityComparer<T> comparer) : base(capacity, comparer) { }

        [Obsolete("Obsolete")]
        protected CleanableHashSet(SerializationInfo info, StreamingContext context) : base(info, context) { }

        public new bool Add(T item) => MarkDirtyIf(base.Add(item));
        public new bool Remove(T item) => MarkDirtyIf(base.Remove(item));

        public new void ExceptWith(IEnumerable<T> other)
        {
            int oldCount = Count;
            base.ExceptWith(other);
            MarkDirtyIf(oldCount != Count);
        }

        public new void Clear()
        {
            int oldCount = Count;
            base.Clear();
            MarkDirtyIf(oldCount != Count);
        }

        public new int RemoveWhere(Predicate<T> match)
        {
            int succ = base.RemoveWhere(match);
            MarkDirtyIf(succ > 0);
            return succ;
        }

        //TODO: implement the other hashset edit functions after confirming whether or not they already call Add or Remove()
        //public new void IntersectWith(IEnumerable<T> other);
        //public void TrimExcess();
        //public void UnionWith(IEnumerable<T> other);

        public override int GetHashCode() => HashCode.Combine(Comparer, Count, ChangedEvent, IsDirty);
        #endregion
    }
}
