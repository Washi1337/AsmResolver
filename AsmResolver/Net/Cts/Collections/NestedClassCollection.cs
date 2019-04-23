using System.Collections.Generic;
using AsmResolver.Net.Metadata;

namespace AsmResolver.Net.Cts.Collections
{
    public class NestedClassCollection : MemberCollection<TypeDefinition, NestedClass>
    {
        private readonly NestedClassTable _table;
        private readonly ICollection<uint> _nestedClassRids;

        public NestedClassCollection(TypeDefinition owner)
            : base(owner)
        {
        }

        internal NestedClassCollection(TypeDefinition owner, NestedClassTable table, ICollection<uint> nestedClassRids) 
            : base(owner)
        {
            _table = table;
            _nestedClassRids = nestedClassRids;
        }

        protected override void Initialize()
        {
            if (_nestedClassRids != null)
            {
                foreach (uint rid in _nestedClassRids)
                {
                    if (!_table.TryGetRow((int) (rid - 1), out var nestedClassRow))
                        continue;
                   
                    var member = (NestedClass) _table.GetMemberFromRow(Owner.Image, nestedClassRow);
                    Items.Add(member);
                    SetOwner(member, Owner);
                }
            }
            
            base.Initialize();
        }

        protected override TypeDefinition GetOwner(NestedClass item)
        {
            return item.EnclosingClass;
        }

        protected override void SetOwner(NestedClass item, TypeDefinition owner)
        {
            item.EnclosingClass = owner;
            item.Class.Module = owner?.Module;
        }
    }
}