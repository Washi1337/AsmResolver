using System;
using System.Collections.Generic;
using System.Threading;
using AsmResolver.DotNet.Blob;
using AsmResolver.DotNet.Collections;
using AsmResolver.PE.DotNet.Metadata;
using AsmResolver.PE.DotNet.Metadata.Guid;
using AsmResolver.PE.DotNet.Metadata.Strings;
using AsmResolver.PE.DotNet.Metadata.Tables;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;
using AsmResolver.PE.DotNet.Metadata.UserStrings;

namespace AsmResolver.DotNet.Serialized
{
    /// <summary>
    /// Represents a lazily initialized implementation of <see cref="ModuleDefinition"/>  that is read from a
    /// .NET metadata image. 
    /// </summary>
    public class SerializedModuleDefinition : ModuleDefinition
    {
        private readonly IMetadata _metadata;
        private readonly ModuleDefinitionRow _row;
        private readonly CachedSerializedMemberFactory _memberFactory;

        private IDictionary<uint, IList<uint>> _typeDefTree;
        private IDictionary<uint, uint> _parentTypeRids;
        
        private MetadataRange[] _fieldLists;
        private uint[] _fieldDeclaringTypes;
        
        private MetadataRange[] _methodLists;
        private uint[] _methodDeclaringTypes;
        
        private MetadataRange[] _paramLists;
        private uint[] _parameterMethods;
        
        private IDictionary<uint, MetadataRange> _propertyLists;
        private uint[] _propertyDeclaringTypes;
        
        private IDictionary<uint, MetadataRange> _eventLists;
        private uint[] _eventDeclaringTypes;

        /// <summary>
        /// Creates a module definition from a module metadata row.
        /// </summary>
        /// <param name="metadata">The object providing access to the underlying metadata streams.</param>
        /// <param name="token">The token to initialize the module for.</param>
        /// <param name="row">The metadata table row to base the module definition on.</param>
        public SerializedModuleDefinition(IMetadata metadata, MetadataToken token, ModuleDefinitionRow row)
            : base(token)
        {
            _metadata = metadata;
            _row = row;
            Generation = row.Generation;
            MetadataToken = token;

            _memberFactory = new CachedSerializedMemberFactory(metadata, this);
            CorLibTypeFactory = new CorLibTypeFactory(FindMostRecentCorLib());
        }

        /// <inheritdoc />
        public override IMetadataMember LookupMember(MetadataToken token) =>
            !TryLookupMember(token, out var member)
                ? throw new ArgumentException($"Cannot resolve metadata token {token}.")
                : member;

        /// <inheritdoc />
        public override bool TryLookupMember(MetadataToken token, out IMetadataMember member) =>
            _memberFactory.TryLookupMember(token, out member);

        /// <inheritdoc />
        public override string LookupString(MetadataToken token) => 
            !TryLookupString(token, out var member)
                ? throw new ArgumentException($"Cannot resolve string token {token}.")
                : member;

        /// <inheritdoc />
        public override bool TryLookupString(MetadataToken token, out string value)
        {
            value = _metadata.GetStream<UserStringsStream>().GetStringByIndex(token.Rid);
            return value == null;
        }

        /// <inheritdoc />
        public override IndexEncoder GetIndexEncoder(CodedIndex codedIndex) =>
            _metadata.GetStream<TablesStream>().GetIndexEncoder(codedIndex);

        /// <inheritdoc />
        protected override string GetName() 
            => _metadata.GetStream<StringsStream>()?.GetStringByIndex(_row.Name);

        /// <inheritdoc />
        protected override Guid GetMvid() 
            => _metadata.GetStream<GuidStream>()?.GetGuidByIndex(_row.Mvid) ?? Guid.Empty;

        /// <inheritdoc />
        protected override Guid GetEncId()
            => _metadata.GetStream<GuidStream>()?.GetGuidByIndex(_row.EncId) ?? Guid.Empty;

        /// <inheritdoc />
        protected override Guid GetEncBaseId()
            => _metadata.GetStream<GuidStream>()?.GetGuidByIndex(_row.EncBaseId) ?? Guid.Empty;

        /// <inheritdoc />
        protected override IList<TypeDefinition> GetTopLevelTypes()
        {
            EnsureTypeDefinitionTreeInitialized();
            
            var types = new OwnedCollection<ModuleDefinition, TypeDefinition>(this);

            var typeDefTable = _metadata.GetStream<TablesStream>().GetTable<TypeDefinitionRow>(TableIndex.TypeDef);
            for (int i = 0; i < typeDefTable.Count; i++)
            {
                uint rid = (uint) i + 1;
                if (!_parentTypeRids.ContainsKey(rid))
                {
                    var token = new MetadataToken(TableIndex.TypeDef, rid);
                    types.Add(_memberFactory.LookupTypeDefinition(token));
                }
            }

            return types;
        }

        private void EnsureTypeDefinitionTreeInitialized()
        {
            if (_typeDefTree is null)
                InitializeTypeDefinitionTree();
        }

        private void InitializeTypeDefinitionTree()
        {
            var tablesStream = _metadata.GetStream<TablesStream>();
            var nestedClassTable = tablesStream.GetTable<NestedClassRow>(TableIndex.NestedClass);
            
            _typeDefTree = new Dictionary<uint, IList<uint>>();
            _parentTypeRids = new Dictionary<uint, uint>();
            
            foreach (var nestedClass in nestedClassTable)
            {
                _parentTypeRids.Add(nestedClass.NestedClass, nestedClass.EnclosingClass);
                var nestedTypeRids = GetNestedTypeRids(nestedClass.EnclosingClass);
                nestedTypeRids.Add(nestedClass.NestedClass);
            }
        }

        internal IList<uint> GetNestedTypeRids(uint enclosingTypeRid)
        {
            if (!_typeDefTree.TryGetValue(enclosingTypeRid, out var nestedRids))
            {
                nestedRids = new List<uint>();
                _typeDefTree.Add(enclosingTypeRid, nestedRids);
            }

            return nestedRids;
        }

        internal uint GetParentTypeRid(uint nestedTypeRid)
        {
            EnsureTypeDefinitionTreeInitialized();
            _parentTypeRids.TryGetValue(nestedTypeRid, out uint parentRid);
            return parentRid;
        }

        private void EnsureTypeMemberListsInitialized()
        {
            if (_fieldLists is null || _methodLists is null)
                InitializeTypeMemberLists();
        }
        
        private void InitializeTypeMemberLists()
        {
            var tablesStream = _metadata.GetStream<TablesStream>();
            var typeDefTable = tablesStream.GetTable(TableIndex.TypeDef);
            var fieldDefTable = tablesStream.GetTable(TableIndex.Field);
            var methodDefTable = tablesStream.GetTable(TableIndex.Method);

            var fieldLists = new MetadataRange[typeDefTable.Count];
            var methodLists = new MetadataRange[typeDefTable.Count];
            var fieldDeclaringTypes = new uint[fieldDefTable.Count];
            var methodDeclaringTypes = new uint[methodDefTable.Count];
            
            for (uint typeDefRid = 1; typeDefRid <= typeDefTable.Count; typeDefRid++)
            {
                InitializeMemberList(typeDefRid, tablesStream.GetFieldRange(typeDefRid), fieldLists, fieldDeclaringTypes);
                InitializeMemberList(typeDefRid, tablesStream.GetMethodRange(typeDefRid), methodLists, methodDeclaringTypes);
            }

            Interlocked.CompareExchange(ref _fieldLists, fieldLists, null);
            Interlocked.CompareExchange(ref _methodLists, methodLists, null);
            Interlocked.CompareExchange(ref _fieldDeclaringTypes, fieldDeclaringTypes, null);
            Interlocked.CompareExchange(ref _methodDeclaringTypes, methodDeclaringTypes, null);
        }

        internal MetadataRange GetFieldRange(uint typeRid)
        {
            EnsureTypeMemberListsInitialized();
            return typeRid - 1 < _fieldLists.Length
                ? _fieldLists[typeRid - 1]
                : MetadataRange.Empty;
        } 

        internal MetadataRange GetMethodRange(uint typeRid)
        {
            EnsureTypeMemberListsInitialized();
            return typeRid - 1 < _methodLists.Length
                ? _methodLists[typeRid - 1]
                : MetadataRange.Empty;
        }

        internal uint GetFieldDeclaringType(uint fieldRid)
        {
            EnsureTypeMemberListsInitialized();
            return fieldRid - 1 < _fieldDeclaringTypes.Length
                ? _fieldDeclaringTypes[fieldRid - 1]
                : 0;
        }

        internal uint GetMethodDeclaringType(uint fieldRid)
        {
            EnsureTypeMemberListsInitialized();
            return fieldRid - 1 < _methodDeclaringTypes.Length
                ? _methodDeclaringTypes[fieldRid - 1]
                : 0;
        }

        private void EnsureParameterListsInitialized()
        {
            if (_paramLists is null)
                InitializeParameterLists();
        }

        private void InitializeParameterLists()
        {
            var tablesStream = _metadata.GetStream<TablesStream>();
            var parameterTable = tablesStream.GetTable(TableIndex.Param);
            var methodTable = tablesStream.GetTable(TableIndex.Method);

            var paramLists = new MetadataRange[methodTable.Count];
            var parameterMethods = new uint[parameterTable.Count];
            for (uint methodRid = 1; methodRid <= methodTable.Count; methodRid++)
            {
                InitializeMemberList(methodRid, tablesStream.GetParameterRange(methodRid), paramLists, parameterMethods);
            }

            Interlocked.CompareExchange(ref _paramLists, paramLists, null);
            Interlocked.CompareExchange(ref _parameterMethods, parameterMethods, null);
        }

        internal MetadataRange GetParameterRange(uint methodRid)
        {
            EnsureParameterListsInitialized();
            return methodRid - 1 < _paramLists.Length
                ? _paramLists[methodRid - 1]
                : MetadataRange.Empty;
        }

        internal uint GetParameterOwner(uint paramRid)
        {
            EnsureParameterListsInitialized();
            return paramRid - 1 < _parameterMethods.Length
                ? _parameterMethods[paramRid - 1]
                : 0;
        }

        private void EnsurePropertyListsInitialized()
        {
            if (_propertyLists is null)
                InitializePropertyLists();
        }

        private void InitializePropertyLists()
        {
            var tablesStream = _metadata.GetStream<TablesStream>();
            var propertyMapTable = tablesStream.GetTable<PropertyMapRow>(TableIndex.PropertyMap);
            var propertyTable = tablesStream.GetTable(TableIndex.Property);

            var propertyLists = new Dictionary<uint, MetadataRange>();
            var propertyDeclaringTypes  = new uint[propertyTable.Count];
            
            for (uint mapRid = 1; mapRid <= propertyMapTable.Count; mapRid++)
            {
                uint ownerRid = propertyMapTable[(int) (mapRid - 1)].Parent;
                InitializeMemberList(ownerRid, tablesStream.GetPropertyRange(mapRid), propertyLists, propertyDeclaringTypes);
            }
            
            Interlocked.CompareExchange(ref _propertyLists, propertyLists, null);
            Interlocked.CompareExchange(ref _propertyDeclaringTypes, propertyDeclaringTypes, null);
        }

        internal MetadataRange GetPropertyRange(uint typeRid)
        {
            EnsurePropertyListsInitialized();
            return _propertyLists.TryGetValue(typeRid, out var range)
                ? range
                : MetadataRange.Empty;
        }

        internal uint GetPropertyOwner(uint propertyRid)
        {
            EnsurePropertyListsInitialized();
            return propertyRid - 1 < _propertyDeclaringTypes.Length
                ? _propertyDeclaringTypes[propertyRid - 1]
                : 0;
        }

        private void EnsureEventListsInitialized()
        {
            if (_eventLists is null)
                InitializeEventLists();
        }

        private void InitializeEventLists()
        {
            var tablesStream = _metadata.GetStream<TablesStream>();
            var eventMapTable = tablesStream.GetTable<EventMapRow>(TableIndex.EventMap);
            var eventTable = tablesStream.GetTable(TableIndex.Event);

            var eventLists = new Dictionary<uint, MetadataRange>();
            var eventDeclaringTypes  = new uint[eventTable.Count];
            
            for (uint mapRid = 1; mapRid <= eventMapTable.Count; mapRid++)
            {
                uint ownerRid = eventMapTable[(int) (mapRid - 1)].Parent;
                InitializeMemberList(ownerRid, tablesStream.GetEventRange(mapRid), eventLists, eventDeclaringTypes);
            }
            
            Interlocked.CompareExchange(ref _eventLists, eventLists, null);
            Interlocked.CompareExchange(ref _eventDeclaringTypes, eventDeclaringTypes, null);
        }

        internal MetadataRange GetEventRange(uint typeRid)
        {
            EnsureEventListsInitialized();
            return _eventLists.TryGetValue(typeRid, out var range)
                ? range
                : MetadataRange.Empty;
        }

        internal uint GetEventOwner(uint eventRid)
        {
            EnsureEventListsInitialized();
            return eventRid - 1 < _eventDeclaringTypes.Length
                ? _eventDeclaringTypes[eventRid - 1]
                : 0;
        }

        private static void InitializeMemberList(uint ownerRid, MetadataRange memberRange,
            MetadataRange[] memberLists, uint[] memberDeclaringTypes)
        {
            memberLists[ownerRid - 1] = memberRange;
            foreach (var token in memberRange)
                memberDeclaringTypes[token.Rid - 1] = ownerRid;
        }

        private static void InitializeMemberList(uint ownerRid, MetadataRange memberRange,
            IDictionary<uint, MetadataRange> memberLists, uint[] memberDeclaringTypes)
        {
            memberLists[ownerRid] = memberRange;
            foreach (var token in memberRange)
                memberDeclaringTypes[token.Rid - 1] = ownerRid;
        }

        /// <inheritdoc />
        protected override IList<AssemblyReference> GetAssemblyReferences()
        {
            var result = new OwnedCollection<ModuleDefinition, AssemblyReference>(this);

            var table = _metadata.GetStream<TablesStream>().GetTable<AssemblyReferenceRow>();
            for (int i = 0; i < table.Count; i++)
            {
                var token = new MetadataToken(TableIndex.AssemblyRef, (uint) i + 1);
                result.Add(new SerializedAssemblyReference(_metadata, token, table[i]));
            }

            return result;
        }

        private IResolutionScope FindMostRecentCorLib()
        {
            // TODO: perhaps check public key tokens.
            
            IResolutionScope mostRecentCorLib = null;
            var mostRecentVersion = new Version();
            foreach (var reference in AssemblyReferences)
            {
                if (CorLibTypeFactory.KnownCorLibNames.Contains(reference.Name))
                {
                    if (mostRecentVersion < reference.Version)
                        mostRecentCorLib = reference;
                }
            }

            if (mostRecentCorLib is null)
            {
                if (CorLibTypeFactory.KnownCorLibNames.Contains(Assembly.Name))
                    mostRecentCorLib = this;
            }

            return mostRecentCorLib;
        }
    }
}