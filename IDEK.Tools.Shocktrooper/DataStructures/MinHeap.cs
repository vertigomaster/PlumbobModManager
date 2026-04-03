using System;
using System.Collections.Generic;

//TODO: revise namespace for better consistency with the rest of the library.
namespace IDEK.Tools.ShocktrooperUtils.DataStructures
{
    /// <summary>
    /// A small binary min-heap implementation that keys items using a provided selector returning a floating-point key.
    /// </summary>
    public class MinHeap<T>
    {
        private readonly List<T> data;
        private readonly Func<T, double> keySelector;

        public MinHeap(Func<T, double> keySelector)
        {
            this.keySelector = keySelector ?? throw new ArgumentNullException(nameof(keySelector));
            this.data = new List<T>();
        }

        public int Count => data.Count;

        public void Clear() => data.Clear();

        public void Push(T item)
        {
            data.Add(item);
            SiftUp(data.Count - 1);
        }

        public bool TryPop(out T item)
        {
            if (data.Count == 0) { item = default; return false; }
            item = data[0];
            int last = data.Count - 1;
            if (last > 0)
            {
                data[0] = data[last];
            }
            data.RemoveAt(last);
            if (data.Count > 0) SiftDown(0);
            return true;
        }

        private void SiftUp(int i)
        {
            while (i > 0)
            {
                int p = (i - 1) >> 1;
                if (keySelector(data[p]) <= keySelector(data[i])) break;
                Swap(p, i);
                i = p;
            }
        }

        private void SiftDown(int i)
        {
            int n = data.Count;
            while (true)
            {
                int l = i * 2 + 1;
                int r = l + 1;
                int smallest = i;
                if (l < n && keySelector(data[l]) < keySelector(data[smallest])) smallest = l;
                if (r < n && keySelector(data[r]) < keySelector(data[smallest])) smallest = r;
                if (smallest == i) break;
                Swap(i, smallest);
                i = smallest;
            }
        }

        private void Swap(int a, int b)
        {
            var tmp = data[a]; data[a] = data[b]; data[b] = tmp;
        }
    }
}
