using System.Collections.Generic;

namespace AsmResolver.Collections
{
    public class OneToManyRelation<TKey, TValue>
    {
        private readonly IDictionary<TKey, ICollection<TValue>> _memberLists = new Dictionary<TKey, ICollection<TValue>>();
        private readonly IDictionary<TValue, TKey> _memberOwners = new Dictionary<TValue, TKey>();

        public bool Add(TKey key, TValue value)
        {
            if (!_memberOwners.ContainsKey(value))
            {
                GetMemberList(key).Add(value);
                _memberOwners.Add(value, key);
                return true;
            }

            return false;
        }
        
        public ICollection<TValue> GetMemberList(TKey key)
        {
            if (!_memberLists.TryGetValue(key, out var items))
            {
                items = new List<TValue>();
                _memberLists.Add(key, items);
            }
            
            return items;
        }

        public TKey GetMemberOwner(TValue value)
        {
            return _memberOwners.TryGetValue(value, out var key)
                ? key
                : default;
        }
    }
}