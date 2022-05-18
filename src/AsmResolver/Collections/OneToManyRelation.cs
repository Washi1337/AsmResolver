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
        private readonly Dictionary<TKey, ICollection<TValue>> _memberLists;
        private readonly Dictionary<TValue, TKey> _memberOwners;

        /// <summary>
        /// Creates a new, empty one-to-many relation mapping.
        /// </summary>
        public OneToManyRelation()
        {
            _memberLists = new Dictionary<TKey, ICollection<TValue>>();
            _memberOwners = new Dictionary<TValue, TKey>();
        }

        /// <summary>
        /// Creates a new, empty one-to-many relation mapping.
        /// </summary>
        /// <param name="capacity">The initial number of elements the relation can store.</param>
        public OneToManyRelation(int capacity)
        {
            _memberLists = new Dictionary<TKey, ICollection<TValue>>(capacity);
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
                GetValues(key).Add(value);
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
        public ICollection<TValue> GetValues(TKey key)
        {
            if (!_memberLists.TryGetValue(key, out var items))
            {
                items = new List<TValue>();
                _memberLists.Add(key, items);
            }

            return items;
        }

        /// <summary>
        /// Gets the key that maps to the provided value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>The key.</returns>
        public TKey? GetKey(TValue value)
        {
            return _memberOwners.TryGetValue(value, out var key)
                ? key
                : default;
        }
    }
}
