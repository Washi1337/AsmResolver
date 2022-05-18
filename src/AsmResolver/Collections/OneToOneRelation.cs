using System.Collections.Generic;

namespace AsmResolver.Collections
{
    /// <summary>
    /// Describes a one-to-one relation between two object types, with an efficient lookup time for both keys and values.
    /// </summary>
    /// <typeparam name="TKey">The first object type.</typeparam>
    /// <typeparam name="TValue">The second object type.</typeparam>
    public sealed class OneToOneRelation<TKey, TValue>
        where TKey : notnull
        where TValue : notnull
    {
        private readonly Dictionary<TKey, TValue> _keyToValue;
        private readonly Dictionary<TValue, TKey> _valueToKey;

        /// <summary>
        /// Creates a new, empty one-to-one mapping.
        /// </summary>
        public OneToOneRelation()
        {
            _keyToValue = new Dictionary<TKey, TValue>();
            _valueToKey = new Dictionary<TValue, TKey>();
        }

        /// <summary>
        /// Creates a new, empty one-to-one mapping.
        /// </summary>
        /// <param name="capacity">The initial number of elements the relation can store.</param>
        public OneToOneRelation(int capacity)
        {
            _keyToValue = new Dictionary<TKey, TValue>(capacity);
            _valueToKey = new Dictionary<TValue, TKey>(capacity);
        }

        /// <summary>
        /// Registers a one-to-one relation between two objects.
        /// </summary>
        /// <param name="key">The first object.</param>
        /// <param name="value">The second object to map to.</param>
        /// <returns><c>true</c> if the key value pair was successfully registered. <c>false</c> if the key or
        /// value already existed.</returns>
        public bool Add(TKey key, TValue value)
        {
            if (!_keyToValue.ContainsKey(key) && !_valueToKey.ContainsKey(value))
            {
                _keyToValue.Add(key, value);
                _valueToKey.Add(value, key);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Gets the value that was assigned to the provided key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>The value.</returns>
        public TValue? GetValue(TKey key)
        {
            _keyToValue.TryGetValue(key, out var value);
            return value;
        }

        /// <summary>
        /// Gets the key to which the provided value was assigned.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>The key.</returns>
        public TKey? GetKey(TValue value)
        {
            _valueToKey.TryGetValue(value, out var key);
            return key;
        }

        /// <summary>
        /// Gets all keys stored in the one-to-one mapping.
        /// </summary>
        /// <returns>The keys.</returns>
        public IEnumerable<TKey> GetKeys() => _keyToValue.Keys;

        /// <summary>
        /// Gets all values stored in the one-to-one mapping.
        /// </summary>
        /// <returns>The values.</returns>
        public IEnumerable<TValue> GetValues() => _valueToKey.Keys;
    }
}
