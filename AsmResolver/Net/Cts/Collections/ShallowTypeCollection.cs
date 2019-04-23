using System;
using System.Collections.Generic;
using AsmResolver.Net.Metadata;

namespace AsmResolver.Net.Cts.Collections
{
    public class ShallowTypeCollection : MemberCollection<ModuleDefinition, TypeDefinition>
    {
        private readonly TypeDefinitionTable _typeTable;
        private readonly ICollection<uint> _topLevelTypeTokens;

        internal ShallowTypeCollection(ModuleDefinition owner, TypeDefinitionTable table, ICollection<uint> topLevelTypeTokens) 
            : base(owner)
        {
            _typeTable = table ?? throw new ArgumentNullException(nameof(table));
            _topLevelTypeTokens = topLevelTypeTokens;
        }

        protected override void Initialize()
        {
            base.Initialize();
            foreach (var typeRid in _topLevelTypeTokens)
            {
                if (!_typeTable.TryGetRow((int) (typeRid - 1), out var typeRow))
                    continue;
                
                var member = (TypeDefinition) _typeTable.GetMemberFromRow(Owner.Image, typeRow);
                Items.Add(member);
                SetOwner(member, Owner);
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