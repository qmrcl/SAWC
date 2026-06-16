using System.Collections;
using System.Collections.Generic;

namespace SAWC.Modifiers
{
    public sealed class PrioritizedList<T> : IReadOnlyList<T> where T : IPrioritized
    {
        private readonly List<T> _items = new();

        public int Count => _items.Count;
        public T this[int index] => _items[index];

        public void Add(T item)
        {
            if (_items.Contains(item)) return;

            int insertIndex = _items.Count;
            for (int i = 0; i < _items.Count; i++)
            {
                if (_items[i].Priority > item.Priority)
                {
                    insertIndex = i;
                    break;
                }
            }

            _items.Insert(insertIndex, item);
        }

        public bool Remove(T item)
        {
            return _items.Remove(item);
        }

        public void Clear()
        {
            _items.Clear();
        }

        public void UpdatePriority(T item)
        {
            if (_items.Remove(item))
            {
                Add(item);
            }
        }

        public IEnumerator<T> GetEnumerator() => _items.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}