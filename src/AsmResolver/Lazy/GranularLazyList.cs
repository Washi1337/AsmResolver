using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace AsmResolver.Lazy
{
    /// <summary>
    /// Provides a base implementation of a lazy list that delays the initialization of each element in the collection
    /// individually until it is accessed.
    /// </summary>
    /// <typeparam name="TItem">The type of elements the list contains.</typeparam>
    public abstract class GranularLazyList<TItem> : IList<TItem>
    {
        private readonly object _lockObject = new object();
        private readonly BitArray _initializationVector;
        private int _uninitializedItems;
        
        protected GranularLazyList(int initialCount)
        {
            Items = new TItem[initialCount].ToList();
            _initializationVector = new BitArray(initialCount);
            _uninitializedItems = initialCount;
        }
        
        protected IList<TItem> Items
        {
            get;
        }

        /// <inheritdoc />
        public int Count => Items.Count;

        /// <inheritdoc />
        public bool IsReadOnly => Items.IsReadOnly;

        /// <inheritdoc />
        public TItem this[int index]
        {
            get
            {
                if (!_initializationVector[index])
                    InitializeItem(index);
                
                return Items[index];
            }
            set
            {
                lock (_lockObject)
                {
                    Items[index] = value;
                    if (_uninitializedItems > 0)
                    {
                        _initializationVector[index] = true;
                        _uninitializedItems--;
                    }
                }
            }
        }

        /// <inheritdoc />
        public void Add(TItem item)
        {
            lock (_lockObject)
            {
                EnsureIsInitializedEntirely();
                Items.Add(item);
            }
        }

        /// <inheritdoc />
        public void Clear()
        {
            lock (_lockObject)
            {
                Items.Clear();
                _uninitializedItems = 0;
            }
        }

        /// <inheritdoc />
        public bool Contains(TItem item)
        {
            return IndexOf(item) >= 0;
        }

        /// <inheritdoc />
        public void CopyTo(TItem[] array, int arrayIndex)
        {
            lock (_lockObject)
            {
                EnsureIsInitializedEntirely();
                Items.CopyTo(array, arrayIndex);
            }
        }

        /// <inheritdoc />
        public bool Remove(TItem item)
        {
            lock (_lockObject)
            {
                EnsureIsInitializedEntirely();
                return Items.Remove(item);
            }
        }

        /// <inheritdoc />
        public int IndexOf(TItem item)
        {
            lock (_lockObject)
            {
                for (int i = 0; i < Count; i++)
                {
                    var x = this[i];
                    if (Equals(x, item))
                        return i;
                }
            }

            return -1;
        }

        /// <inheritdoc />
        public void Insert(int index, TItem item)
        {
            lock (_lockObject)
            {
                EnsureIsInitializedEntirely();
                Items.Insert(index, item);
            }
        }

        /// <inheritdoc />
        public void RemoveAt(int index)
        {
            lock (_lockObject)
            {
                EnsureIsInitializedEntirely();
                Items.RemoveAt(index);
            }
        }
        
        /// <inheritdoc />
        public IEnumerator<TItem> GetEnumerator()
        {
            return Items.GetEnumerator();
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable) Items).GetEnumerator();
        }

        private void EnsureIsInitializedEntirely()
        {
            if (_uninitializedItems > 0)
                InitializeEntireList();
        }

        private void InitializeEntireList()
        {
            lock (_lockObject)
            {
                if (_uninitializedItems > 0)
                {
                    for (int i = 0; i < Count; i++)
                        Items[i] = GetItem(i);
                    _uninitializedItems = 0;
                    _initializationVector.SetAll(true);
                }
            }
        }

        private void InitializeItem(int index)
        {
            lock (_lockObject)
            {
                if (!_initializationVector[index])
                {
                    Items[index] = GetItem(index);
                    _initializationVector[index] = true;
                    _uninitializedItems--;
                }
            }
        }

        /// <summary>
        /// Obtains the item at the provided index. 
        /// </summary>
        /// <param name="index">The index of the element.</param>
        /// <returns>The item.</returns>
        /// <remarks>
        /// This method is called for each element initialization in the list.
        /// </remarks>
        protected abstract TItem GetItem(int index);

    }
}