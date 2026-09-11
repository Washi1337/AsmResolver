using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using AsmResolver.Collections;
using AsmResolver.DotNet.Signatures;
using AsmResolver.PE.DotNet.Metadata;
using AsmResolver.PE.DotNet.Metadata.Tables;

namespace AsmResolver.DotNet.Builder.Discovery
{
    /// <summary>
    /// Provides a mechanism for traversing a module and collecting all members defined in it.
    /// </summary>
    public sealed class MemberDiscoverer
    {
        private const MethodAttributes MethodPlaceHolderAttributes =
            MethodAttributes.Public
            | MethodAttributes.Abstract
            | MethodAttributes.Virtual
            | MethodAttributes.HideBySig
            | MethodAttributes.NewSlot;

        private const FieldAttributes FieldPlaceHolderAttributes = FieldAttributes.Public;

        private readonly ModuleDefinition _module;

        private readonly MemberAllocation<TypeDefinition> _types;
        private readonly MemberAllocation<FieldDefinition> _fields;
        private readonly MemberAllocation<MethodDefinition> _methods;
        private readonly MemberAllocation<ParameterDefinition> _parameters;
        private readonly MemberAllocation<PropertyDefinition> _properties;
        private readonly MemberAllocation<EventDefinition> _events;

        private readonly List<MethodDefinition> _allPlaceHolderMethods = new();
        private int _placeHolderParameterCounter;

        private readonly TypeReference _eventHandlerTypeRef;
        private readonly TypeSignature _eventHandlerTypeSig;

        private MemberDiscoverer(ModuleDefinition module, MemberDiscoveryFlags flags)
        {
            _module = module ?? throw new ArgumentNullException(nameof(module));

            _types = new(TableIndex.TypeDef, (flags & MemberDiscoveryFlags.PreserveTypeOrder) != 0);
            _fields = new(TableIndex.Field, (flags & MemberDiscoveryFlags.PreserveFieldOrder) != 0);
            _methods = new(TableIndex.Method, (flags & MemberDiscoveryFlags.PreserveMethodOrder) != 0);
            _parameters = new(TableIndex.Param, (flags & MemberDiscoveryFlags.PreserveParameterOrder) != 0);
            _properties = new(TableIndex.Property, (flags & MemberDiscoveryFlags.PreservePropertyOrder) != 0);
            _events = new(TableIndex.Event, (flags & MemberDiscoveryFlags.PreserveEventOrder) != 0);

            _eventHandlerTypeRef = module.CorLibTypeFactory.CorLibScope.CreateTypeReference(
                "System",
                nameof(EventHandler)
            );

            _eventHandlerTypeSig = _eventHandlerTypeRef.ToTypeSignature(isValueType: false);
        }

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
            // 2) Do a normal  member tree traversal, collect new members, and try to place them in the available
            //    null slots. If that is not possible anymore, mark them as "floating" because there might still be
            //    newly added members that were actually assigned a new token.
            //
            // 3) After we are sure that every fixed member was assigned a slot, go over all floating members and
            //    place them either in any of the free slots or append them to the end of the list.
            //
            // 3) Any remaining null slots need to be stuffed with placeholder member definitions. These will be
            //    added to a dummy namespace for placeholder types, and added to a dummy type definition for all
            //    member definitions.

            var context = new MemberDiscoverer(module, flags);

            if (flags != MemberDiscoveryFlags.None)
                context.CollectExistingMembers();

            context.CollectNewlyAddedFixedMembers();
            context.AddFloatingMembers();

            if (flags != MemberDiscoveryFlags.None)
                context.FillFreeRids();

            Debug.Assert(context._types.Result.All(x => x is not null));
            Debug.Assert(context._fields.Result.All(x => x is not null));
            Debug.Assert(context._methods.Result.All(x => x is not null));
            Debug.Assert(context._parameters.Result.All(x => x is not null));
            Debug.Assert(context._properties.Result.All(x => x is not null));
            Debug.Assert(context._events.Result.All(x => x is not null));

            return new MemberDiscoveryResult(
                context._types.Result!,
                context._fields.Result!,
                context._methods.Result!,
                context._parameters.Result!,
                context._properties.Result!,
                context._events.Result!
            );
        }

        private void CollectExistingMembers()
        {
            if (_module.DotNetDirectory?.Metadata is null)
                return;

            var selection = _module.DotNetDirectory.Metadata.GetImpliedStreamSelection();
            var stream = selection.TablesStream;
            if (stream is null)
                return;

            if (_types.PreserveOrder)
                _types.InsertFromTable(_module, stream);
            if (_fields.PreserveOrder)
                _fields.InsertFromTable(_module, stream);
            if (_methods.PreserveOrder)
                _methods.InsertFromTable(_module, stream);
            if (_parameters.PreserveOrder)
                _parameters.InsertFromTable(_module, stream);
            if (_properties.PreserveOrder)
                _properties.InsertFromTable(_module, stream);
            if (_events.PreserveOrder)
                _events.InsertFromTable(_module, stream);
        }

        private void CollectNewlyAddedFixedMembers()
        {
            // Do a normal traversal of the member tree, and try to place newly added members in either the
            // available slots, or at the end of the member lists.

            foreach (var type in _module.GetAllTypes())
            {
                _types.InsertOrFloatIfNew(type);

                // Try find new fields.
                if (type.HasFields)
                {
                    for (int i = 0; i < type.Fields.Count; i++)
                        _fields.InsertOrFloatIfNew(type.Fields[i]);
                }

                // Try find new methods.
                if (type.HasMethods)
                {
                    for (int i = 0; i < type.Methods.Count; i++)
                    {
                        var method = type.Methods[i];
                        _methods.InsertOrFloatIfNew(method);

                        // Try find new parameters.
                        if (method.HasParameterDefinitions)
                        {
                            for (int j = 0; j < method.ParameterDefinitions.Count; j++)
                                _parameters.InsertOrFloatIfNew(method.ParameterDefinitions[j]);
                        }
                    }
                }

                // Try find new properties.
                if (type.HasProperties)
                {
                    for (int i = 0; i < type.Properties.Count; i++)
                        _properties.InsertOrFloatIfNew(type.Properties[i]);
                }

                // Try find new events.
                if (type.HasEvents)
                {
                    for (int i = 0; i < type.Events.Count; i++)
                        _events.InsertOrFloatIfNew(type.Events[i]);
                }
            }
        }

        private void AddFloatingMembers()
        {
            _types.InsertAllFloating();
            _fields.InsertAllFloating();
            _methods.InsertAllFloating();
            _parameters.InsertAllFloating();
            _properties.InsertAllFloating();
            _events.InsertAllFloating();
        }

        private void FillFreeRids()
        {
            // Check if we need to do this at all.
            if (!_types.HasFreeRids
                && !_fields.HasFreeRids
                && !_methods.HasFreeRids
                && !_parameters.HasFreeRids
                && !_properties.HasFreeRids
                && !_events.HasFreeRids)
            {
                return;
            }

            // Create a new randomly generated namespace.
            string placeHolderNamespace = Guid.NewGuid().ToString("B");

            // Ensure that at least one dummy type exists, so that we can use it to insert placeholder members.
            TypeDefinition placeHolderType;
            if (!_types.HasFreeRids)
            {
                // No free RID means we do not need to stuff with dummy types. However, we still need at least one
                // dummy type to insert the remaining placeholder fields, methods, parameters, properties and/or events.
                placeHolderType = new PlaceHolderTypeDefinition(_module, placeHolderNamespace, new MetadataToken(TableIndex.TypeDef, 0));
                _types.InsertInNextAvailableSlot(placeHolderType);
            }
            else
            {
                // There's at least one type RID free. Stuff free type slots and remember the first stuffed type.
                uint placeHolderTypeRid = _types.FreeRids[0];
                _types.FillFreeRids(
                    null,
                    (_, token) => new PlaceHolderTypeDefinition(_module, placeHolderNamespace, token)
                );
                placeHolderType = _types.Result[(int) placeHolderTypeRid - 1]!;
            }

            // Stuff remaining RIDs.
            _fields.FillFreeRids(placeHolderType, AddPlaceHolderField);
            _methods.FillFreeRids(placeHolderType, AddPlaceHolderMethod);
            _parameters.FillFreeRids(placeHolderType, AddPlaceHolderParameter);
            _properties.FillFreeRids(placeHolderType, AddPlaceHolderProperty);
            _events.FillFreeRids(placeHolderType, AddPlaceHolderEvent);
        }

        private FieldDefinition AddPlaceHolderField(TypeDefinition placeHolderType, MetadataToken token)
        {
            // Create new placeholder field.
            var placeHolderField = new FieldDefinition(
                name: RidToName(token.Rid),
                attributes: FieldPlaceHolderAttributes,
                fieldType: _module.CorLibTypeFactory.Object
            );

            // Add the field to the type.
            placeHolderType.Fields.Add(placeHolderField);

            return placeHolderField;
        }

        private MethodDefinition AddPlaceHolderMethod(TypeDefinition placeHolderType, MetadataToken token)
        {
            // Create new placeholder method.
            var placeHolderMethod = new MethodDefinition(
                name: RidToName(token.Rid),
                attributes: MethodPlaceHolderAttributes,
                signature: MethodSignature.CreateInstance(_module.CorLibTypeFactory.Void)
            );

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
                _methods.InsertInNextAvailableSlot(AddPlaceHolderMethod(placeHolderType, token));

            // Get current method to add the parameter def to.
            int methodIndex = _placeHolderParameterCounter % _allPlaceHolderMethods.Count;
            int parameterSequence = _placeHolderParameterCounter / _allPlaceHolderMethods.Count;
            var method = _allPlaceHolderMethods[methodIndex];

            // We start by adding parameter definitions for the hidden return parameter (sequence = 0).
            // If the parameter index is above 0, then we need to add it to the method signature for a
            // valid .NET module.
            if (parameterSequence > 0)
                method.Signature!.ParameterTypes.Add(_module.CorLibTypeFactory.Object);

#if DEBUG
            string? parameterName = method.ParameterDefinitions.Count == 0 ? null : $"_{method.ParameterDefinitions.Count}";
#else
            const string? parameterName = null;
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
            var property = new PropertyDefinition(
                name: RidToName(token.Rid),
                attributes: PropertyAttributes.None,
                signature: PropertySignature.CreateStatic(_module.CorLibTypeFactory.Object)
            );

            // Define getter.
            var getMethod = new MethodDefinition(
                $"get_{property.Name}",
                MethodPlaceHolderAttributes | MethodAttributes.SpecialName | MethodAttributes.Static,
                MethodSignature.CreateStatic(_module.CorLibTypeFactory.Object)
            );

            // Add members.
            placeHolderType.Methods.Add(getMethod);
            placeHolderType.Properties.Add(property);
            property.Semantics.Add(new MethodSemantics(getMethod, MethodSemanticsAttributes.Getter));
            _methods.InsertInNextAvailableSlot(getMethod);

            return property;
        }

        private EventDefinition AddPlaceHolderEvent(TypeDefinition placeHolderType, MetadataToken token)
        {
            // Define new event.
            var @event = new EventDefinition(
                name: $"_{token.Rid}",
                attributes: EventAttributes.None,
                eventType: _eventHandlerTypeRef
            );

            // Create signature for add/remove methods.
            var signature = MethodSignature.CreateStatic(
                _module.CorLibTypeFactory.Void,
                [_module.CorLibTypeFactory.Object, _eventHandlerTypeSig]
            );

            // Define add and remove methods.
            var addMethod = new MethodDefinition(
                $"add_{@event.Name}",
                MethodPlaceHolderAttributes | MethodAttributes.SpecialName | MethodAttributes.Static,
                signature
            );
            var removeMethod = new MethodDefinition(
                $"remove_{@event.Name}",
                MethodPlaceHolderAttributes | MethodAttributes.SpecialName | MethodAttributes.Static,
                signature
            );

            // Add members.
            placeHolderType.Methods.Add(addMethod);
            placeHolderType.Methods.Add(removeMethod);
            placeHolderType.Events.Add(@event);

            @event.Semantics.Add(new MethodSemantics(addMethod, MethodSemanticsAttributes.AddOn));
            @event.Semantics.Add(new MethodSemantics(removeMethod, MethodSemanticsAttributes.RemoveOn));

            _methods.InsertInNextAvailableSlot(addMethod);
            _methods.InsertInNextAvailableSlot(removeMethod);

            return @event;
        }

        private class MemberAllocation<TMember>(TableIndex tableIndex, bool preserveOrder)
            where TMember : class, IMetadataMember, IModuleProvider
        {
            public TableIndex TableIndex
            {
                get;
            } = tableIndex;

            public bool PreserveOrder
            {
                get;
            } = preserveOrder;

            public List<TMember?> Result
            {
                get;
            } = [];

            public List<TMember> Floating
            {
                get;
            } = [];

            public List<uint> FreeRids
            {
                get;
            } = [];

            public bool HasFreeRids => FreeRids.Count > 0;

            private bool IsNewMember(TMember member)
            {
                return member.MetadataToken.Rid == 0 // Member has not been assigned a RID.
                    || member.MetadataToken.Rid > Result.Count // Member's RID does not fall within the existing md range.
                    || Result[(int) (member.MetadataToken.Rid - 1)] != member; // Member's RID refers to a different member.
            }

            public void InsertFromTable(ModuleDefinition module, TablesStream tablesStream)
            {
                // Get original number of elements in the table.
                int count = tablesStream.GetTable(TableIndex).Count;

                // Traverse the table, look up the high-level metadata model, and see if it is still present.
                for (uint rid = 1; rid <= count; rid++)
                {
                    var token = new MetadataToken(TableIndex, rid);

                    if (module.TryLookupMember(token, out TMember? definition) && definition.ContextModule == module)
                    {
                        // Member is still present in the module.
                        Result.Add(definition);
                    }
                    else
                    {
                        // Member was removed from the module, mark current RID available.
                        FreeRids.Add(rid);
                        Result.Add(null);
                    }
                }
            }

            public void InsertOrFloatIfNew(TMember member)
            {
                if (!IsNewMember(member))
                    return;

                if (member.MetadataToken.Rid != 0 && PreserveOrder)
                {
                    // Member is a new member but assigned a RID.
                    // Ensure enough rows are allocated, so that we can insert it in the right place.
#if NET8_0_OR_GREATER
                    Result.EnsureCapacity((int) member.MetadataToken.Rid);
#else
                    if (Result.Capacity < member.MetadataToken.Rid)
                        Result.Capacity = (int) member.MetadataToken.Rid;
#endif
                    while (Result.Count < member.MetadataToken.Rid)
                    {
                        Result.Add(null);
                        FreeRids.Add((uint) Result.Count);
                    }

                    // Check if the slot is available.
                    if (Result[(int) member.MetadataToken.Rid - 1] is { } slot)
                        throw new MetadataTokenConflictException(slot, member, member.MetadataToken.Rid);

                    Result[(int) member.MetadataToken.Rid - 1] = member;
                    FreeRids.Remove(member.MetadataToken.Rid);
                }
                else if (!TryInsertInNullSlot(member))
                {
                    Floating.Add(member);
                }
            }

            private bool TryInsertInNullSlot(TMember member)
            {
                if (FreeRids.Count <= 0)
                    return false;

                uint nextFreeRid = FreeRids[0];
                FreeRids.RemoveAt(0);
                Result[(int) (nextFreeRid - 1)] = member;
                return true;
            }

            public void FillFreeRids(
                TypeDefinition? placeHolderType,
                Func<TypeDefinition, MetadataToken, TMember> createPlaceHolder
            )
            {
                foreach (uint rid in FreeRids)
                {
                    var token = new MetadataToken(TableIndex, rid);
                    Result[(int) (rid - 1)] = createPlaceHolder(placeHolderType!, token);
                }

                FreeRids.Clear();
            }

            public void InsertAllFloating()
            {
                foreach (var floating in Floating)
                    InsertInNextAvailableSlot(floating);
                Floating.Clear();
            }

            public void InsertInNextAvailableSlot(TMember member)
            {
                if (!TryInsertInNullSlot(member))
                    Result.Add(member);
            }
        }

        private static Utf8String RidToName(uint rid)
        {
            const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";

            int index = (int) rid;
            int length = Math.Max(1, (int) Math.Ceiling(Math.Log(index, alphabet.Length)));

            byte[] buffer = new byte[length];
            for (int i = 0; i < length; i++)
            {
                index = Math.DivRem(index, alphabet.Length, out int remainder);
                buffer[i] = (byte) alphabet[remainder];
            }

            return Utf8String.CreateUnsafe(buffer);
        }

        /// <summary>
        /// Represents a placeholder type definition.
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
                Name = RidToName(token.Rid);
                Attributes = TypeAttributes.Class | TypeAttributes.Abstract | TypeAttributes.NotPublic;
                BaseType = module.CorLibTypeFactory.Object.Type;

                // HACK: override the module containing this type:
                ((IOwnedCollectionElement<ITypeOwner>) this).Owner = module;
            }
        }

    }
}
