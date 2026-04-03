using System;

namespace IDEK.Tools.ShocktroopUtils
{
    /// <summary>
    /// A simple composable implementation of ICleanable. 
    /// </summary>
    /// <remarks>
    /// This class is intended to be composed, not inherited from.
    /// You are encouraged to have your other class implement ICleanable through a private instance of this class here.
    /// <br/>
    /// This also improves accessibility since you don't have to manually reimplement this whole class every time.
    /// </remarks>
    public sealed class StandardCleanableImpl : ICleanable, ICloneable
    {
        #region Implementation of ICleanable

        /// <inheritdoc />
        public Action ChangedEvent { get; set; } = null;

        /// <inheritdoc />
        public Action DirtiedEvent { get; set; } = null;

        /// <inheritdoc />
        public bool IsDirty { get; protected set; } = false;

        /// <inheritdoc />
        public void Clean() => IsDirty = false;

        /// <inheritdoc />
        public bool ChangeCallbacksEnabled { get; set; } = true;

        /// <inheritdoc />
        public bool TryMarkDirty()
        {
            if (IsDirty) return false; //already dirty
            MarkDirty();
            return true;
        }

        /// <inheritdoc />
        public void MarkDirty()
        {
            if (!ChangeCallbacksEnabled) return;
            bool wasAlreadyDirty = IsDirty;
            IsDirty = true;
            if (!wasAlreadyDirty) DirtiedEvent?.Invoke();
            ChangedEvent?.Invoke();
        }

        /// <inheritdoc />
        public bool MarkDirtyIf(bool condition)
        {
            if (condition) MarkDirty();
            return condition;
        }

        #endregion

        #region Implementation of ICloneable

        /// <inheritdoc />
        public object Clone()
        {
            StandardCleanableImpl clone = MemberwiseClone() as StandardCleanableImpl ?? new();

            //doing the Actions explicitly, just in case.
            //I admittedly don't understand MemberwiseClone well enough
            //to trust it with this.
            clone.ChangedEvent = ChangedEvent;
            clone.DirtiedEvent = DirtiedEvent;
            
            return clone;
        }

        #endregion
    }
}