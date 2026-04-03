using System;

namespace IDEK.Tools.ShocktroopUtils
{
    /// <summary>
    /// Interface for checking if a class has been changed (dirtied), enabling you to act upon it being updated
    /// </summary>
    public interface ICleanable
    {
        /// <summary>
        /// Fires whenever a change occurs, even if this instance already dirty.
        /// If you only want to check for changes that freshly mark this instance as dirty,
        /// listen to <see cref="DirtiedEvent"/> instead.
        /// </summary>
        public Action ChangedEvent { get; set; }

        /// <summary>
        /// Note: This only fires once the dirty flag is flipped. If this instance is already dirty,
        /// this will not fire.
        /// If you want to check for every change, listen to <see cref="ChangedEvent"/> instead.
        /// </summary>
        public Action DirtiedEvent { get; set; }
        public bool IsDirty { get; }
        public void Clean();

        /// <summary>
        /// Sets whether to allow the change callbacks to fire. Temporarily silencing this and then manually invoking
        /// <see cref="MarkDirtyIf"/> is a great way to "throttle" rapid/frequent changes.
        /// </summary>
        /// <remarks>
        /// Example: say you have an IEnumerable of some kind with 50 crabs in it,
        /// and you want to add the blue crabs from it into a list-like object that implements <see cref="ICleanable"/>.
        /// Depending on how you go about it, that singular operation could invoke the <see cref="ChangedEvent"/>
        /// repeatedly, once for each blue grab. 20 blue crabs would lead to 20 extra invocations.
        /// If you disable this flag first, add your grabs, and manually dirty it afterward though,
        /// you cut those 20 calls down to 1. 
        /// </remarks>
        public bool ChangeCallbacksEnabled { get; }

        /// <summary>
        /// Attempts to mark the thing dirty. Generally returns false if it is already dirty.
        /// </summary>
        /// <returns>True if the <see cref="IsDirty"/> flag was changed from false -> true.
        /// <br/>False if the <see cref="IsDirty"/> flag was already set to true.</returns>
        bool TryMarkDirty();

        /// <summary>
        /// Marks as dirty, generally resulting in the callbacks firing.
        /// </summary>
        void MarkDirty();

        /// <summary>
        /// Marks the HashSet as dirty if the condition is true. Returns said condition for cleaner syntax/compactness.
        /// </summary>
        /// <param name="condition"></param>
        /// <returns></returns>
        bool MarkDirtyIf(bool condition);
    }

    /// <inheritdoc cref="ICleanable"/>
    /// <remarks>this subclass enables the event callbacks to supply a <see cref="T"/>.
    /// <br/>
    /// The type parameter could be an element of the ICollection that is also implementing this interface,
    /// Or you could set T to something else that can contain/describe the operation
    /// (useful for command histories and other complex cases). 
    /// </remarks>
    public interface ICleanable<T> : ICleanable
    {
        public new Action<T> ChangedEvent { get; set; }
        public new Action<T> DirtiedEvent { get; set; }
        // public bool IsDirty { get; }
        // public void Clean();
    }
}
