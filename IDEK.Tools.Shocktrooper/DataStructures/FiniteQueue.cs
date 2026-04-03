using System.Collections;
using System.Collections.Generic;

namespace IDEK.Tools.DataStructures
{
    ///<summary>
    /// A queue with a hard finite size. Attempting to push an element when the queue is at capacity will immediately do a Pop() afterwards to retain the size.
    /// </summary> 
    public class FiniteQueue<T> : Queue<T>
    {
        private int _maxSize;

        public FiniteQueue(int maxSize)
        {
            this._maxSize = maxSize;
        }

        public void Resize(int newSize)
        {
            _maxSize = newSize;
        }
        
        public new void Enqueue(T item)
        {
            base.Enqueue(item);
            while(this.Count > this._maxSize) this.Dequeue();
        }
    }
}