using System;
using System.Collections.Generic;
using System.Linq;
using AsmResolver.DotNet.Collections;
using AsmResolver.DotNet.Signatures;
using AsmResolver.PE.DotNet.Metadata.Tables;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;

namespace AsmResolver.DotNet.Builder.Discovery
{
    /// <summary>
    /// Provides a mechanism for traversing a module and collecting all members defined in it.
    /// </summary>
    public sealed class MemberDiscoverer
    {
        /// <summary>
        /// Performs a traversal on the provided module and collects all member defined in it. 
        /// </summary>
        /// <param name="module">The module to traverse.</param>
        /// <param name="flags">Flags indicating which member lists the original order needs to be preserved.</param>
        /// <returns>The collected members.</returns>
        public static MemberDiscoveryResult DiscoverMembersInModule(ModuleDefinition module, MemberDiscoveryFlags flags)
        {
            // Strategy:
            //
            // 1) Collect all members that were present in the original metadata tables, and leave null slots
            //    in the lists when the member was removed from the module, to preserve RIDs of existing members.
            //
            // 2) Do a normal member tree traversal, collect new members, and try to place them in the available
            //    null slots. If that is not possible anymore, just append to the end of the list.
            //
            // 3) Any remaining null slots need to be stuffed with placeholder member definitions. These will be
            //    added to a dummy namespace for placeholder types, and added to a dummy type definition for all
            //    member definitions.
            
            var context = new MemberDiscoverer(module, flags);
            
            if (flags != MemberDiscoveryFlags.None)
                context.CollectExistingMembers();

            context.CollectNewlyAddedMembers();
                
            if (flags != MemberDiscoveryFlags.None)
                context.StuffFreeMemberSlots();

            return context._result;
        }

        private readonly ModuleDefinition _module;
        private readonly MemberDiscoveryFlags _flags;
        private readonly MemberDiscoveryResult _result = new MemberDiscoveryResult();

        private readonly IDictionary<TableIndex, Queue<uint>> _freeRids= new Dictionary<TableIndex, Queue<uint>>
        {
            [TableIndex.TypeDef] = new Queue<uint>(),
            [TableIndex.Field] = new Queue<uint>(),
            [TableIndex.Method] = new Queue<uint>(),
            [TableIndex.Param] = new Queue<uint>(),
            [TableIndex.Property] = new Queue<uint>(),
            [TableIndex.Event] = new Queue<uint>(),
        };

        private MemberDiscoverer(ModuleDefinition module, MemberDiscoveryFlags flags)
        {
            _module = module ?? throw new ArgumentNullException(nameof(module));
            _flags = flags;
        }

        private IList<TMember> GetResultList<TMember>(TableIndex tableIndex)
            where TMember : IMetadataMember
        {
            return tableIndex switch
            {
                TableIndex.TypeDef => (IList<TMember>) _result.Types,
                TableIndex.Field => (IList<TMember>) _result.Fields,
                TableIndex.Method => (IList<TMember>) _result.Methods,
                TableIndex.Param => (IList<TMember>) _result.Parameters,
                TableIndex.Property => (IList<TMember>) _result.Properties,
                TableIndex.Event => (IList<TMember>) _result.Events,
                _ => throw new ArgumentOutOfRangeException(nameof(tableIndex))
            };
        }

        private void CollectExistingMembers()
        {
            if ((_flags & MemberDiscoveryFlags.PreserveTypeOrder) != 0)
                CollectMembersFromTable<TypeDefinition>(TableIndex.TypeDef);
            if ((_flags & MemberDiscoveryFlags.PreserveFieldOrder) != 0)
                CollectMembersFromTable<FieldDefinition>(TableIndex.Field);
            if ((_flags & MemberDiscoveryFlags.PreserveMethodOrder) != 0)
                CollectMembersFromTable<MethodDefinition>(TableIndex.Method);
            if ((_flags & MemberDiscoveryFlags.PreserveParameterOrder) != 0)
                CollectMembersFromTable<ParameterDefinition>(TableIndex.Param);
            if ((_flags & MemberDiscoveryFlags.PreservePropertyOrder) != 0)
                CollectMembersFromTable<PropertyDefinition>(TableIndex.Property);
            if ((_flags & MemberDiscoveryFlags.PreserveEventOrder) != 0)
                CollectMembersFromTable<EventDefinition>(TableIndex.Event);
        }

        private void CollectMembersFromTable<TMember>(TableIndex tableIndex)
            where TMember: IMetadataMember, IModuleProvider
        {
            // Get original number of elements in the table. 
            int count = _module.DotNetDirectory.Metadata
                .GetStream<TablesStream>()
                .GetTable(tableIndex)
                .Count;

            var resultingList = GetResultList<TMember>(tableIndex);

            // Traverse the table, look up the high-level metadata model, and see if it is still present.
            for (uint rid = 1; rid <= count; rid++)
            {
                var token = new MetadataToken(tableIndex, rid);
                var definition = (TMember) _module.LookupMember(token);

                if (definition.Module == _module)
                {
                    // Member is still present in the module.
                    resultingList.Add(definition);
                }
                else
                {
                    // Member was removed from the module, mark current RID available.
                    _freeRids[tableIndex].Enqueue(rid);
                    resultingList.Add(default);
                }
            }
        }

        private void CollectNewlyAddedMembers()
        {
            // Do a normal traversal of the member tree, and try to place newly added members in either the
            // available slots, or at the end of the member lists.
            
            foreach (var type in _module.GetAllTypes())
            {
                InsertOrAppendIfNew(type);

                // Try find new fields.
                for (int i = 0; i < type.Fields.Count; i++)
                    InsertOrAppendIfNew(type.Fields[i]);

                // Try find new methods.
                for (int i = 0; i < type.Methods.Count; i++)
                {
                    var method = type.Methods[i];
                    InsertOrAppendIfNew(method);

                    foreach (var parameter in method.ParameterDefinitions)
                        InsertOrAppendIfNew(parameter);
                }

                // Try find new properties.
                for (int i = 0; i < type.Properties.Count; i++)
                    InsertOrAppendIfNew(type.Properties[i]);

                // Try find new events.
                for (int i = 0; i < type.Events.Count; i++)
                    InsertOrAppendIfNew(type.Events[i]);
            }
        }

        private void InsertOrAppendIfNew<TMember>(TMember member)
            where TMember : class, IMetadataMember
        {
            var memberType = member.MetadataToken.Table;
            var memberList = GetResultList<TMember>(memberType);
            
            if (IsNewMember(memberList, member))
            {
                // Check if there is any free RID available to use.
                var freeRids = _freeRids[memberType];
                if (freeRids.Count > 0)
                    memberList[(int) (freeRids.Dequeue() - 1)] = member;
                else
                    memberList.Add(member);
            }
        }

        private static bool IsNewMember<TMember>(IList<TMember> existingMembers, TMember member)
            where TMember : class, IMetadataMember
        {
            return member.MetadataToken.Rid == 0 // Member has not been assigned a RID.
                   || member.MetadataToken.Rid > existingMembers.Count // Member's RID does not fall within the existing md range.
                   || existingMembers[(int) (member.MetadataToken.Rid - 1)] != member; // Member's RID refers to a different member.
        }

        private void StuffFreeMemberSlots()
        {
            if (_freeRids.Values.All(q => q.Count == 0))
                return;
            
            string placeHolderNamespace = Guid.NewGuid().ToString();

            uint placeHolderTypeRid = 0;

            TypeDefinition placeHolderType;
            if (_freeRids[TableIndex.TypeDef].Count == 0)
            {
                placeHolderType = new PlaceHolderTypeDefinition(_module, placeHolderNamespace, MetadataToken.Zero);
                _result.Types.Add(placeHolderType);
            }
            else
            {
                placeHolderType = null;
                placeHolderTypeRid = _freeRids[TableIndex.TypeDef].Peek();
            }

            StuffFreeTypeSlots(placeHolderNamespace);
            
            placeHolderType ??= _result.Types[(int) placeHolderTypeRid - 1];

            StuffFreeFieldSlots(placeHolderType);
            StuffFreeMethodSlots(placeHolderType);

            // TODO: stuff remaining member types.
        }

        private void StuffFreeTypeSlots(string placeHolderNamespace)
        {
            // Stuff type rows with placeholders.
            var freeTypeRids = _freeRids[TableIndex.TypeDef];
            var types = _result.Types;
            while (freeTypeRids.Count > 0)
            {
                uint rid = freeTypeRids.Dequeue();
                var token = new MetadataToken(TableIndex.TypeDef, rid);
                types[(int) (rid - 1)] = new PlaceHolderTypeDefinition(_module, placeHolderNamespace, token);
            }
        }

        private void StuffFreeFieldSlots(TypeDefinition placeHolderType)
        {
            var freeFieldRids = _freeRids[TableIndex.Field];
            var fields = _result.Fields;
            while (freeFieldRids.Count > 0)
            {
                uint rid = freeFieldRids.Dequeue();
                var token = new MetadataToken(TableIndex.Field, rid);

                var placeHolderField = new FieldDefinition($"PlaceHolderFieldDef_{token.Rid.ToString()}",
                    FieldAttributes.Private | FieldAttributes.Static,
                    FieldSignature.CreateStatic(_module.CorLibTypeFactory.Object));
                placeHolderType.Fields.Add(placeHolderField);
                fields[(int) (rid - 1)] = placeHolderField;
            }
        }

        private void StuffFreeMethodSlots(TypeDefinition placeHolderType)
        {
            var freeMethodRids = _freeRids[TableIndex.Method];
            var methods = _result.Methods;
            while (freeMethodRids.Count > 0)
            {
                uint rid = freeMethodRids.Dequeue();
                var token = new MetadataToken(TableIndex.Field, rid);

                var placeHolderMethod = new MethodDefinition($"PlaceHolderMethodDef_{token.Rid.ToString()}",
                    MethodAttributes.Private | MethodAttributes.Static,
                    MethodSignature.CreateStatic(_module.CorLibTypeFactory.Void));
                placeHolderType.Methods.Add(placeHolderMethod);
                methods[(int) (rid - 1)] = placeHolderMethod;
            }
        }

        private sealed class PlaceHolderTypeDefinition : TypeDefinition
        {
            public PlaceHolderTypeDefinition(ModuleDefinition module, string ns, MetadataToken token)
                : base(token)
            {
                ((IOwnedCollectionElement<ModuleDefinition>) this).Owner = module;
                Namespace = ns;
                Name =  $"PlaceHolderTypeDef_{token.Rid.ToString()}";
                Attributes = TypeAttributes.Class | TypeAttributes.Abstract | TypeAttributes.Sealed
                             | TypeAttributes.NotPublic;
            }
        }

        
        
    }
}