using System.Collections.Generic;

namespace SAWC.Pipeline
{
    public sealed class PrioritizedList<T> where T : IPrioritized
    {
        private readonly List<T> _items = new();

        public IReadOnlyList<T> Items => _items;

        public void Add(T item)
        {
            if (!_items.Contains(item))
            {
                _items.Add(item);
                _items.Sort((a, b) => a.Priority.CompareTo(b.Priority));
            }
        }

        public void Remove(T item)
        {
            _items.Remove(item);
        }
    }
}