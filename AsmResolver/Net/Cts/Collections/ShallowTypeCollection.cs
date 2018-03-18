using System;
using AsmResolver.Net.Metadata;

namespace AsmResolver.Net.Cts.Collections
{
    public class ShallowTypeCollection : MemberCollection<ModuleDefinition, TypeDefinition>
    {
        private readonly TypeDefinitionTable _table;
        
        public ShallowTypeCollection(ModuleDefinition owner, TypeDefinitionTable table) 
            : base(owner)
        {
            if (table == null) 
                throw new ArgumentNullException("table");
            _table = table;
        }

        protected override void Initialize()
        {
            base.Initialize();
            foreach (var row in _table)
            {
                var member = (TypeDefinition) _table.GetMemberFromRow(Owner.Image, row);
                if (member.DeclaringType == null)
                {
                    SetOwner(member, Owner);
                    Items.Add(member);
                }
            }
        }

        protected override ModuleDefinition GetOwner(TypeDefinition item)
        {
            return item.Module;
        }

        protected override void SetOwner(TypeDefinition item, ModuleDefinition owner)
        {
            item.Module = owner;
        }
    }
}