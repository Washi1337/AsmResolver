using System;
using System.Collections.Generic;
using System.Linq;
using AsmResolver.DotNet.Collections;
using AsmResolver.PE.DotNet.Metadata.Tables;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;

namespace AsmResolver.DotNet.Builder.Discovery
{
    public static class MemberDiscoverer
    {
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
            
            var context = new MemberDiscoveryContext(module, flags);
            
            CollectExistingMembers(context);
            CollectNewlyAddedMembers(context);
            StuffFreeMemberSlots(context);

            return context.Result;
        }

        private static void CollectExistingMembers(MemberDiscoveryContext context)
        {
            var flags = context.Flags;

            if ((flags & MemberDiscoveryFlags.PreserveTypeOrder) != 0)
                CollectMembersFromTable<TypeDefinition>(context, TableIndex.TypeDef);
            if ((flags & MemberDiscoveryFlags.PreserveFieldOrder) != 0)
                CollectMembersFromTable<FieldDefinition>(context, TableIndex.Field);
            if ((flags & MemberDiscoveryFlags.PreserveMethodOrder) != 0)
                CollectMembersFromTable<MethodDefinition>(context, TableIndex.Method);
            if ((flags & MemberDiscoveryFlags.PreserveParameterOrder) != 0)
                CollectMembersFromTable<ParameterDefinition>(context, TableIndex.Param);
            if ((flags & MemberDiscoveryFlags.PreservePropertyOrder) != 0)
                CollectMembersFromTable<PropertyDefinition>(context, TableIndex.Property);
            if ((flags & MemberDiscoveryFlags.PreserveEventOrder) != 0)
                CollectMembersFromTable<EventDefinition>(context, TableIndex.Event);
        }

        private static void CollectMembersFromTable<TMember>(MemberDiscoveryContext context, TableIndex tableIndex)
            where TMember: IMetadataMember, IModuleProvider
        {
            // Get original number of elements in the table. 
            int count = context.Module.DotNetDirectory.Metadata
                .GetStream<TablesStream>()
                .GetTable(tableIndex)
                .Count;

            var resultingList = context.GetResultList<TMember>(tableIndex);

            // Traverse the table, look up the high-level metadata model, and see if it is still present.
            for (uint rid = 1; rid <= count; rid++)
            {
                var token = new MetadataToken(tableIndex, rid);
                var definition = (TMember) context.Module.LookupMember(token);

                if (definition.Module == context.Module)
                {
                    // Member is still present in the module.
                    resultingList.Add(definition);
                }
                else
                {
                    // Member was removed from the module, mark current RID available.
                    context.FreeRids[tableIndex].Enqueue(rid);
                    resultingList.Add(default);
                }
            }
        }

        private static void CollectNewlyAddedMembers(MemberDiscoveryContext context)
        {
            // Do a normal traversal of the member tree, and try to place newly added members in either the
            // available slots, or at the end of the member lists.
            
            foreach (var type in context.Module.GetAllTypes())
            {
                InsertOrAppendIfNew(context, type);

                // Try find new fields.
                for (int i = 0; i < type.Fields.Count; i++)
                    InsertOrAppendIfNew(context, type.Fields[i]);

                // Try find new methods.
                for (int i = 0; i < type.Methods.Count; i++)
                {
                    var method = type.Methods[i];
                    InsertOrAppendIfNew(context, method);

                    foreach (var parameter in method.ParameterDefinitions)
                        InsertOrAppendIfNew(context, parameter);
                }

                // Try find new properties.
                for (int i = 0; i < type.Properties.Count; i++)
                    InsertOrAppendIfNew(context, type.Properties[i]);

                // Try find new events.
                for (int i = 0; i < type.Events.Count; i++)
                    InsertOrAppendIfNew(context, type.Events[i]);
            }
        }

        private static void InsertOrAppendIfNew<TMember>(MemberDiscoveryContext context, TMember member)
            where TMember : class, IMetadataMember
        {
            var memberType = member.MetadataToken.Table;
            var memberList = context.GetResultList<TMember>(memberType);
            
            if (IsNewMember(memberList, member))
            {
                // Check if there is any free RID available to use.
                var freeRids = context.FreeRids[memberType];
                if (freeRids.Count > 0)
                    memberList[(int) (freeRids.Dequeue() - 1)] = member;
                else
                    memberList.Add(member);
            }
        }

        private static bool IsNewMember<TMember>(IList<TMember> existingMembers, TMember member)
            where TMember : class, IMetadataMember
        {
            return member.MetadataToken.Rid == 0 // Type has not been assigned a RID.
                   || member.MetadataToken.Rid > existingMembers.Count // Type's RID does not fall within the existing md range.
                   || existingMembers[(int) (member.MetadataToken.Rid - 1)] != member; // Type's RID refers to a different type.
        }

        private static void StuffFreeMemberSlots(MemberDiscoveryContext context)
        {
            if (context.FreeRids.Values.All(q => q.Count == 0))
                return;
            
            string placeHolderNamespace = Guid.NewGuid().ToString();

            // Stuff type rows with placeholders.
            var freeRids = context.FreeRids[TableIndex.TypeDef];
            var types = context.Result.Types;
            while (freeRids.Count > 0)
            {
                uint rid = freeRids.Dequeue();
                var token = new MetadataToken(TableIndex.TypeDef, rid);
                types[(int) (rid - 1)] = new PlaceHolderTypeDefinition(context.Module, placeHolderNamespace, token);
            }
            
            // TODO: stuff remaining member types.
        }

        private sealed class MemberDiscoveryContext
        {
            public MemberDiscoveryContext(ModuleDefinition module, MemberDiscoveryFlags flags)
            {
                Module = module ?? throw new ArgumentNullException(nameof(module));
                Flags = flags;
            }
            
            public ModuleDefinition Module
            {
                get;
            }

            public MemberDiscoveryFlags Flags
            {
                get;
            }

            public IDictionary<TableIndex, Queue<uint>> FreeRids
            {
                get;
            } = new Dictionary<TableIndex, Queue<uint>>
            {
                [TableIndex.TypeDef] = new Queue<uint>(),
                [TableIndex.Field] = new Queue<uint>(),
                [TableIndex.Method] = new Queue<uint>(),
                [TableIndex.Param] = new Queue<uint>(),
                [TableIndex.Property] = new Queue<uint>(),
                [TableIndex.Event] = new Queue<uint>(),
            };

            public MemberDiscoveryResult Result
            {
                get;
            } = new MemberDiscoveryResult();

            public IList<TMember> GetResultList<TMember>(TableIndex tableIndex)
                where TMember : IMetadataMember
            {
                return tableIndex switch
                {
                    TableIndex.TypeDef => (IList<TMember>) Result.Types,
                    TableIndex.Field => (IList<TMember>) Result.Fields,
                    TableIndex.Method => (IList<TMember>) Result.Methods,
                    TableIndex.Param => (IList<TMember>) Result.Parameters,
                    TableIndex.Property => (IList<TMember>) Result.Properties,
                    TableIndex.Event => (IList<TMember>) Result.Events,
                    _ => throw new ArgumentOutOfRangeException(nameof(tableIndex))
                };
            }
        } 

        private sealed class PlaceHolderTypeDefinition : TypeDefinition
        {
            public PlaceHolderTypeDefinition(ModuleDefinition module, string ns, MetadataToken token)
                : base(token)
            {
                ((IOwnedCollectionElement<ModuleDefinition>) this).Owner = module;
                Namespace = ns;
                Name = $"DeletedTypeDef_{token.Rid.ToString()}";
                Attributes = TypeAttributes.Class | TypeAttributes.Abstract | TypeAttributes.Sealed
                             | TypeAttributes.NotPublic;
            }
        }
    }
}