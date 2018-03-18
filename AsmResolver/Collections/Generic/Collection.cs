using System.Collections;
using System.Collections.Generic;

namespace AsmResolver.Collections.Generic
{
    public class Collection<T> : IList<T>
    {
        public Collection()
        {
            Items = new List<T>();
            IsInitialized = false;
        }
        protected List<T> Items
        {
            get;
            set;
        }

        protected bool IsInitialized
        {
            get;
            set;
        }

        public T this[int index]
        {
            get
            {
                EnsureIsInitialized();
                return Items[index];
            }
            set
            {
                EnsureIsInitialized();
                SetItem(index, value);
            }
        }

        public virtual int Count
        {
            get
            {
                EnsureIsInitialized(); 
                return Items.Count;
            }
        }

        public virtual bool IsReadOnly
        {
            get { return false; }
        }
        
        protected virtual void Initialize()
        {
        }

        protected void EnsureIsInitialized()
        {
            if(!IsInitialized)
                Initialize();
            IsInitialized = true;
        }

        public void Add(T item)
        {
            Insert(Count, item);
        }

        public void Clear()
        {
            EnsureIsInitialized();
            ClearItems();
        }

        public bool Contains(T item)
        {
            EnsureIsInitialized();
            return Items.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            EnsureIsInitialized();
            Items.CopyTo(array, arrayIndex);
        }

        public bool Remove(T item)
        {
            var index = IndexOf(item);
            if (index == -1)
                return false;
            RemoveAt(index);
            return true;
        }

        public int IndexOf(T item)
        {
            EnsureIsInitialized();
            return Items.IndexOf(item);
        }

        public void Insert(int index, T item)
        {
            EnsureIsInitialized();
            InsertItem(index, item);
        }

        public void RemoveAt(int index)
        {
            EnsureIsInitialized();
            RemoveItem(index);
        }

        public IEnumerator<T> GetEnumerator()
        {
            EnsureIsInitialized();
            return Items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        protected virtual void ClearItems()
        {
            Items.Clear();
        }

        protected virtual void InsertItem(int index, T item)
        {
            Items.Insert(index, item);
        }

        protected virtual void SetItem(int index, T item)
        {
            Items[index] = item;
        }

        protected virtual void RemoveItem(int index)
        {
            Items.RemoveAt(index);
        }
    }
}
