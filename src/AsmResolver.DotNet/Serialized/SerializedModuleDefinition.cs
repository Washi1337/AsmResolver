using System;
using System.Collections.Generic;
using AsmResolver.DotNet.Signatures;
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
        private readonly ModuleReadParameters _readParameters;
        private readonly CachedSerializedMemberFactory _memberFactory;

        private readonly LazyRidListRelation<TypeDefinitionRow> _fieldLists;
        private readonly LazyRidListRelation<TypeDefinitionRow> _methodLists;
        private readonly LazyRidListRelation<MethodDefinitionRow> _paramLists;
        private readonly LazyRidListRelation<PropertyMapRow> _propertyLists;
        private readonly LazyRidListRelation<EventMapRow> _eventLists;

        private OneToManyRelation<uint, uint> _typeDefTree;
        private OneToManyRelation<MetadataToken, uint> _semantics;
        private OneToManyRelation<MetadataToken, uint> _customAttributes;
        private OneToManyRelation<MetadataToken, uint> _genericParameters;

        /// <summary>
        /// Creates a module definition from a module metadata row.
        /// </summary>
        /// <param name="metadata">The object providing access to the underlying metadata streams.</param>
        /// <param name="token">The token to initialize the module for.</param>
        /// <param name="row">The metadata table row to base the module definition on.</param>
        /// <param name="readParameters">The parameters to use while reading the module.</param>
        public SerializedModuleDefinition(IMetadata metadata, MetadataToken token, ModuleDefinitionRow row,
            ModuleReadParameters readParameters)
            : base(token)
        {
            _metadata = metadata;
            _row = row;
            _readParameters = readParameters;
            Generation = row.Generation;
            MetadataToken = token;

            _memberFactory = new CachedSerializedMemberFactory(metadata, this);
            
            var assemblyTable = _metadata
                .GetStream<TablesStream>()
                .GetTable<AssemblyDefinitionRow>();
            
            if (assemblyTable.Count > 0)
            {
                var assembly = new SerializedAssemblyDefinition(metadata,
                    new MetadataToken(TableIndex.Assembly, 1),
                    assemblyTable[0],
                    this,
                    readParameters);
                Assembly = assembly;
            }
            
            CorLibTypeFactory = new CorLibTypeFactory(FindMostRecentCorLib());

            var tablesStream = metadata.GetStream<TablesStream>();

            _fieldLists = new LazyRidListRelation<TypeDefinitionRow>(metadata, TableIndex.TypeDef,
                (rid, _) => rid, tablesStream.GetFieldRange);
            _methodLists = new LazyRidListRelation<TypeDefinitionRow>(metadata, TableIndex.TypeDef, 
                (rid, _) => rid, tablesStream.GetMethodRange);
            _paramLists = new LazyRidListRelation<MethodDefinitionRow>(metadata, TableIndex.Method, 
                (rid, _) => rid, tablesStream.GetParameterRange);
            _propertyLists = new LazyRidListRelation<PropertyMapRow>(metadata, TableIndex.PropertyMap, 
                (_, map) => map.Parent, tablesStream.GetPropertyRange);
            _eventLists = new LazyRidListRelation<EventMapRow>(metadata, TableIndex.EventMap,
                (_, map) => map.Parent, tablesStream.GetEventRange);
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

            var typeDefTable = _metadata
                .GetStream<TablesStream>()
                .GetTable<TypeDefinitionRow>(TableIndex.TypeDef);
            
            for (int i = 0; i < typeDefTable.Count; i++)
            {
                uint rid = (uint) i + 1;
                if (_typeDefTree.GetMemberOwner(rid) == 0)
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
            
            _typeDefTree = new OneToManyRelation<uint, uint>();
            foreach (var nestedClass in nestedClassTable)
                _typeDefTree.Add(nestedClass.EnclosingClass, nestedClass.NestedClass);
        }

        internal IEnumerable<uint> GetNestedTypeRids(uint enclosingTypeRid)
        {
            EnsureTypeDefinitionTreeInitialized();
            return _typeDefTree.GetMemberList(enclosingTypeRid);
        }

        internal uint GetParentTypeRid(uint nestedTypeRid)
        {
            EnsureTypeDefinitionTreeInitialized();
            return _typeDefTree.GetMemberOwner(nestedTypeRid);
        }

        internal MetadataRange GetFieldRange(uint typeRid) => _fieldLists.GetMemberRange(typeRid);
        internal uint GetFieldDeclaringType(uint fieldRid) => _fieldLists.GetMemberOwner(fieldRid);
        internal MetadataRange GetMethodRange(uint typeRid) => _methodLists.GetMemberRange(typeRid);
        internal uint GetMethodDeclaringType(uint methodRid) => _methodLists.GetMemberOwner(methodRid);
        internal MetadataRange GetParameterRange(uint methodRid) => _paramLists.GetMemberRange(methodRid);
        internal uint GetParameterOwner(uint paramRid) => _paramLists.GetMemberOwner(paramRid);
        internal MetadataRange GetPropertyRange(uint typeRid) => _propertyLists.GetMemberRange(typeRid);
        internal uint GetPropertyDeclaringType(uint propertyRid) => _propertyLists.GetMemberOwner(propertyRid);
        internal MetadataRange GetEventRange(uint typeRid) => _eventLists.GetMemberRange(typeRid);
        internal uint GetEventDeclaringType(uint eventRid) => _eventLists.GetMemberOwner(eventRid);

        private void EnsureMethodSemanticsInitialized()
        {
            if (_semantics is null)
                InitializeMethodSemantics();
        }

        private void InitializeMethodSemantics()
        {
            var tablesStream = _metadata.GetStream<TablesStream>();
            var semanticsTable = tablesStream.GetTable<MethodSemanticsRow>(TableIndex.MethodSemantics);
            var encoder = tablesStream.GetIndexEncoder(CodedIndex.HasSemantics);
            
            _semantics = new OneToManyRelation<MetadataToken, uint>();
            for (int i = 0; i < semanticsTable.Count; i++)
            {
                var ownerToken = encoder.DecodeIndex(semanticsTable[i].Association);
                uint semanticsRid = (uint) (i + 1);
                _semantics.Add(ownerToken, semanticsRid);
            }
        }

        internal IEnumerable<uint> GetMethodSemantics(MetadataToken owner)
        {
            EnsureMethodSemanticsInitialized();
            return _semantics.GetMemberList(owner);
        }

        internal MetadataToken GetMethodSemanticsOwner(uint methodRid)
        {
            EnsureMethodSemanticsInitialized();
            return _semantics.GetMemberOwner(methodRid);
        }

        private void EnsureCustomAttributesInitialized()
        {
            if (_customAttributes is null)
                InitializeCustomAttributes();
        }

        private void InitializeCustomAttributes()
        {
            var tablesStream = _metadata.GetStream<TablesStream>();
            var attributeTable = tablesStream.GetTable<CustomAttributeRow>(TableIndex.CustomAttribute);
            var encoder = tablesStream.GetIndexEncoder(CodedIndex.HasCustomAttribute);
            
            _customAttributes = new OneToManyRelation<MetadataToken, uint>();
            for (int i = 0; i < attributeTable.Count; i++)
            {
                var ownerToken = encoder.DecodeIndex(attributeTable[i].Parent);
                uint attributeRid = (uint) (i + 1);
                _customAttributes.Add(ownerToken, attributeRid);
            }
        }

        internal IEnumerable<uint> GetCustomAttributes(MetadataToken owner)
        {
            EnsureCustomAttributesInitialized();
            return _customAttributes.GetMemberList(owner);
        }
        
        /// <inheritdoc />
        protected override IList<CustomAttribute> GetCustomAttributes() => GetCustomAttributeCollection(this);

        internal MetadataToken GetCustomAttributeOwner(uint attributeRid)
        {
            EnsureCustomAttributesInitialized();
            return _customAttributes.GetMemberOwner(attributeRid);
        }

        internal IList<CustomAttribute> GetCustomAttributeCollection(IHasCustomAttribute owner)
        {
            var result = new OwnedCollection<IHasCustomAttribute, CustomAttribute>(owner);

            foreach (uint rid in GetCustomAttributes(owner.MetadataToken))
            {
                var attribute = (CustomAttribute) LookupMember(new MetadataToken(TableIndex.CustomAttribute, rid));
                result.Add(attribute);
            }
            
            return result;
        }

        private void EnsureGenericParametersInitialized()
        {
            if (_genericParameters is null)
                InitializeGenericParameters();
        }

        private void InitializeGenericParameters()
        {
            var tablesStream = _metadata.GetStream<TablesStream>();
            var parameterTable = tablesStream.GetTable<GenericParameterRow>(TableIndex.GenericParam);
            var encoder = tablesStream.GetIndexEncoder(CodedIndex.TypeOrMethodDef);
            
            _genericParameters = new OneToManyRelation<MetadataToken, uint>();
            for (int i = 0; i < parameterTable.Count; i++)
            {
                var ownerToken = encoder.DecodeIndex(parameterTable[i].Owner);
                uint parameterRid = (uint) (i + 1);
                _genericParameters.Add(ownerToken, parameterRid);
            }
        }

        internal MetadataToken GetGenericParameterOwner(uint parameterRid)
        {
            EnsureGenericParametersInitialized();
            return _genericParameters.GetMemberOwner(parameterRid);
        }
        
        internal ICollection<uint> GetGenericParameters(MetadataToken ownerToken)
        {
            EnsureGenericParametersInitialized();
            return _genericParameters.GetMemberList(ownerToken);
        }

        /// <inheritdoc />
        protected override IList<AssemblyReference> GetAssemblyReferences()
        {
            var result = new OwnedCollection<ModuleDefinition, AssemblyReference>(this);

            var table = _metadata.GetStream<TablesStream>().GetTable<AssemblyReferenceRow>();
            for (int i = 0; i < table.Count; i++)
            {
                var token = new MetadataToken(TableIndex.AssemblyRef, (uint) i + 1);
                result.Add(new SerializedAssemblyReference(_metadata, this, token, table[i]));
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