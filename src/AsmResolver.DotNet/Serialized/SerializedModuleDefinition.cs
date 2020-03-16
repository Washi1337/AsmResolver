using System;
using System.Collections.Generic;
using System.Threading;
using AsmResolver.DotNet.Signatures;
using AsmResolver.DotNet.Collections;
using AsmResolver.PE.DotNet;
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
        private readonly ModuleDefinitionRow _row;

        private readonly CachedSerializedMemberFactory _memberFactory;
        private readonly LazyRidListRelation<TypeDefinitionRow> _fieldLists;
        private readonly LazyRidListRelation<TypeDefinitionRow> _methodLists;
        private readonly LazyRidListRelation<MethodDefinitionRow> _paramLists;
        private readonly LazyRidListRelation<PropertyMapRow> _propertyLists;
        private readonly LazyRidListRelation<EventMapRow> _eventLists;

        private OneToManyRelation<uint, uint> _typeDefTree;
        private OneToManyRelation<MetadataToken, uint> _semantics;
        private OneToOneRelation<MetadataToken, uint> _constants;
        private OneToManyRelation<MetadataToken, uint> _customAttributes;
        private OneToManyRelation<MetadataToken, uint> _genericParameters;
        private OneToManyRelation<MetadataToken, uint> _interfaces;
        private OneToOneRelation<MetadataToken, uint> _classLayouts;
        private OneToOneRelation<MetadataToken, uint> _implementationMaps;

        /// <summary>
        /// Creates a module definition from a module metadata row.
        /// </summary>
        /// <param name="dotNetDirectory">The object providing access to the underlying .NET data directory.</param>
        /// <param name="token">The token to initialize the module for.</param>
        /// <param name="row">The metadata table row to base the module definition on.</param>
        /// <param name="readParameters">The parameters to use while reading the module.</param>
        public SerializedModuleDefinition(IDotNetDirectory dotNetDirectory, MetadataToken token, ModuleDefinitionRow row,
            ModuleReadParameters readParameters)
            : base(token)
        {
            // Store parameters in fields.
            DotNetDirectory = dotNetDirectory;
            _row = row;
            ReadParameters = readParameters ?? throw new ArgumentNullException(nameof(readParameters));
            
            // Copy over "simple" columns.
            Generation = row.Generation;
            MetadataToken = token;
            Attributes = DotNetDirectory.Flags;
            
            // Initialize member factory.
            var metadata = dotNetDirectory.Metadata;
            _memberFactory = new CachedSerializedMemberFactory(metadata, this);
            
            // Find assembly definitino and corlib assembly.
            Assembly = FindParentAssembly();
            var corLib = FindMostRecentCorLib();
            if (corLib is {})
            {
                CorLibTypeFactory = new CorLibTypeFactory(corLib);
            }
            else
            {
                CorLibTypeFactory = CorLibTypeFactory.CreateMscorlib40TypeFactory();
                corLib = CorLibTypeFactory.CorLibScope;
            }

            MetadataResolver = new DefaultMetadataResolver(CreateAssemblyResolver(corLib));

            // Prepare lazy RID lists.
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

        /// <summary>
        /// Gets the underlying object providing access to the data directory containing .NET metadata.  
        /// </summary>
        public IDotNetDirectory DotNetDirectory
        {
            get;
        }

        /// <summary>
        /// Gets the reading parameters that are used for reading the contents of the module.
        /// </summary>
        public ModuleReadParameters ReadParameters
        {
            get;
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
            value = DotNetDirectory.Metadata.GetStream<UserStringsStream>().GetStringByIndex(token.Rid);
            return value == null;
        }

        /// <inheritdoc />
        public override IndexEncoder GetIndexEncoder(CodedIndex codedIndex) =>
            DotNetDirectory.Metadata.GetStream<TablesStream>().GetIndexEncoder(codedIndex);

        /// <inheritdoc />
        protected override string GetName() 
            => DotNetDirectory.Metadata.GetStream<StringsStream>()?.GetStringByIndex(_row.Name);

        /// <inheritdoc />
        protected override Guid GetMvid() 
            => DotNetDirectory.Metadata.GetStream<GuidStream>()?.GetGuidByIndex(_row.Mvid) ?? Guid.Empty;

        /// <inheritdoc />
        protected override Guid GetEncId()
            => DotNetDirectory.Metadata.GetStream<GuidStream>()?.GetGuidByIndex(_row.EncId) ?? Guid.Empty;

        /// <inheritdoc />
        protected override Guid GetEncBaseId()
            => DotNetDirectory.Metadata.GetStream<GuidStream>()?.GetGuidByIndex(_row.EncBaseId) ?? Guid.Empty;

        /// <inheritdoc />
        protected override IList<TypeDefinition> GetTopLevelTypes()
        {
            EnsureTypeDefinitionTreeInitialized();
            
            var types = new OwnedCollection<ModuleDefinition, TypeDefinition>(this);

            var typeDefTable = DotNetDirectory
                .Metadata
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
                Interlocked.CompareExchange(ref _typeDefTree, InitializeTypeDefinitionTree(), null);
        }

        private OneToManyRelation<uint, uint> InitializeTypeDefinitionTree()
        {
            var tablesStream = DotNetDirectory.Metadata.GetStream<TablesStream>();
            var nestedClassTable = tablesStream.GetTable<NestedClassRow>(TableIndex.NestedClass);
            
            var typeDefTree = new OneToManyRelation<uint, uint>();
            foreach (var nestedClass in nestedClassTable)
                typeDefTree.Add(nestedClass.EnclosingClass, nestedClass.NestedClass);

            return typeDefTree;
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
                Interlocked.CompareExchange(ref _semantics, InitializeMethodSemantics(), null);
        }

        private OneToManyRelation<MetadataToken, uint> InitializeMethodSemantics()
        {
            var tablesStream = DotNetDirectory.Metadata.GetStream<TablesStream>();
            var semanticsTable = tablesStream.GetTable<MethodSemanticsRow>(TableIndex.MethodSemantics);
            var encoder = tablesStream.GetIndexEncoder(CodedIndex.HasSemantics);
            
            var semantics = new OneToManyRelation<MetadataToken, uint>();
            for (int i = 0; i < semanticsTable.Count; i++)
            {
                var ownerToken = encoder.DecodeIndex(semanticsTable[i].Association);
                uint semanticsRid = (uint) (i + 1);
                semantics.Add(ownerToken, semanticsRid);
            }

            return semantics;
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

        private void EnsureConstantsInitialized()
        {
            if (_constants is null)
                Interlocked.CompareExchange(ref _constants, GetConstants(), null);
        }

        private OneToOneRelation<MetadataToken, uint> GetConstants()
        {
            var tablesStream = DotNetDirectory.Metadata.GetStream<TablesStream>();
            var constantTable = tablesStream.GetTable<ConstantRow>(TableIndex.Constant);
            var encoder = tablesStream.GetIndexEncoder(CodedIndex.HasConstant);
            
            var constants = new OneToOneRelation<MetadataToken, uint>();
            for (int i = 0; i < constantTable.Count; i++)
            {
                var ownerToken = encoder.DecodeIndex(constantTable[i].Parent);
                uint constantRid = (uint) (i + 1);
                constants.Add(ownerToken, constantRid);
            }

            return constants;
        }

        internal uint GetConstantRid(MetadataToken ownerToken)
        {
            EnsureConstantsInitialized();
            return _constants.GetValue(ownerToken);
        }

        internal Constant GetConstant(MetadataToken ownerToken)
        {
            uint constantRid = GetConstantRid(ownerToken);
            return TryLookupMember(new MetadataToken(TableIndex.Constant, constantRid), out var member)
                ? member as Constant
                : null;
        }

        internal MetadataToken GetConstantOwner(uint constantRid)
        {
            EnsureConstantsInitialized();
            return _constants.GetKey(constantRid);
        }

        private void EnsureCustomAttributesInitialized()
        {
            if (_customAttributes is null)
                Interlocked.CompareExchange(ref _customAttributes, InitializeCustomAttributes(), null);
        }

        private OneToManyRelation<MetadataToken, uint> InitializeCustomAttributes()
        {
            var tablesStream = DotNetDirectory.Metadata.GetStream<TablesStream>();
            var attributeTable = tablesStream.GetTable<CustomAttributeRow>(TableIndex.CustomAttribute);
            var encoder = tablesStream.GetIndexEncoder(CodedIndex.HasCustomAttribute);
            
            var customAttributes = new OneToManyRelation<MetadataToken, uint>();
            for (int i = 0; i < attributeTable.Count; i++)
            {
                var ownerToken = encoder.DecodeIndex(attributeTable[i].Parent);
                uint attributeRid = (uint) (i + 1);
                customAttributes.Add(ownerToken, attributeRid);
            }

            return customAttributes;
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
                Interlocked.CompareExchange(ref _genericParameters, InitializeGenericParameters(), null);
        }

        private OneToManyRelation<MetadataToken, uint> InitializeGenericParameters()
        {
            var tablesStream = DotNetDirectory.Metadata.GetStream<TablesStream>();
            var parameterTable = tablesStream.GetTable<GenericParameterRow>(TableIndex.GenericParam);
            var encoder = tablesStream.GetIndexEncoder(CodedIndex.TypeOrMethodDef);
            
            var genericParameters = new OneToManyRelation<MetadataToken, uint>();
            for (int i = 0; i < parameterTable.Count; i++)
            {
                var ownerToken = encoder.DecodeIndex(parameterTable[i].Owner);
                uint parameterRid = (uint) (i + 1);
                genericParameters.Add(ownerToken, parameterRid);
            }

            return genericParameters;
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

        private void EnsureInterfacesInitialized()
        {
            if (_interfaces is null)
                Interlocked.CompareExchange(ref _interfaces, InitializeInterfaces(), null);
        }

        private OneToManyRelation<MetadataToken, uint> InitializeInterfaces()
        {
            var tablesStream = DotNetDirectory.Metadata.GetStream<TablesStream>();
            var interfaceImplTable = tablesStream.GetTable<InterfaceImplementationRow>(TableIndex.InterfaceImpl);
            
            var interfaces = new OneToManyRelation<MetadataToken, uint>();
            for (int i = 0; i < interfaceImplTable.Count; i++)
            {
                var ownerToken = new MetadataToken(TableIndex.TypeDef, interfaceImplTable[i].Class);
                uint interfaceImplRid = (uint) (i + 1);
                interfaces.Add(ownerToken, interfaceImplRid);
            }

            return interfaces;
        }

        internal ICollection<uint> GetInterfaceImplementationRids(MetadataToken ownerToken)
        {
            EnsureInterfacesInitialized();
            return _interfaces.GetMemberList(ownerToken);
        }

        private void EnsureClassLayoutsInitialized()
        {
            if (_classLayouts is null)
                Interlocked.CompareExchange(ref _classLayouts, InitializeClassLayouts(), null);
        }

        private OneToOneRelation<MetadataToken, uint> InitializeClassLayouts()
        {
            var tablesStream = DotNetDirectory.Metadata.GetStream<TablesStream>();
            var layoutTable = tablesStream.GetTable<ClassLayoutRow>(TableIndex.ClassLayout);
            
            var layouts = new OneToOneRelation<MetadataToken, uint>();
            for (int i = 0; i < layoutTable.Count; i++)
            {
                var ownerToken = new MetadataToken(TableIndex.TypeDef, layoutTable[i].Parent);
                uint layoutRid = (uint) (i + 1);
                layouts.Add(ownerToken, layoutRid);
            }

            return layouts;
        }

        internal uint GetClassLayoutRid(MetadataToken ownerToken)
        {
            EnsureClassLayoutsInitialized();
            return _classLayouts.GetValue(ownerToken);
        }

        private void EnsureImplementationMapsInitialized()
        {
            if (_implementationMaps is null)
                Interlocked.CompareExchange(ref _implementationMaps, InitializeImplementationMaps(), null);
        }

        private OneToOneRelation<MetadataToken, uint> InitializeImplementationMaps()
        {
            var tablesStream = DotNetDirectory.Metadata.GetStream<TablesStream>();
            var mapTable = tablesStream.GetTable<ImplementationMapRow>(TableIndex.ImplMap);
            var encoder = tablesStream.GetIndexEncoder(CodedIndex.TypeOrMethodDef);
            
            var maps = new OneToOneRelation<MetadataToken, uint>();
            for (int i = 0; i < mapTable.Count; i++)
            {
                var ownerToken = encoder.DecodeIndex(mapTable[i].MemberForwarded);
                uint mapRid = (uint) (i + 1);
                maps.Add(ownerToken, mapRid);
            }

            return maps;
        }

        internal uint GetImplementationMapRid(MetadataToken ownerToken)
        {
            EnsureImplementationMapsInitialized();
            return _implementationMaps.GetValue(ownerToken);
        }

        internal MetadataToken GetImplementationMapOwner(uint mapRid)
        {
            EnsureImplementationMapsInitialized();
            return _implementationMaps.GetKey(mapRid);
        }

        /// <inheritdoc />
        protected override IList<AssemblyReference> GetAssemblyReferences()
        {
            var result = new OwnedCollection<ModuleDefinition, AssemblyReference>(this);

            var table = DotNetDirectory.Metadata
                .GetStream<TablesStream>()
                .GetTable<AssemblyReferenceRow>(TableIndex.AssemblyRef);
            
            // Don't use the member factory here, this method may be called before the member factory is initialized.
            for (int i = 0; i < table.Count; i++)
            {
                var token = new MetadataToken(TableIndex.AssemblyRef, (uint) i + 1);
                result.Add(new SerializedAssemblyReference(this, token, table[i]));
            }
            
            return result;
        }

        /// <inheritdoc />
        protected override IList<ModuleReference> GetModuleReferences()
        {
            var result = new OwnedCollection<ModuleDefinition, ModuleReference>(this);

            var table = DotNetDirectory.Metadata
                .GetStream<TablesStream>()
                .GetTable(TableIndex.ModuleRef);
            
            for (int i = 0; i < table.Count; i++)
            {
                var token = new MetadataToken(TableIndex.ModuleRef, (uint) i + 1);
                if (_memberFactory.TryLookupMember(token, out var member) && member is ModuleReference module)
                    result.Add(module);
            }
            
            return result;
        }

        /// <inheritdoc />
        protected override IList<FileReference> GetFileReferences()
        {
            var result = new OwnedCollection<ModuleDefinition, FileReference>(this);

            var table = DotNetDirectory.Metadata
                .GetStream<TablesStream>()
                .GetTable(TableIndex.File);
            
            for (int i = 0; i < table.Count; i++)
            {
                var token = new MetadataToken(TableIndex.File, (uint) i + 1);
                if (_memberFactory.TryLookupMember(token, out var member) && member is FileReference file)
                    result.Add(file);
            }
            
            return result;
        }

        /// <inheritdoc />
        protected override IList<ManifestResource> GetResources()
        {
            var result = new OwnedCollection<ModuleDefinition, ManifestResource>(this);

            var table = DotNetDirectory.Metadata
                .GetStream<TablesStream>()
                .GetTable(TableIndex.ManifestResource);
            
            for (int i = 0; i < table.Count; i++)
            {
                var token = new MetadataToken(TableIndex.ManifestResource, (uint) i + 1);
                if (_memberFactory.TryLookupMember(token, out var member) && member is ManifestResource resource)
                    result.Add(resource);
            }
            
            return result;
        }

        /// <inheritdoc />
        protected override IList<ExportedType> GetExportedTypes()
        {
            var result = new OwnedCollection<ModuleDefinition, ExportedType>(this);

            var table = DotNetDirectory.Metadata
                .GetStream<TablesStream>()
                .GetTable(TableIndex.ExportedType);
            
            for (int i = 0; i < table.Count; i++)
            {
                var token = new MetadataToken(TableIndex.ExportedType, (uint) i + 1);
                if (_memberFactory.TryLookupMember(token, out var member) && member is ExportedType exportedType)
                    result.Add(exportedType);
            }
            
            return result;
        }

        /// <inheritdoc />
        protected override IManagedEntrypoint GetManagedEntrypoint()
        {
            if ((DotNetDirectory.Flags & DotNetDirectoryFlags.ILLibrary) == 0)
            {
                if ((DotNetDirectory.Flags & DotNetDirectoryFlags.NativeEntrypoint) != 0)
                {
                    // TODO: native entrypoints.
                    return null;
                }

                if (DotNetDirectory.Entrypoint != 0)
                    return LookupMember(DotNetDirectory.Entrypoint) as IManagedEntrypoint;
            }

            return null;
        }

        private AssemblyDefinition FindParentAssembly()
        {
            var assemblyTable = DotNetDirectory.Metadata
                .GetStream<TablesStream>()
                .GetTable<AssemblyDefinitionRow>();

            if (assemblyTable.Count > 0)
            {
                var assembly = new SerializedAssemblyDefinition(DotNetDirectory,
                    new MetadataToken(TableIndex.Assembly, 1),
                    assemblyTable[0],
                    this,
                    ReadParameters);
                return assembly;
            }

            return null;
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

            if (mostRecentCorLib is null && Assembly is {})
            {
                if (CorLibTypeFactory.KnownCorLibNames.Contains(Assembly.Name))
                    mostRecentCorLib = this;
            }

            return mostRecentCorLib;
        }
    }
}