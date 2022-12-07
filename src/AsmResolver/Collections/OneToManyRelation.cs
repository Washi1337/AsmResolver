using System;
using System.Collections;
using System.Collections.Generic;

namespace AsmResolver.Collections
{
    /// <summary>
    /// Represents a one-to-many relation, where an object is mapped to a collection of other objects.
    /// </summary>
    /// <typeparam name="TKey">The type of objects to map.</typeparam>
    /// <typeparam name="TValue">The type of objects to map to.</typeparam>
    public sealed class OneToManyRelation<TKey, TValue>
        where TKey : notnull
        where TValue : notnull
    {
        private readonly Dictionary<TKey, ValueSet> _memberLists;
        private readonly Dictionary<TValue, TKey> _memberOwners;

        /// <summary>
        /// Creates a new, empty one-to-many relation mapping.
        /// </summary>
        public OneToManyRelation()
        {
            _memberLists = new Dictionary<TKey, ValueSet>();
            _memberOwners = new Dictionary<TValue, TKey>();
        }

        /// <summary>
        /// Creates a new, empty one-to-many relation mapping.
        /// </summary>
        /// <param name="capacity">The initial number of elements the relation can store.</param>
        public OneToManyRelation(int capacity)
        {
            _memberLists = new Dictionary<TKey, ValueSet>(capacity);
            _memberOwners = new Dictionary<TValue, TKey>(capacity);
        }

        /// <summary>
        /// Registers a relation between two objects.
        /// </summary>
        /// <param name="key">The first object.</param>
        /// <param name="value">The second object to map to.</param>
        /// <returns><c>true</c> if the key value pair was successfully registered. <c>false</c> if there already exists
        /// a key that maps to the provided value.</returns>
        public bool Add(TKey key, TValue value)
        {
            if (!_memberOwners.ContainsKey(value))
            {
                if (!_memberLists.TryGetValue(key, out var valueSet))
                {
                    valueSet = new ValueSet();
                    _memberLists.Add(key, valueSet);
                }

                valueSet.Items.Add(value);
                _memberOwners.Add(value, key);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Gets a collection of values the provided key maps to.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>The values.</returns>
        public ValueSet GetValues(TKey key) => _memberLists.TryGetValue(key, out var valueSet)
            ? valueSet
            : ValueSet.Empty;

        /// <summary>
        /// Gets the key that maps to the provided value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>The key.</returns>
        public TKey? GetKey(TValue value) => _memberOwners.TryGetValue(value, out var key)
            ? key
            : default;

        /// <summary>
        /// Represents a collection of values assigned to a single key in a one-to-many relation.
        /// </summary>
        public class ValueSet : ICollection<TValue>
        {
            /// <summary>
            /// Represents the empty value set.
            /// </summary>
            public static readonly ValueSet Empty = new();

            internal List<TValue> Items
            {
                get;
            } = new();

            /// <inheritdoc />
            public int Count => Items.Count;

            /// <inheritdoc />
            public bool IsReadOnly => true;

            /// <inheritdoc />
            public void Add(TValue item) => throw new NotSupportedException();

            /// <inheritdoc />
            public void Clear() => throw new NotSupportedException();

            /// <inheritdoc />
            public bool Contains(TValue item) => Items.Contains(item);

            /// <inheritdoc />
            public void CopyTo(TValue[] array, int arrayIndex) => Items.CopyTo(array, arrayIndex);

            /// <inheritdoc />
            public bool Remove(TValue item) => throw new NotSupportedException();

            /// <summary>
            /// Gets an enumerator that enumerates all values in the collection.
            /// </summary>
            public Enumerator GetEnumerator() => new(Items.GetEnumerator());

            /// <inheritdoc />
            IEnumerator<TValue> IEnumerable<TValue>.GetEnumerator() => GetEnumerator();

            /// <inheritdoc />
            IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable) Items).GetEnumerator();

            /// <summary>
            /// Represents an enumerator that enumerates all items in a value collection.
            /// </summary>
            public struct Enumerator : IEnumerator<TValue>
            {
                private List<TValue>.Enumerator _enumerator;

                internal Enumerator(List<TValue>.Enumerator enumerator)
                {
                    _enumerator = enumerator;
                }

                /// <inheritdoc />
                public TValue Current => _enumerator.Current!;

                /// <inheritdoc />
                object IEnumerator.Current => ((IEnumerator) _enumerator).Current!;

                /// <inheritdoc />
                public bool MoveNext() => _enumerator.MoveNext();

                /// <inheritdoc />
                public void Reset() => throw new NotSupportedException();

                /// <inheritdoc />
                public void Dispose() => _enumerator.Dispose();
            }
        }
    }
}
