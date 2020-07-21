using System;
using System.Collections.Generic;
using System.Linq;
using AsmResolver.Collections;
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
        private const MethodAttributes MethodPlaceHolderAttributes = MethodAttributes.Public
                                                                     | MethodAttributes.Abstract
                                                                     | MethodAttributes.Virtual
                                                                     | MethodAttributes.HideBySig
                                                                     | MethodAttributes.NewSlot;

        private const FieldAttributes FieldPlaceHolderAttributes = FieldAttributes.Public;

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
        
        private readonly IList<MethodDefinition> _allPlaceHolderMethods = new List<MethodDefinition>();
        private int _placeHolderParameterCounter;
        
        private readonly TypeReference _eventHandlerTypeRef;
        private readonly TypeSignature _eventHandlerTypeSig;

        private readonly IDictionary<TableIndex, IList<uint>> _freeRids= new Dictionary<TableIndex, IList<uint>>
        {
            [TableIndex.TypeDef] = new List<uint>(),
            [TableIndex.Field] = new List<uint>(),
            [TableIndex.Method] = new List<uint>(),
            [TableIndex.Param] = new List<uint>(),
            [TableIndex.Property] = new List<uint>(),
            [TableIndex.Event] = new List<uint>(),
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
                    _freeRids[tableIndex].Add(rid);
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
                var freeRids = _freeRids[memberType];

                var mask = member.MetadataToken.Table switch
                {
                    TableIndex.TypeDef => MemberDiscoveryFlags.PreserveTypeOrder,
                    TableIndex.Field => MemberDiscoveryFlags.PreserveFieldOrder,
                    TableIndex.Method => MemberDiscoveryFlags.PreserveMethodOrder,
                    TableIndex.Param => MemberDiscoveryFlags.PreserveParameterOrder,
                    TableIndex.Property => MemberDiscoveryFlags.PreservePropertyOrder,
                    TableIndex.Event => MemberDiscoveryFlags.PreserveEventOrder,
                    _ => throw new ArgumentOutOfRangeException(nameof(member))
                };
                if (member.MetadataToken.Rid != 0 && (_flags & mask) == mask)
                {
                    // Member is a new member but assigned a RID.
                    // Ensure enough rows are allocated, so that we can insert it in the right place.
                    while (memberList.Count < member.MetadataToken.Rid)
                    {
                        memberList.Add(null);
                        freeRids.Add((uint) memberList.Count);
                    }

                    // Check if the slot is available.
                    var slot = memberList[(int) member.MetadataToken.Rid - 1];
                    if (slot is {})
                    {
                        throw new ArgumentException(
                            $"{slot} and {member} are assigned the same RID {member.MetadataToken.Rid}.");
                    }

                    memberList[(int) member.MetadataToken.Rid - 1] = member;
                    freeRids.Remove(member.MetadataToken.Rid);
                    
                }
                else if (freeRids.Count > 0)
                {
                    // Use any free RID if it is available.
                    uint nextFreeRid = freeRids[0];
                    freeRids.RemoveAt(0);
                    memberList[(int) (nextFreeRid - 1)] = member;
                }
                else
                {
                    // Fallback method: Just append to the end of the table.
                    memberList.Add(member);
                }
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
            // Check if we need to do this at all.
            if (_freeRids.Values.All(q => q.Count == 0))
                return;
            
            // Create a new randomly generated namespace.
            string placeHolderNamespace = Guid.NewGuid().ToString("B");

            // Ensure that at least one dummy type exists, so that we can use it to insert placeholder members.
            TypeDefinition placeHolderType;
            if (_freeRids[TableIndex.TypeDef].Count == 0)
            {
                // There is no RID available for the dummy type, allocate a new one. 
                placeHolderType = new PlaceHolderTypeDefinition(_module, placeHolderNamespace, MetadataToken.Zero);
                _result.Types.Add(placeHolderType);
            }
            else
            {
                // There's at least one type RID free. Stuff free type slots and remember the first stuffed type.
                uint placeHolderTypeRid = _freeRids[TableIndex.TypeDef][0];
                StuffFreeMemberSlots<TypeDefinition>(null, TableIndex.TypeDef,
                    (_, token) => new PlaceHolderTypeDefinition(_module, placeHolderNamespace, token));
                placeHolderType = _result.Types[(int) placeHolderTypeRid - 1];
            }

            // Stuff remaining RIDs.
            StuffFreeMemberSlots(placeHolderType, TableIndex.Field, AddPlaceHolderField);
            StuffFreeMemberSlots(placeHolderType, TableIndex.Method, AddPlaceHolderMethod);
            StuffFreeMemberSlots(placeHolderType, TableIndex.Property, AddPlaceHolderProperty);
            StuffFreeMemberSlots(placeHolderType, TableIndex.Event, AddPlaceHolderEvent);
            StuffFreeMemberSlots(placeHolderType, TableIndex.Param, AddPlaceHolderParameter);
        }

        private void StuffFreeMemberSlots<TMember>(TypeDefinition placeHolderType, TableIndex tableIndex, 
            Func<TypeDefinition, MetadataToken, TMember> createPlaceHolder) 
            where TMember : IMetadataMember
        {
            // Get resulting member lists and free RIDs. 
            var freeRids = _freeRids[tableIndex];
            var members = GetResultList<TMember>(tableIndex);
            
            while (freeRids.Count > 0)
            {
                // Stuff free RID with a place holder member.
                uint rid = freeRids[0];
                freeRids.RemoveAt(0);
                var token = new MetadataToken(tableIndex, rid);
                members[(int) (rid - 1)] = createPlaceHolder(placeHolderType, token);
            }
        }

        private FieldDefinition AddPlaceHolderField(TypeDefinition placeHolderType, MetadataToken token)
        {
            // Create new placeholder field.
            var placeHolderField = new FieldDefinition(
                $"PlaceHolderField_{token.Rid.ToString()}",
                FieldPlaceHolderAttributes,
                FieldSignature.CreateStatic(_module.CorLibTypeFactory.Object));
            
            // Add the field to the type.
            placeHolderType.Fields.Add(placeHolderField);
            
            return placeHolderField;
        }

        private MethodDefinition AddPlaceHolderMethod(TypeDefinition placeHolderType, MetadataToken token)
        {
            // Create new placeholder method.
            var placeHolderMethod = new MethodDefinition(
                $"PlaceHolderMethod_{token.Rid.ToString()}",
                MethodPlaceHolderAttributes,
                MethodSignature.CreateInstance(_module.CorLibTypeFactory.Void));
            
            // Add the method to the type.
            placeHolderType.Methods.Add(placeHolderMethod);
            
            // Record placeholder methods, so that we can use them for adding placeholder parameters as well.
            _allPlaceHolderMethods.Add(placeHolderMethod);
            
            return placeHolderMethod;
        }

        private ParameterDefinition AddPlaceHolderParameter(TypeDefinition placeHolderType, MetadataToken token)
        {
            // If methods were not preserved, we need to create a new placeholder method to
            // contain our dummy parameters in.
            
            if (_allPlaceHolderMethods.Count == 0)
                InsertOrAppendIfNew(AddPlaceHolderMethod(placeHolderType, token));

            // Get current method to add the parameter def to.
            int methodIndex = _placeHolderParameterCounter % _allPlaceHolderMethods.Count;
            int parameterSequence = _placeHolderParameterCounter / _allPlaceHolderMethods.Count;
            var method = _allPlaceHolderMethods[methodIndex];

            // We start by adding parameter definitions for the hidden return parameter (sequence = 0).
            // If the parameter index is above 0, then we need to add it to the method signature for a
            // valid .NET module.
            if (parameterSequence > 0)
                method.Signature.ParameterTypes.Add(_module.CorLibTypeFactory.Object);
            
#if DEBUG
            string parameterName = method.ParameterDefinitions.Count == 0 ? null : $"placeholder{method.ParameterDefinitions.Count}";
#else
            const string parameterName = null;
#endif 
                
            // Create and add the placeholder parameter.
            var parameter = new ParameterDefinition((ushort) parameterSequence, parameterName, 0);
            method.ParameterDefinitions.Add(parameter);
            
            // Move to next method.
            _placeHolderParameterCounter++;
            
            return parameter;
        }

        private PropertyDefinition AddPlaceHolderProperty(TypeDefinition placeHolderType, MetadataToken token)
        {
            // Define new property.
            var property = new PropertyDefinition($"PlaceHolderProperty_{token.Rid.ToString()}", 0,
                PropertySignature.CreateStatic(_module.CorLibTypeFactory.Object));

            // Define getter.
            var getMethod = new MethodDefinition(
                $"get_{property.Name}",
                MethodPlaceHolderAttributes | MethodAttributes.SpecialName,
                MethodSignature.CreateStatic(_module.CorLibTypeFactory.Object));
            
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
            
            // Define add and remove methods.
            var addMethod = new MethodDefinition(
                $"add_{@event.Name}", 
                MethodPlaceHolderAttributes | MethodAttributes.SpecialName, 
                signature);
            var removeMethod = new MethodDefinition(
                $"remove_{@event.Name}", 
                MethodPlaceHolderAttributes | MethodAttributes.SpecialName, 
                signature);
            
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

        /// <summary>
        /// Represents a place holder type definition.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This class is kind of a hack. It allows us to return type definitions
        /// that seem to be present in the module without actually adding the type definition to
        /// <see cref="ModuleDefinition.TopLevelTypes"/> or as a nested type of one of these types.
        /// </para>
        /// <para>
        /// This has a nice effect that we never really change the internal state of the .NET module during the
        /// discovery process when we need to stuff free RIDs with placeholder types, preventing all kinds of
        /// problems.
        /// </para>
        /// </remarks>
        private sealed class PlaceHolderTypeDefinition : TypeDefinition
        {
            public PlaceHolderTypeDefinition(ModuleDefinition module, string ns, MetadataToken token)
                : base(token)
            {
                Namespace = ns;
                Name =  $"PlaceHolderTypeDef_{token.Rid.ToString()}";
                Attributes = TypeAttributes.Class | TypeAttributes.Abstract | TypeAttributes.NotPublic;
                BaseType = module.CorLibTypeFactory.Object.Type;
                
                // HACK: override the module containing this type:
                ((IOwnedCollectionElement<ModuleDefinition>) this).Owner = module;
            }
        }

    }
}