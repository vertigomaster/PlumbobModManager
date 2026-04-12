using System;
using System.Collections.Generic;
using IDEK.Tools.Logging;

namespace IDEK.Tools.Utilities
{
    /// <inheritdoc cref="IdekEvent{T}"/>
    public class IdekEvent : IdekEvent<Action> { }
    
    /// <summary>
    /// A safe and simple event wrapper that avoids double adds and does not throw exceptions on double removes,
    /// instead returning a boolean to indicate whether they succeeded or not.
    /// </summary>
    /// <remarks>
    /// This is to be used IN PLACE OF a native C#, much like Unity's UnityEvent class but without needing Unity.
    /// <br/>
    /// If you are wanting to place similar wrapping over an existing native C# event,
    /// see <see cref="NativeEventGuard{T}"/> instead!
    /// </remarks>
    /// <typeparam name="T"></typeparam>
    public class IdekEvent<T> : IDisposable where T : Delegate
    {
        private readonly HashSet<T> _registered = new();
        private T? _delegate;

        public bool TryAddListener(T handler)
        {
            if (!_registered.Add(handler)) return false;
            // ConsoleLog.Log("adding handler to IdekEvent");
            _delegate = (T)Delegate.Combine(_delegate, handler);
            return true;
        }

        public bool TryRemoveListener(T handler)
        {
            if (!_registered.Remove(handler)) return false;
            _delegate = (T?)Delegate.Remove(_delegate, handler);
            return true;
        }

        public void Invoke(params object?[] args)
        {
            // ConsoleLog.Log("invoking our IdekEvent");
            _delegate?.DynamicInvoke(args);
        }

        public void RemoveAllListeners()
        {
            _registered.Clear();
            _delegate = null;
        }

        #region IDisposable

        /// <inheritdoc />
        public void Dispose()
        {
            // TODO release managed resources here
            RemoveAllListeners();
            GC.SuppressFinalize(this); //compiler told me to
        }

        #endregion
    }
}