// AsmResolver - Executable file format inspection library 
// Copyright (C) 2016-2019 Washi
// 
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 3.0 of the License, or (at your option) any later version.
// 
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public
// License along with this library; if not, write to the Free Software
// Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301  USA

using System.Collections;
using System.Collections.Generic;

namespace AsmResolver.Collections
{
    /// <summary>
    /// Provides a base for lists that are lazy initialized.
    /// </summary>
    /// <typeparam name="TItem">The type of elements the list stores.</typeparam>
    public abstract class LazyList<TItem> : IList<TItem>
    {
        private bool _initialized;

        /// <inheritdoc />
        public TItem this[int index]
        {
            get
            {
                EnsureIsInitialized();
                return Items[index];
            }
            set
            {
                EnsureIsInitialized();
                Items[index] = value;
            }
        }

        /// <inheritdoc />
        public int Count
        {
            get
            {
                EnsureIsInitialized();
                return Items.Count;
            }
        }

        /// <inheritdoc />
        public bool IsReadOnly => false;

        /// <summary>
        /// Gets the underlying list.
        /// </summary>
        public IList<TItem> Items
        {
            get;
        } = new List<TItem>();

        /// <summary>
        /// Initializes the list
        /// </summary>
        protected abstract void Initialize();

        private void EnsureIsInitialized()
        {
            if (!_initialized)
            {
                lock (this)
                {
                    if (!_initialized)
                    {
                        Initialize();
                        _initialized = true;
                    }
                }
            }
        }

        /// <inheritdoc />
        public void Add(TItem item)
        {
            EnsureIsInitialized();
            Items.Add(item);
        }

        /// <inheritdoc />
        public void Clear()
        {
            Items.Clear();
            _initialized = true;
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
            EnsureIsInitialized();
            return Items.Remove(item);
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
            EnsureIsInitialized();
            Items.Insert(index, item);
        }

        /// <inheritdoc />
        public void RemoveAt(int index)
        {
            EnsureIsInitialized();
            Items.RemoveAt(index);
        }

        /// <inheritdoc />
        public IEnumerator<TItem> GetEnumerator()
        {
            EnsureIsInitialized();
            return Items.GetEnumerator();
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        
    }
}