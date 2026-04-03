using System;
using System.Collections.Generic;

namespace IDEK.Tools.Utilities
{
    /// <summary>
    /// A safety wrapper for external, pure/raw C# Events to ease the trouble of manually tracking listeners.
    /// </summary>
    public sealed class NativeEventGuard<T> where T : Delegate
    {
        private readonly Action<T> _add;
        private readonly Action<T> _remove;
        private HashSet<T> _registeredCallbacks = new();
        
        /// <summary>
        /// Due to the frustrating limitations of C# events, you must explicitly specify what the add and remove
        /// operations for that event look like.
        /// <br/>
        /// That's usually just: <c>var example = new EventGuard(h => yourEvent += h, h => yourEvent -= h);</c>
        /// </summary>
        /// <param name="add"></param>
        /// <param name="remove"></param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <remarks>
        /// 
        /// </remarks>
        public NativeEventGuard(Action<T> add, Action<T> remove)
        {
            _add = add ?? throw new ArgumentNullException(nameof(add));
            _remove = remove ?? throw new ArgumentNullException(nameof(remove));
        }

        public bool TryAddListener(T callback)
        {
            lock (_registeredCallbacks)
            {
                if (!_registeredCallbacks.Add(callback)) return false;
            }

            _add(callback);
            return true;
        }

        public bool TryRemoveListener(T callback)
        {
            lock (_registeredCallbacks)
            {
                if (!_registeredCallbacks.Remove(callback)) return false;
            }

            _remove(callback);
            return true;
        }

        public void RemoveAllListeners()
        {
            foreach (var c in _registeredCallbacks)
            {
                _remove(c);
            }

            _registeredCallbacks.Clear();
        }

    }
}