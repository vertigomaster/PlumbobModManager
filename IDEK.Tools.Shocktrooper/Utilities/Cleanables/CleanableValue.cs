using System;

namespace IDEK.Tools.ShocktroopUtils.Cleanables
{
    /// <summary>
    /// An ICleanable wrapper intended for basic value types.
    /// Sets itself to dirty if the value is set to something that would fail a static Equals() check.
    /// While it will technically work for reference types and more complex objects,
    /// you'll likely want to write your own implementation to handle them better/in the way you need. 
    /// </summary>
    /// <typeparam name="T">The type being wrapped. Ideally a simple value type.</typeparam>
    public class CleanableValue<T> : ICleanable<T>
    {
        private T _value;
        private readonly StandardCleanableImpl _cleaner = new();
        private Action<T> _dirtiedEvent;
        private Action<T> _changedEvent;
        private bool _lastSetWasDiff = false;

        public T Value
        {
            get => _value;
            set
            {
                _lastSetWasDiff = Equals(_value, value);
                _value = value;
                MarkDirtyIf(_lastSetWasDiff);
            }
        }

        public CleanableValue()
        {
            _cleaner.ChangedEvent += () => _changedEvent?.Invoke(_value);
            _cleaner.DirtiedEvent += () => _dirtiedEvent?.Invoke(_value);
        }
        
        public static implicit operator T(CleanableValue<T> self) => self.Value;

        #region Implementation of ICleanable

        /// <inheritdoc />
        public Action<T> ChangedEvent
        {
            get => _changedEvent;
            set => _changedEvent = value;
        }

        /// <inheritdoc />
        public Action<T> DirtiedEvent
        {
            get => _dirtiedEvent;
            set => _dirtiedEvent = value;
        }

        /// <inheritdoc />
        Action ICleanable.DirtiedEvent
        {
            get => _cleaner.DirtiedEvent;
            set => _cleaner.DirtiedEvent = value;
        }


        /// <inheritdoc />
        Action ICleanable.ChangedEvent
        {
            get => _cleaner.ChangedEvent;
            set => _cleaner.ChangedEvent = value;
        }

        /// <inheritdoc />
        public bool IsDirty => _cleaner.IsDirty;

        /// <inheritdoc />
        public void Clean() => _cleaner.Clean();

        /// <inheritdoc />
        public bool ChangeCallbacksEnabled => _cleaner.ChangeCallbacksEnabled;

        /// <inheritdoc />
        public bool TryMarkDirty() => _cleaner.TryMarkDirty();

        /// <inheritdoc />
        public void MarkDirty() => _cleaner.MarkDirty();

        /// <inheritdoc />
        public bool MarkDirtyIf(bool condition) => _cleaner.MarkDirtyIf(condition);

        #endregion
    }
}
