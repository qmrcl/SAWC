using System.Collections.Generic;

namespace SAWC.Modifiers
{
    public readonly struct ReadOnlyListWrapper<T>
    {
        private readonly List<T> _list;

        public ReadOnlyListWrapper(List<T> list) => _list = list;

        public int Count => _list?.Count ?? 0;

        public T this[int index] => _list[index];

        public List<T>.Enumerator GetEnumerator() => _list.GetEnumerator();
    }

    public sealed class PrioritizedList<T> where T : IPrioritized
    {
        private readonly List<T> _items = new();

        public ReadOnlyListWrapper<T> Items => new ReadOnlyListWrapper<T>(_items);

        public void Add(T item)
        {
            if (_items.Contains(item)) return;

            int index = _items.BinarySearch(item, PriorityComparer.Instance);

            if (index < 0)
                index = ~index;

            _items.Insert(index, item);
        }

        public void Remove(T item)
        {
            _items.Remove(item);
        }

        private sealed class PriorityComparer : IComparer<T>
        {
            public static readonly PriorityComparer Instance = new();
            public int Compare(T x, T y) => x.Priority.CompareTo(y.Priority);
        }
    }
}