using System.Collections.Generic;

namespace AsmResolver.DotNet.Collections
{
    internal class OneToOneRelation<TKey, TValue>
    {
        private readonly IDictionary<TKey, TValue> _keyToValue = new Dictionary<TKey, TValue>();
        private readonly IDictionary<TValue, TKey> _valueToKey = new Dictionary<TValue, TKey>();

        public bool Add(TKey key, TValue value)
        {
            if (!_keyToValue.ContainsKey(key))
            {
                _keyToValue.Add(key, value);
                _valueToKey.Add(value, key);
                return true;
            }

            return false;
        }

        public TValue GetValue(TKey key)
        {
            _keyToValue.TryGetValue(key, out var value);
            return value;
        }

        public TKey GetKey(TValue value)
        {
            _valueToKey.TryGetValue(value, out var key);
            return key;
        }
    }
}