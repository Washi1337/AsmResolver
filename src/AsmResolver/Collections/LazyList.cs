using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace AsmResolver.Collections
{
    /// <summary>
    /// Provides a base for lists that are lazy initialized.
    /// </summary>
    /// <typeparam name="TItem">The type of elements the list stores.</typeparam>
    [DebuggerDisplay("Count = {" + nameof(Count) + "}")]
    public abstract class LazyList<TItem> : IList<TItem>, IReadOnlyList<TItem>
    {
        private readonly List<TItem> _items;

        /// <summary>
        /// Creates a new, empty, uninitialized list.
        /// </summary>
        public LazyList()
        {
            _items = new List<TItem>();
        }

        /// <summary>
        /// Creates a new, empty, uninitialized list.
        /// </summary>
        /// <param name="capacity">The initial number of elements the list can store.</param>
        public LazyList(int capacity)
        {
            _items = new List<TItem>(capacity);
        }

        /// <inheritdoc cref="IList{T}.Item" />
        public TItem this[int index]
        {
            get
            {
                EnsureIsInitialized();
                return Items[index];
            }
            set
            {
                lock (_items)
                {
                    EnsureIsInitialized();
                    OnSetItem(index, value);
                }
            }
        }

        /// <inheritdoc cref="ICollection{T}.Count" />
        public virtual int Count
        {
            get
            {
                EnsureIsInitialized();
                return Items.Count;
            }
        }

        /// <summary>
        /// Gets or sets the total number of elements the list can contain before it has to resize its internal buffer.
        /// </summary>
        public int Capacity
        {
            get => _items.Capacity;
            set => _items.Capacity = value;
        }

        /// <inheritdoc />
        public bool IsReadOnly => false;

        /// <summary>
        /// Gets a value indicating the list is initialized or not.
        /// </summary>
        protected bool IsInitialized
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the underlying list.
        /// </summary>
        protected IList<TItem> Items => _items;

        /// <summary>
        /// Initializes the list. This method is called in a thread-safe manner.
        /// </summary>
        protected abstract void Initialize();

        /// <summary>
        /// Performs any final adjustments to the collection after all initial items were added to the underlying list.
        /// </summary>
        /// <remarks>
        /// Upon calling this method, the <see cref="IsInitialized"/> has already been set to <c>true</c>, but the
        /// initialization lock has not been released yet. This means that any element in the list is guaranteed
        /// to be still in its initial state. It is therefore safe to access elements, as well as adding or removing
        /// items from <see cref="Items"/>.
        /// </remarks>
        protected virtual void PostInitialize()
        {
        }

        private void EnsureIsInitialized()
        {
            if (!IsInitialized)
            {
                lock (_items)
                {
                    if (!IsInitialized)
                    {
                        Initialize();
                        IsInitialized = true;
                        PostInitialize();
                    }
                }
            }
        }

        /// <inheritdoc />
        public void Add(TItem item) => Insert(Count, item);

        /// <summary>
        /// Appends the elements of a collection to the end of the <see cref="LazyList{TItem}"/>.
        /// </summary>
        /// <param name="items">The items to append.</param>
        public void AddRange(IEnumerable<TItem> items) => InsertRange(Count, items);

        /// <inheritdoc />
        public void Clear()
        {
            lock (_items)
            {
                OnClearItems();
                IsInitialized = true;
            }
        }

        /// <inheritdoc />
        public bool Contains(TItem item)
        {
            EnsureIsInitialized();
            return Items.Contains(item);
        }

        /// <inheritdoc />
        public void CopyTo(TItem[] array, int arrayIndex)
        {
            EnsureIsInitialized();
            Items.CopyTo(array, arrayIndex);
        }

        /// <inheritdoc />
        public bool Remove(TItem item)
        {
            lock (_items)
            {
                EnsureIsInitialized();
                int index = Items.IndexOf(item);
                if (index == -1)
                    return false;
                OnRemoveItem(index);
            }

            return true;
        }

        /// <inheritdoc />
        public int IndexOf(TItem item)
        {
            EnsureIsInitialized();
            return Items.IndexOf(item);
        }

        /// <inheritdoc />
        public void Insert(int index, TItem item)
        {
            lock (_items)
            {
                EnsureIsInitialized();
                OnInsertItem(index, item);
            }
        }

        /// <summary>
        /// Inserts the elements of a collection into the <see cref="LazyList{TItem}"/> at the provided index.
        /// </summary>
        /// <param name="index">The starting index to insert the items in.</param>
        /// <param name="items">The items to insert.</param>
        private void InsertRange(int index, IEnumerable<TItem> items)
        {
            lock (_items)
            {
                EnsureIsInitialized();
                OnInsertRange(index, items);
            }
        }

        /// <inheritdoc />
        public void RemoveAt(int index)
        {
            lock (_items)
            {
                EnsureIsInitialized();
                OnRemoveItem(index);
            }
        }

        /// <summary>
        /// The method that gets called upon replacing an item in the list.
        /// </summary>
        /// <param name="index">The index that is being replaced.</param>
        /// <param name="item">The new item.</param>
        protected virtual void OnSetItem(int index, TItem item) => _items[index] = item;

        /// <summary>
        /// The method that gets called upon inserting a new item in the list.
        /// </summary>
        /// <param name="index">The index where the item is inserted at.</param>
        /// <param name="item">The new item.</param>
        protected virtual void OnInsertItem(int index, TItem item) => _items.Insert(index, item);

        /// <summary>
        /// The method that gets called upon inserting a collection of new items in the list.
        /// </summary>
        /// <param name="index">The index where the item is inserted at.</param>
        /// <param name="items">The new items.</param>
        protected virtual void OnInsertRange(int index, IEnumerable<TItem> items) => _items.InsertRange(index, items);

        /// <summary>
        /// The method that gets called upon removing an item.
        /// </summary>
        /// <param name="index">The index of the element to remove.</param>
        protected virtual void OnRemoveItem(int index) => _items.RemoveAt(index);

        /// <summary>
        /// The method that gets called upon clearing the entire list.
        /// </summary>
        protected virtual void OnClearItems() => _items.Clear();

        /// <summary>
        /// Returns an enumerator that enumerates the lazy list.
        /// </summary>
        /// <returns>The enumerator.</returns>
        /// <remarks>
        /// This enumerator only ensures the list is initialized upon calling the <see cref="Enumerator.MoveNext"/> method.
        /// </remarks>
        public Enumerator GetEnumerator() => new(this);

        /// <inheritdoc />
        IEnumerator<TItem> IEnumerable<TItem>.GetEnumerator() => GetEnumerator();

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <summary>
        /// Represents an enumerator that enumerates all items in a lazy initialized list.
        /// </summary>
        /// <remarks>
        /// The enumerator only initializes the list when it is needed. If no calls to <see cref="MoveNext"/> were
        /// made, and the lazy list was not initialized yet, it will remain uninitialized.
        /// </remarks>
        public struct Enumerator : IEnumerator<TItem?>
        {
            private readonly LazyList<TItem> _list;
            private List<TItem>.Enumerator _enumerator;
            private bool hasEnumerator;

            /// <summary>
            /// Creates a new instance of the enumerator.
            /// </summary>
            /// <param name="list">The list to enumerate.</param>
            public Enumerator(LazyList<TItem> list)
            {
                _list = list;
                _enumerator = default;
                hasEnumerator = false;
            }

            /// <inheritdoc />
            public TItem? Current => hasEnumerator ? _enumerator.Current : default;

            /// <inheritdoc />
            object? IEnumerator.Current => Current;

            /// <inheritdoc />
            public bool MoveNext()
            {
                if (!hasEnumerator)
                {
                    _list.EnsureIsInitialized();
                    _enumerator = _list._items.GetEnumerator();
                    hasEnumerator = true;
                }

                return _enumerator.MoveNext();
            }

            /// <inheritdoc />
            public void Reset() => throw new NotSupportedException();

            /// <inheritdoc />
            public void Dispose()
            {
                if (hasEnumerator)
                    _enumerator.Dispose();
            }
        }

    }
}
