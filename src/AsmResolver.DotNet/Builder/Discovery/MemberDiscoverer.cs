using System;
using System.Collections.Generic;
using System.Linq;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.DotNet.Collections;
using AsmResolver.DotNet.Signatures;
using AsmResolver.DotNet.Signatures.Types;
using AsmResolver.PE.DotNet.Cil;
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
        
        private readonly TypeReference _eventHandlerTypeRef;
        private readonly TypeSignature _eventHandlerTypeSig;

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

            _eventHandlerTypeRef = new TypeReference(
                module,
                module.CorLibTypeFactory.CorLibScope,
                "System",
                nameof(EventHandler));
            
            _eventHandlerTypeSig = _eventHandlerTypeRef.ToTypeSignature();
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

            StuffFreeMemberSlots(placeHolderType, TableIndex.Field, AddPlaceHolderField);
            StuffFreeMemberSlots(placeHolderType, TableIndex.Method, AddPlaceHolderMethod);
            StuffFreeMemberSlots(placeHolderType, TableIndex.Property, AddPlaceHolderProperty);
            StuffFreeMemberSlots(placeHolderType, TableIndex.Event, AddPlaceHolderEvent);

            // TODO: stuff parameter table.
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

        private void StuffFreeMemberSlots<TMember>(TypeDefinition placeHolderType, TableIndex tableIndex, 
            Func<TypeDefinition, MetadataToken, TMember> createPlaceHolder) 
            where TMember : IMetadataMember
        {
            var freeRids = _freeRids[tableIndex];
            var members = GetResultList<TMember>(tableIndex);
            while (freeRids.Count > 0)
            {
                uint rid = freeRids.Dequeue();
                var token = new MetadataToken(tableIndex, rid);
                members[(int) (rid - 1)] = createPlaceHolder(placeHolderType, token);
            }
        }

        private FieldDefinition AddPlaceHolderField(TypeDefinition placeHolderType, MetadataToken token)
        {
            var placeHolderField = new FieldDefinition($"PlaceHolderField_{token.Rid.ToString()}",
                FieldAttributes.Private | FieldAttributes.Static,
                FieldSignature.CreateStatic(_module.CorLibTypeFactory.Object));
            placeHolderType.Fields.Add(placeHolderField);
            return placeHolderField;
        }

        private MethodDefinition AddPlaceHolderMethod(TypeDefinition placeHolderType, MetadataToken token)
        {
            var placeHolderMethod = new MethodDefinition($"PlaceHolderMethod_{token.Rid.ToString()}",
                MethodAttributes.Private | MethodAttributes.Static,
                MethodSignature.CreateStatic(_module.CorLibTypeFactory.Void));
            placeHolderMethod.CilMethodBody = new CilMethodBody(placeHolderMethod)
            {
                Instructions = {new CilInstruction(CilOpCodes.Ret)}
            };

            placeHolderType.Methods.Add(placeHolderMethod);
            return placeHolderMethod;
        }

        private PropertyDefinition AddPlaceHolderProperty(TypeDefinition placeHolderType, MetadataToken token)
        {
            // Define new property.
            var property = new PropertyDefinition($"PlaceHolderProperty_{token.Rid.ToString()}", 0,
                PropertySignature.CreateStatic(_module.CorLibTypeFactory.Object));

            // Define getter.
            var getMethod = new MethodDefinition($"get_{property.Name}",
                MethodAttributes.Private | MethodAttributes.Static | MethodAttributes.SpecialName
                | MethodAttributes.HideBySig, MethodSignature.CreateStatic(_module.CorLibTypeFactory.Object));
            getMethod.CilMethodBody = new CilMethodBody(getMethod)
            {
                Instructions = {new CilInstruction(CilOpCodes.Ldnull), new CilInstruction(CilOpCodes.Ret)}
            };
            
            // Add members.
            placeHolderType.Methods.Add(getMethod);
            placeHolderType.Properties.Add(property);
            property.Semantics.Add(new MethodSemantics(getMethod, MethodSemanticsAttributes.Getter));
            InsertOrAppendIfNew(getMethod);
            
            return property;
        }

        private EventDefinition AddPlaceHolderEvent(TypeDefinition placeHolderType, MetadataToken token)
        {
            // Define new event.
            var @event = new EventDefinition($"PlaceHolderEvent_{token.Rid.ToString()}", 0,
                _eventHandlerTypeRef);

            // Create signature for add/remove methods.
            var signature = MethodSignature.CreateStatic(
                _module.CorLibTypeFactory.Void,
                _module.CorLibTypeFactory.Object, 
                _eventHandlerTypeSig);

            var methodAttributes = MethodAttributes.Private | MethodAttributes.Static | MethodAttributes.SpecialName
                                   | MethodAttributes.HideBySig;
            
            // Define add.
            var addMethod = new MethodDefinition($"add_{@event.Name}", methodAttributes, signature);
            addMethod.CilMethodBody = new CilMethodBody(addMethod)
            {
                Instructions = {new CilInstruction(CilOpCodes.Ret)}
            };
            
            // Define remove.
            var removeMethod = new MethodDefinition($"remove_{@event.Name}", methodAttributes, signature);
            removeMethod.CilMethodBody = new CilMethodBody(removeMethod)
            {
                Instructions = {new CilInstruction(CilOpCodes.Ret)}
            };
            
            // Add members.
            placeHolderType.Methods.Add(addMethod);
            placeHolderType.Methods.Add(removeMethod);
            placeHolderType.Events.Add(@event);
            
            @event.Semantics.Add(new MethodSemantics(addMethod, MethodSemanticsAttributes.AddOn));
            @event.Semantics.Add(new MethodSemantics(removeMethod, MethodSemanticsAttributes.RemoveOn));

            InsertOrAppendIfNew(addMethod);
            InsertOrAppendIfNew(removeMethod);
            
            return @event;
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