using System;
using System.Collections.Generic;
using System.Threading;
using AsmResolver.Collections;
using AsmResolver.DotNet.Collections;
using AsmResolver.DotNet.Signatures.Marshal;
using AsmResolver.DotNet.Signatures.Types;
using AsmResolver.PE;
using AsmResolver.PE.Debug;
using AsmResolver.PE.DotNet;
using AsmResolver.PE.DotNet.Metadata.Blob;
using AsmResolver.PE.DotNet.Metadata.Guid;
using AsmResolver.PE.DotNet.Metadata.Strings;
using AsmResolver.PE.DotNet.Metadata.Tables;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;
using AsmResolver.PE.DotNet.Metadata.UserStrings;
using AsmResolver.PE.Win32Resources;

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
        private Dictionary<uint, MetadataToken> _semanticMethods;
        private OneToOneRelation<MetadataToken, uint> _constants;
        private OneToManyRelation<MetadataToken, uint> _customAttributes;
        private OneToManyRelation<MetadataToken, uint> _securityDeclarations;
        private OneToManyRelation<MetadataToken, uint> _genericParameters;
        private OneToManyRelation<MetadataToken, uint> _genericParameterConstraints;
        private OneToManyRelation<MetadataToken, uint> _interfaces;
        private OneToManyRelation<MetadataToken, uint> _methodImplementations;
        private OneToOneRelation<MetadataToken, uint> _classLayouts;
        private OneToOneRelation<MetadataToken, uint> _implementationMaps;
        private OneToOneRelation<MetadataToken, uint> _fieldRvas;
        private OneToOneRelation<MetadataToken, uint> _fieldMarshals;
        private OneToOneRelation<MetadataToken, uint> _fieldLayouts;

        /// <summary>
        /// Interprets a PE image as a .NET module.
        /// </summary>
        /// <param name="peImage">The image to interpret as a .NET module.</param>
        /// <param name="readerParameters">The parameters to use while reading the module.</param>
        public SerializedModuleDefinition(IPEImage peImage, ModuleReaderParameters readerParameters)
            : base(new MetadataToken(TableIndex.Module, 1))
        {
            if (peImage is null)
                throw new ArgumentNullException(nameof(peImage));
            if (readerParameters is null)
                throw new ArgumentNullException(nameof(readerParameters));
            
            var metadata = peImage.DotNetDirectory?.Metadata;
            if (metadata is null)
                throw new BadImageFormatException("Input PE image does not contain a .NET metadata directory.");

            var tablesStream = metadata.GetStream<TablesStream>();
            if (tablesStream is null)
                throw new BadImageFormatException(".NET metadata directory does not define a tables stream.");

            var moduleTable = tablesStream.GetTable<ModuleDefinitionRow>(TableIndex.Module);
            if (!moduleTable.TryGetByRid(1, out _row))
                throw new BadImageFormatException("Module definition table does not contain any rows.");

            // Store parameters in fields.
            ReaderContext = new ModuleReaderContext(peImage, this, readerParameters);

            // Copy over PE header fields.
            FilePath = peImage.FilePath;
            MachineType = peImage.MachineType;
            FileCharacteristics = peImage.Characteristics;
            PEKind = peImage.PEKind;
            SubSystem = peImage.SubSystem;
            DllCharacteristics = peImage.DllCharacteristics;
            TimeDateStamp = peImage.TimeDateStamp;
            
            // Copy over "simple" columns.
            Generation = _row.Generation;
            Attributes = peImage.DotNetDirectory.Flags;
            
            // Initialize member factory.
            _memberFactory = new CachedSerializedMemberFactory(ReaderContext);
            
            // Find assembly definition and corlib assembly.
            Assembly = FindParentAssembly();
            var corLib = FindMostRecentCorLib();
            if (corLib is {})
            {
                CorLibTypeFactory = new CorLibTypeFactory(corLib);
            }
            else
            {
                CorLibTypeFactory = CorLibTypeFactory.CreateMscorlib40TypeFactory(this);
                corLib = CorLibTypeFactory.CorLibScope;
            }

            var assemblyResolver = CreateAssemblyResolver(corLib, readerParameters.WorkingDirectory);
            MetadataResolver = new DefaultMetadataResolver(assemblyResolver);

            // Prepare lazy RID lists.
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
        public override IDotNetDirectory DotNetDirectory => ReaderContext.Image.DotNetDirectory;

        /// <summary>
        /// Gets the reading context that is used for reading the contents of the module.
        /// </summary>
        public ModuleReaderContext ReaderContext
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
        public override IEnumerable<TypeReference> GetImportedTypeReferences()
        {
            var table = DotNetDirectory.Metadata
                .GetStream<TablesStream>()
                .GetTable(TableIndex.TypeRef);

            for (uint rid = 1; rid <= table.Count; rid++)
            {
                if (TryLookupMember(new MetadataToken(TableIndex.TypeRef, rid), out var member)
                    && member is TypeReference reference)
                {
                    yield return reference;
                }
            }
        }

        /// <inheritdoc />
        public override IEnumerable<MemberReference> GetImportedMemberReferences()
        {
            var table = DotNetDirectory.Metadata
                .GetStream<TablesStream>()
                .GetTable(TableIndex.MemberRef);

            for (uint rid = 1; rid <= table.Count; rid++)
            {
                if (TryLookupMember(new MetadataToken(TableIndex.MemberRef, rid), out var member)
                    && member is MemberReference reference)
                {
                    yield return reference;
                }
            }
        }

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
                if (_typeDefTree.GetKey(rid) == 0)
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
            return _typeDefTree.GetValues(enclosingTypeRid);
        }

        internal uint GetParentTypeRid(uint nestedTypeRid)
        {
            EnsureTypeDefinitionTreeInitialized();
            return _typeDefTree.GetKey(nestedTypeRid);
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
            var tablesStream = DotNetDirectory.Metadata.GetStream<TablesStream>();
            var semanticsTable = tablesStream.GetTable<MethodSemanticsRow>(TableIndex.MethodSemantics);
            var encoder = tablesStream.GetIndexEncoder(CodedIndex.HasSemantics);
            
            var semantics = new OneToManyRelation<MetadataToken, uint>();
            var semanticMethods = new Dictionary<uint, MetadataToken>();
            for (int i = 0; i < semanticsTable.Count; i++)
            {
                var methodSemanticsRow = semanticsTable[i];
                var semanticsToken = new MetadataToken(TableIndex.MethodSemantics, (uint) (i + 1));
                
                var ownerToken = encoder.DecodeIndex(methodSemanticsRow.Association);
                semantics.Add(ownerToken, semanticsToken.Rid);
                semanticMethods.Add(methodSemanticsRow.Method, semanticsToken);
            }

            Interlocked.CompareExchange(ref _semantics, semantics, null);
            Interlocked.CompareExchange(ref _semanticMethods, semanticMethods, null);
        }

        internal IEnumerable<uint> GetMethodSemantics(MetadataToken owner)
        {
            EnsureMethodSemanticsInitialized();
            return _semantics.GetValues(owner);
        }

        internal MetadataToken GetMethodSemanticsOwner(uint semanticsRid)
        {
            EnsureMethodSemanticsInitialized();
            return _semantics.GetKey(semanticsRid);
        }

        internal MetadataToken GetMethodParentSemantics(uint methodRid)
        {
            EnsureMethodSemanticsInitialized();
            _semanticMethods.TryGetValue(methodRid, out var token);
            return token;
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

        /// <inheritdoc />
        protected override IList<CustomAttribute> GetCustomAttributes() => GetCustomAttributeCollection(this);

        internal MetadataToken GetCustomAttributeOwner(uint attributeRid)
        {
            EnsureCustomAttributesInitialized();
            return _customAttributes.GetKey(attributeRid);
        }

        internal IList<CustomAttribute> GetCustomAttributeCollection(IHasCustomAttribute owner)
        {
            EnsureCustomAttributesInitialized();
            var result = new OwnedCollection<IHasCustomAttribute, CustomAttribute>(owner);
            
            foreach (uint rid in _customAttributes.GetValues(owner.MetadataToken))
            {
                var attribute = (CustomAttribute) LookupMember(new MetadataToken(TableIndex.CustomAttribute, rid));
                result.Add(attribute);
            }
            
            return result;
        }
        
        private void EnsureSecurityDeclarationsInitialized()
        {
            if (_securityDeclarations is null)
                Interlocked.CompareExchange(ref _securityDeclarations, InitializeSecurityDeclarations(), null);
        }

        private OneToManyRelation<MetadataToken, uint> InitializeSecurityDeclarations()
        {
            var tablesStream = DotNetDirectory.Metadata.GetStream<TablesStream>();
            var declarationTable = tablesStream.GetTable<SecurityDeclarationRow>(TableIndex.DeclSecurity);
            var encoder = tablesStream.GetIndexEncoder(CodedIndex.HasDeclSecurity);
            
            var securityDeclarations = new OneToManyRelation<MetadataToken, uint>();
            for (int i = 0; i < declarationTable.Count; i++)
            {
                var ownerToken = encoder.DecodeIndex(declarationTable[i].Parent);
                uint attributeRid = (uint) (i + 1);
                securityDeclarations.Add(ownerToken, attributeRid);
            }

            return securityDeclarations;
        }

        internal MetadataToken GetSecurityDeclarationOwner(uint attributeRid)
        {
            EnsureCustomAttributesInitialized();
            return _securityDeclarations.GetKey(attributeRid);
        }

        internal IList<SecurityDeclaration> GetSecurityDeclarationCollection(IHasSecurityDeclaration owner)
        {
            EnsureSecurityDeclarationsInitialized();
            var result = new OwnedCollection<IHasSecurityDeclaration, SecurityDeclaration>(owner);

            foreach (uint rid in _securityDeclarations.GetValues(owner.MetadataToken))
            {
                var attribute = (SecurityDeclaration) LookupMember(new MetadataToken(TableIndex.DeclSecurity, rid));
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
            return _genericParameters.GetKey(parameterRid);
        }
        
        internal ICollection<uint> GetGenericParameters(MetadataToken ownerToken)
        {
            EnsureGenericParametersInitialized();
            return _genericParameters.GetValues(ownerToken);
        }

        private void EnsureGenericParameterConstrainsInitialized()
        {
            if (_genericParameterConstraints is null)
                Interlocked.CompareExchange(ref _genericParameterConstraints, InitializeGenericParameterConstraints(), null);
        }

        private OneToManyRelation<MetadataToken, uint> InitializeGenericParameterConstraints()
        {
            var tablesStream = DotNetDirectory.Metadata.GetStream<TablesStream>();
            var constraintTable = tablesStream.GetTable<GenericParameterConstraintRow>(TableIndex.GenericParamConstraint);
            
            var constraints = new OneToManyRelation<MetadataToken, uint>();
            for (int i = 0; i < constraintTable.Count; i++)
            {
                var ownerToken = new MetadataToken(TableIndex.GenericParam, constraintTable[i].Owner);
                uint parameterRid = (uint) (i + 1);
                constraints.Add(ownerToken, parameterRid);
            }

            return constraints;
        }

        internal MetadataToken GetGenericParameterConstraintOwner(uint constraintRid)
        {
            EnsureGenericParameterConstrainsInitialized();
            return _genericParameterConstraints.GetKey(constraintRid);
        }
        
        internal ICollection<uint> GetGenericParameterConstraints(MetadataToken ownerToken)
        {
            EnsureGenericParameterConstrainsInitialized();
            return _genericParameterConstraints.GetValues(ownerToken);
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

        internal MetadataToken GetInterfaceImplementationOwner(uint implementationRid)
        {
            EnsureInterfacesInitialized();
            return _interfaces.GetKey(implementationRid);
        }

        internal ICollection<uint> GetInterfaceImplementationRids(MetadataToken ownerToken)
        {
            EnsureInterfacesInitialized();
            return _interfaces.GetValues(ownerToken);
        }

        private void EnsureMethodImplementationsInitialized()
        {
            if (_methodImplementations is null)
                Interlocked.CompareExchange(ref _methodImplementations, InitializeMethodImplementations(), null);
        }

        private OneToManyRelation<MetadataToken, uint> InitializeMethodImplementations()
        {
            var tablesStream = DotNetDirectory.Metadata.GetStream<TablesStream>();
            var methodImplTable = tablesStream.GetTable<MethodImplementationRow>(TableIndex.MethodImpl);
            
            var methodImplementations = new OneToManyRelation<MetadataToken, uint>();
            for (int i = 0; i < methodImplTable.Count; i++)
            {
                var ownerToken = new MetadataToken(TableIndex.TypeDef, methodImplTable[i].Class);
                uint interfaceImplRid = (uint) (i + 1);
                methodImplementations.Add(ownerToken, interfaceImplRid);
            }

            return methodImplementations;
        }

        internal ICollection<uint> GetMethodImplementationRids(MetadataToken ownerToken)
        {
            EnsureMethodImplementationsInitialized();
            return _methodImplementations.GetValues(ownerToken);
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

        private void EnsureFieldRvasInitialized()
        {
            if (_fieldRvas is null)
                Interlocked.CompareExchange(ref _fieldRvas, InitializeFieldRvas(), null);
        }

        private OneToOneRelation<MetadataToken, uint> InitializeFieldRvas()
        {
            var tablesStream = DotNetDirectory.Metadata.GetStream<TablesStream>();
            var rvaTable = tablesStream.GetTable<FieldRvaRow>(TableIndex.FieldRva);
            
            var rvas = new OneToOneRelation<MetadataToken, uint>();
            for (int i = 0; i < rvaTable.Count; i++)
            {
                var ownerToken = new MetadataToken(TableIndex.Field, rvaTable[i].Field);
                uint rvaRid = (uint) (i + 1);
                rvas.Add(ownerToken, rvaRid);
            }

            return rvas;
        }

        internal uint GetFieldRvaRid(MetadataToken fieldToken)
        {
            EnsureFieldRvasInitialized();
            return _fieldRvas.GetValue(fieldToken);
        }

        private void EnsureFieldMarshalsInitialized()
        {
            if (_fieldMarshals is null)
                Interlocked.CompareExchange(ref _fieldMarshals, InitializeFieldMarshals(), null);
        }

        private OneToOneRelation<MetadataToken, uint> InitializeFieldMarshals()
        {
            var tablesStream = DotNetDirectory.Metadata.GetStream<TablesStream>();
            var marshalTable = tablesStream.GetTable<FieldMarshalRow>(TableIndex.FieldMarshal);
            var encoder = tablesStream.GetIndexEncoder(CodedIndex.HasFieldMarshal);

            var marshals = new OneToOneRelation<MetadataToken, uint>();
            for (int i = 0; i < marshalTable.Count; i++)
            {
                var ownerToken = encoder.DecodeIndex(marshalTable[i].Parent);
                uint rvaRid = (uint) (i + 1);
                marshals.Add(ownerToken, rvaRid);
            }

            return marshals;
        }

        internal uint GetFieldMarshalRid(MetadataToken fieldToken)
        {
            EnsureFieldMarshalsInitialized();
            return _fieldMarshals.GetValue(fieldToken);
        }

        internal MarshalDescriptor GetFieldMarshal(MetadataToken ownerToken)
        {
            var metadata = DotNetDirectory.Metadata;
            var table = metadata
                .GetStream<TablesStream>()
                .GetTable<FieldMarshalRow>(TableIndex.FieldMarshal);
            
            uint rid = GetFieldMarshalRid(ownerToken);

            if (table.TryGetByRid(rid, out var row))
            {
                var reader = metadata
                    .GetStream<BlobStream>()
                    .GetBlobReaderByIndex(row.NativeType);
                return MarshalDescriptor.FromReader(this, reader);
            }

            return null;
        } 
        
        private void EnsureFieldLayoutsInitialized()
        {
            if (_fieldLayouts is null)
                Interlocked.CompareExchange(ref _fieldLayouts, InitializeFieldLayouts(), null);
        }

        private OneToOneRelation<MetadataToken, uint> InitializeFieldLayouts()
        {
            var tablesStream = DotNetDirectory.Metadata.GetStream<TablesStream>();
            var layoutTable = tablesStream.GetTable<FieldLayoutRow>(TableIndex.FieldLayout);

            var fieldLayouts = new OneToOneRelation<MetadataToken, uint>();
            for (int i = 0; i < layoutTable.Count; i++)
            {
                var ownerToken = new MetadataToken(TableIndex.Field, layoutTable[i].Field);
                uint layoutRid = (uint) (i + 1);
                fieldLayouts.Add(ownerToken, layoutRid);
            }

            return fieldLayouts;
        }

        internal uint GetFieldLayoutRid(MetadataToken fieldToken)
        {
            EnsureFieldLayoutsInitialized();
            return _fieldLayouts.GetValue(fieldToken);
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
                result.Add(new SerializedAssemblyReference(ReaderContext, token, table[i]));
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
        protected override string GetRuntimeVersion() => DotNetDirectory.Metadata.VersionString;

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

        /// <inheritdoc />
        protected override IResourceDirectory GetNativeResources() => ReaderContext.Image.Resources;

        /// <inheritdoc />
        protected override IList<DebugDataEntry> GetDebugData() => new List<DebugDataEntry>(ReaderContext.Image.DebugData);

        private AssemblyDefinition FindParentAssembly()
        {
            var assemblyTable = DotNetDirectory.Metadata
                .GetStream<TablesStream>()
                .GetTable<AssemblyDefinitionRow>();

            if (assemblyTable.Count > 0)
            {
                var assembly = new SerializedAssemblyDefinition(
                    ReaderContext,
                    new MetadataToken(TableIndex.Assembly, 1),
                    assemblyTable[0],
                    this);
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
                if (KnownCorLibs.KnownCorLibNames.Contains(reference.Name))
                {
                    if (mostRecentVersion < reference.Version)
                        mostRecentCorLib = reference;
                }
            }

            if (mostRecentCorLib is null && Assembly is {})
            {
                if (KnownCorLibs.KnownCorLibNames.Contains(Assembly.Name))
                    mostRecentCorLib = this;
            }

            return mostRecentCorLib;
        }
    }
}