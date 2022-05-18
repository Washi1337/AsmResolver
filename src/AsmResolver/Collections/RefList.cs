using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace AsmResolver.Collections
{
    /// <summary>
    /// Provides an implementation of a collection for which the raw elements can be accessed by-reference.
    /// This allows for dynamically sized lists that work on mutable structs.
    /// </summary>
    /// <typeparam name="T">The type of elements to store.</typeparam>
    /// <remarks>
    /// This list should be regarded as a mutable array that is not thread-safe.
    /// </remarks>
    public class RefList<T> : ICollection, IList<T>, IReadOnlyList<T>
        where T : struct
    {
        /// <summary>
        /// Gets the default capacity of a ref-list.
        /// </summary>
        public const int DefaultCapacity = 4;

        private T[] _items;
        private int _count;
        private int _version;

        /// <summary>
        /// Creates a new empty list with the default capacity.
        /// </summary>
        public RefList()
            : this(DefaultCapacity)
        {
        }

        /// <summary>
        /// Creates a new empty list with the provided capacity.
        /// </summary>
        /// <param name="capacity">The capacity of the list.</param>
        public RefList(int capacity)
        {
            _items = new T[capacity];
        }

        /// <inheritdoc cref="ICollection{T}.Count" />
        public int Count => _count;

        /// <summary>
        /// Gets or sets the total number of elements that the underlying array can store.
        /// </summary>
        public int Capacity
        {
            get => _items.Length;
            set
            {
                if (value == _items.Length)
                    return;

                if (value < _count)
                    throw new ArgumentException("Capacity must be equal or larger than the current number of elements in the list.");

                EnsureEnoughCapacity(value);
                IncrementVersion();
            }
        }

        /// <summary>
        /// Gets a number indicating the current version of the list.
        /// </summary>
        /// <remarks>
        /// This number is incremented each time the underlying array is resized or when an element is replaced.
        /// It can also be used to verify that the reference returned by <see cref="GetElementRef(int, out int)"/> is
        /// still referencing an element in the current array.
        /// </remarks>
        public int Version => _version;

        /// <inheritdoc />
        bool ICollection<T>.IsReadOnly => false;

        /// <inheritdoc />
        bool ICollection.IsSynchronized => false;

        /// <inheritdoc />
        object ICollection.SyncRoot => this;

        /// <summary>
        /// Gets or sets an individual element within the list.
        /// </summary>
        /// <param name="index">The index of the element to access.</param>
        /// <exception cref="IndexOutOfRangeException">
        /// Occurs when <paramref name="index"/> is not a valid index within the array.
        /// </exception>
        public T this[int index]
        {
            get
            {
                AssertIsValidIndex(index);
                return _items[index];
            }
            set
            {
                AssertIsValidIndex(index);
                _items[index] = value;
                IncrementVersion();
            }
        }

        /// <summary>
        /// Gets an element within the list by reference.
        /// </summary>
        /// <param name="index">The index of the element to access.</param>
        /// <returns>A reference to the element.</returns>
        /// <exception cref="IndexOutOfRangeException">
        /// Occurs when <paramref name="index"/> is not a valid index within the array.
        /// </exception>
        public ref T GetElementRef(int index)
        {
            AssertIsValidIndex(index);
            return ref _items[index];
        }

        /// <summary>
        /// Gets an element within the list by reference.
        /// </summary>
        /// <param name="index">The index of the element to access.</param>
        /// <param name="version">The version of the list upon obtaining the reference.</param>
        /// <returns>A reference to the element.</returns>
        /// <exception cref="IndexOutOfRangeException">
        /// Occurs when <paramref name="index"/> is not a valid index within the array.
        /// </exception>
        public ref T GetElementRef(int index, out int version)
        {
            AssertIsValidIndex(index);
            version = _version;
            return ref _items[index];
        }

        /// <summary>
        /// Adds an element to the end of the list.
        /// </summary>
        /// <param name="item">The element.</param>
        public void Add(in T item)
        {
            EnsureEnoughCapacity(_count + 1);
            _items[_count] = item;
            _count++;
            IncrementVersion();
        }

        /// <inheritdoc />
        void ICollection<T>.Add(T item) => Add(item);

        /// <inheritdoc />
        public void Clear()
        {
            Array.Clear(_items, 0, _items.Length);
            _count = 0;
            IncrementVersion();
        }

        /// <inheritdoc />
        bool ICollection<T>.Contains(T item) => IndexOf(item) >= 0;

        /// <summary>
        /// Determines whether an item is present in the reference list.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns><c>true</c> if the element is present, <c>false</c> otherwise.</returns>
        public bool Contains(in T item) => IndexOf(item) >= 0;

        /// <inheritdoc />
        public void CopyTo(T[] array, int arrayIndex) => _items.CopyTo(array, arrayIndex);

        /// <inheritdoc />
        public void CopyTo(Array array, int index) => _items.CopyTo(array, index);

        /// <summary>
        /// Removes an element from the list.
        /// </summary>
        /// <param name="item">The element to remove.</param>
        /// <returns><c>true</c> if the element was removed successfully, <c>false</c> otherwise.</returns>
        public bool Remove(in T item)
        {
            int index = IndexOf(item);
            if (index > 0)
            {
                RemoveAt(index);
                return true;
            }

            return false;
        }

        /// <inheritdoc />
        bool ICollection<T>.Remove(T item) => Remove(item);

        /// <summary>
        /// Determines the first index within the list that contains the provided element.
        /// </summary>
        /// <param name="item">The element to search.</param>
        /// <returns>The index, or -1 if the element is not present in the list.</returns>
        public int IndexOf(in T item) => Array.IndexOf(_items, item, 0, _count);

        /// <summary>
        /// Inserts an element into the list at the provided index.
        /// </summary>
        /// <param name="index">The index to insert into.</param>
        /// <param name="item">The element to insert.</param>
        public void Insert(int index, in T item)
        {
            EnsureEnoughCapacity(_count + 1);

            if (index < _count)
                Array.Copy(_items, index, _items, index + 1, _count - index);

            _items[index] = item;
            _count++;
            IncrementVersion();
        }

        /// <inheritdoc />
        void IList<T>.Insert(int index, T item) => Insert(index, item);

        /// <inheritdoc />
        public int IndexOf(T item) => Array.IndexOf(_items, item, 0, _count);

        /// <summary>
        /// Removes a single element from the list at the provided index.
        /// </summary>
        /// <param name="index">The index of the element to remove.</param>
        /// <exception cref="ArgumentOutOfRangeException">Occurs when the provided index is invalid.</exception>
        public void RemoveAt(int index)
        {
            if (index < 0 || index >= _count)
                throw new ArgumentOutOfRangeException(nameof(index));

            _count--;

            if (index < _count)
                Array.Copy(_items, index + 1, _items, index, _count - index);

            _items[_count] = default!;
            IncrementVersion();
        }

        /// <summary>
        /// Returns an enumerator that iterates all elements in the list.
        /// </summary>
        public Enumerator GetEnumerator() => new(this);

        /// <inheritdoc />
        IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumerator();

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        private void AssertIsValidIndex(int index)
        {
            if (index < 0 || index >= Count)
                throw new IndexOutOfRangeException();
        }

        private void EnsureEnoughCapacity(int requiredCount)
        {
            if (_items.Length >= requiredCount) 
                return;

            int newCapacity = _items.Length == 0 ? 1 : _items.Length * 2;
            if (newCapacity < requiredCount)
                newCapacity = requiredCount;

            Array.Resize(ref _items, newCapacity);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void IncrementVersion()
        {
            unchecked
            {
                _version++;
            }
        }

        /// <summary>
        /// Provides an implementation for an enumerator that iterates elements in a ref-list.
        /// </summary>
        public struct Enumerator : IEnumerator<T>
        {
            private readonly RefList<T> _list;
            private readonly int _version;
            private int _index;

            /// <summary>
            /// Creates a new instance of a ref-list enumerator.
            /// </summary>
            /// <param name="list">The list to enumerate.</param>
            public Enumerator(RefList<T> list)
            {
                _list = list;
                _version = list._version;
                _index = -1;
            }

            /// <inheritdoc />
            public T Current => _index >= 0 && _index < _list._count
                ? _list[_index]
                : default;

            /// <inheritdoc />
            object IEnumerator.Current => Current;

            /// <inheritdoc />
            public bool MoveNext()
            {
                if (_version != _list._version)
                    throw new InvalidOperationException("Collection was modified.");
                _index++;
                return _index >= 0 && _index < _list._count;
            }

            /// <inheritdoc />
            public void Reset() => throw new NotSupportedException();

            /// <inheritdoc />
            void IDisposable.Dispose()
            {
            }
        }
    }
}
