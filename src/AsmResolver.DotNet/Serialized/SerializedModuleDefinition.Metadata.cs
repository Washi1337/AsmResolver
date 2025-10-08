using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using AsmResolver.Collections;
using AsmResolver.DotNet.Collections;
using AsmResolver.DotNet.Signatures;
using AsmResolver.PE.DotNet.Metadata.Tables;
using AsmResolver.PE.Exports;
using AsmResolver.PE.Platforms;

namespace AsmResolver.DotNet.Serialized
{
    public partial class SerializedModuleDefinition
    {
        private readonly CachedSerializedMemberFactory _memberFactory;
        private readonly LazyRidListRelation<TypeDefinitionRow> _fieldLists;
        private readonly LazyRidListRelation<TypeDefinitionRow> _methodLists;
        private readonly LazyRidListRelation<MethodDefinitionRow> _paramLists;
        private readonly LazyRidListRelation<PropertyMapRow> _propertyLists;
        private readonly LazyRidListRelation<EventMapRow> _eventLists;

        private OneToManyRelation<uint, uint>? _typeDefTree;
        private OneToManyRelation<MetadataToken, uint>? _semantics;
        private Dictionary<uint, MetadataToken>? _semanticMethods;
        private OneToOneRelation<MetadataToken, uint>? _constants;
        private OneToManyRelation<MetadataToken, uint>? _customAttributes;
        private OneToManyRelation<MetadataToken, uint>? _securityDeclarations;
        private OneToManyRelation<MetadataToken, uint>? _genericParameters;
        private OneToManyRelation<MetadataToken, uint>? _genericParameterConstraints;
        private OneToManyRelation<MetadataToken, uint>? _interfaces;
        private OneToManyRelation<MetadataToken, uint>? _methodImplementations;
        private OneToOneRelation<MetadataToken, uint>? _classLayouts;
        private OneToOneRelation<MetadataToken, uint>? _implementationMaps;
        private OneToOneRelation<MetadataToken, uint>? _fieldRvas;
        private OneToOneRelation<MetadataToken, uint>? _fieldMarshals;
        private OneToOneRelation<MetadataToken, uint>? _fieldLayouts;

        private Dictionary<MetadataToken, UnmanagedExportInfo>? _exportInfos;

        [MemberNotNull(nameof(_typeDefTree))]
        private void EnsureTypeDefinitionTreeInitialized()
        {
            if (_typeDefTree is null)
                Interlocked.CompareExchange(ref _typeDefTree, InitializeTypeDefinitionTree(), null);
        }

        private OneToManyRelation<uint, uint> InitializeTypeDefinitionTree()
        {
            var tablesStream = ReaderContext.TablesStream;
            var nestedClassTable = tablesStream.GetTable<NestedClassRow>(TableIndex.NestedClass);

            var typeDefTree = new OneToManyRelation<uint, uint>();
            foreach (var nestedClass in nestedClassTable)
                typeDefTree.Add(nestedClass.EnclosingClass, nestedClass.NestedClass);

            return typeDefTree;
        }

        internal OneToManyRelation<uint, uint>.ValueSet GetNestedTypeRids(uint enclosingTypeRid)
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

        [MemberNotNull(nameof(_semantics))]
        [MemberNotNull(nameof(_semanticMethods))]
        private void EnsureMethodSemanticsInitialized()
        {
            if (_semantics is null || _semanticMethods is null)
                InitializeMethodSemantics();
        }

        [MemberNotNull(nameof(_semantics))]
        [MemberNotNull(nameof(_semanticMethods))]
        private void InitializeMethodSemantics()
        {
            var tablesStream = ReaderContext.TablesStream;
            var semanticsTable = tablesStream.GetTable<MethodSemanticsRow>(TableIndex.MethodSemantics);
            var encoder = tablesStream.GetIndexEncoder(CodedIndex.HasSemantics);

            var semantics = new OneToManyRelation<MetadataToken, uint>(semanticsTable.Count);
            var semanticMethods = new Dictionary<uint, MetadataToken>(semanticsTable.Count);
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

        internal OneToManyRelation<MetadataToken, uint>.ValueSet GetMethodSemantics(MetadataToken owner)
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

        [MemberNotNull(nameof(_constants))]
        private void EnsureConstantsInitialized()
        {
            if (_constants is null)
                Interlocked.CompareExchange(ref _constants, GetConstants(), null);
        }

        private OneToOneRelation<MetadataToken, uint> GetConstants()
        {
            var tablesStream = ReaderContext.TablesStream;
            var constantTable = tablesStream.GetTable<ConstantRow>(TableIndex.Constant);
            var encoder = tablesStream.GetIndexEncoder(CodedIndex.HasConstant);

            var constants = new OneToOneRelation<MetadataToken, uint>(constantTable.Count);
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

        internal Constant? GetConstant(MetadataToken ownerToken)
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

        [MemberNotNull(nameof(_customAttributes))]
        private void EnsureCustomAttributesInitialized()
        {
            if (_customAttributes is null)
                Interlocked.CompareExchange(ref _customAttributes, InitializeCustomAttributes(), null);
        }

        private OneToManyRelation<MetadataToken, uint> InitializeCustomAttributes()
        {
            var tablesStream = ReaderContext.TablesStream;
            var attributeTable = tablesStream.GetTable<CustomAttributeRow>(TableIndex.CustomAttribute);
            var encoder = tablesStream.GetIndexEncoder(CodedIndex.HasCustomAttribute);

            var customAttributes = new OneToManyRelation<MetadataToken, uint>(attributeTable.Count);
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

        internal OneToManyRelation<MetadataToken, uint>.ValueSet GetCustomAttributes(MetadataToken ownerToken)
        {
            EnsureCustomAttributesInitialized();
            return _customAttributes.GetValues(ownerToken);
        }

        internal MetadataToken GetCustomAttributeOwner(uint attributeRid)
        {
            EnsureCustomAttributesInitialized();
            return _customAttributes.GetKey(attributeRid);
        }

        internal bool HasNonEmptyCustomAttributes(IHasCustomAttribute owner)
        {
            EnsureCustomAttributesInitialized();
            return _customAttributes.GetValues(owner.MetadataToken).Count > 0;
        }

        internal IList<CustomAttribute> GetCustomAttributeCollection(IHasCustomAttribute owner)
        {
            EnsureCustomAttributesInitialized();
            var rids = _customAttributes.GetValues(owner.MetadataToken);
            var result = new MemberCollection<IHasCustomAttribute, CustomAttribute>(owner, rids.Count);

            foreach (uint rid in rids)
            {
                var attribute = (CustomAttribute) LookupMember(new MetadataToken(TableIndex.CustomAttribute, rid));
                result.AddNoOwnerCheck(attribute);
            }

            return result;
        }

        [MemberNotNull(nameof(_securityDeclarations))]
        private void EnsureSecurityDeclarationsInitialized()
        {
            if (_securityDeclarations is null)
                Interlocked.CompareExchange(ref _securityDeclarations, InitializeSecurityDeclarations(), null);
        }

        private OneToManyRelation<MetadataToken, uint> InitializeSecurityDeclarations()
        {
            var tablesStream = ReaderContext.TablesStream;
            var declarationTable = tablesStream.GetTable<SecurityDeclarationRow>(TableIndex.DeclSecurity);
            var encoder = tablesStream.GetIndexEncoder(CodedIndex.HasDeclSecurity);

            var securityDeclarations = new OneToManyRelation<MetadataToken, uint>(declarationTable.Count);
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
            EnsureSecurityDeclarationsInitialized();
            return _securityDeclarations.GetKey(attributeRid);
        }

        internal IList<SecurityDeclaration> GetSecurityDeclarationCollection(IHasSecurityDeclaration owner)
        {
            EnsureSecurityDeclarationsInitialized();
            var rids = _securityDeclarations.GetValues(owner.MetadataToken);
            var result = new MemberCollection<IHasSecurityDeclaration, SecurityDeclaration>(owner, rids.Count);

            foreach (uint rid in rids)
            {
                var attribute = (SecurityDeclaration) LookupMember(new MetadataToken(TableIndex.DeclSecurity, rid));
                result.AddNoOwnerCheck(attribute);
            }

            return result;
        }

        [MemberNotNull(nameof(_genericParameters))]
        private void EnsureGenericParametersInitialized()
        {
            if (_genericParameters is null)
                Interlocked.CompareExchange(ref _genericParameters, InitializeGenericParameters(), null);
        }

        private OneToManyRelation<MetadataToken, uint> InitializeGenericParameters()
        {
            var tablesStream = ReaderContext.TablesStream;
            var parameterTable = tablesStream.GetTable<GenericParameterRow>(TableIndex.GenericParam);
            var encoder = tablesStream.GetIndexEncoder(CodedIndex.TypeOrMethodDef);

            var genericParameters = new OneToManyRelation<MetadataToken, uint>(parameterTable.Count);
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

        internal OneToManyRelation<MetadataToken, uint>.ValueSet GetGenericParameters(MetadataToken ownerToken)
        {
            EnsureGenericParametersInitialized();
            return _genericParameters.GetValues(ownerToken);
        }

        [MemberNotNull(nameof(_genericParameterConstraints))]
        private void EnsureGenericParameterConstrainsInitialized()
        {
            if (_genericParameterConstraints is null)
                Interlocked.CompareExchange(ref _genericParameterConstraints, InitializeGenericParameterConstraints(), null);
        }

        private OneToManyRelation<MetadataToken, uint> InitializeGenericParameterConstraints()
        {
            var tablesStream = ReaderContext.TablesStream;
            var constraintTable = tablesStream.GetTable<GenericParameterConstraintRow>(TableIndex.GenericParamConstraint);

            var constraints = new OneToManyRelation<MetadataToken, uint>(constraintTable.Count);
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

        internal OneToManyRelation<MetadataToken, uint>.ValueSet GetGenericParameterConstraints(MetadataToken ownerToken)
        {
            EnsureGenericParameterConstrainsInitialized();
            return _genericParameterConstraints.GetValues(ownerToken);
        }

        [MemberNotNull(nameof(_interfaces))]
        private void EnsureInterfacesInitialized()
        {
            if (_interfaces is null)
                Interlocked.CompareExchange(ref _interfaces, InitializeInterfaces(), null);
        }

        private OneToManyRelation<MetadataToken, uint> InitializeInterfaces()
        {
            var tablesStream = ReaderContext.TablesStream;
            var interfaceImplTable = tablesStream.GetTable<InterfaceImplementationRow>(TableIndex.InterfaceImpl);

            var interfaces = new OneToManyRelation<MetadataToken, uint>(interfaceImplTable.Count);
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

        internal OneToManyRelation<MetadataToken, uint>.ValueSet GetInterfaceImplementationRids(MetadataToken ownerToken)
        {
            EnsureInterfacesInitialized();
            return _interfaces.GetValues(ownerToken);
        }

        [MemberNotNull(nameof(_methodImplementations))]
        private void EnsureMethodImplementationsInitialized()
        {
            if (_methodImplementations is null)
                Interlocked.CompareExchange(ref _methodImplementations, InitializeMethodImplementations(), null);
        }

        private OneToManyRelation<MetadataToken, uint> InitializeMethodImplementations()
        {
            var tablesStream = ReaderContext.TablesStream;
            var methodImplTable = tablesStream.GetTable<MethodImplementationRow>(TableIndex.MethodImpl);

            var methodImplementations = new OneToManyRelation<MetadataToken, uint>(methodImplTable.Count);
            for (int i = 0; i < methodImplTable.Count; i++)
            {
                var ownerToken = new MetadataToken(TableIndex.TypeDef, methodImplTable[i].Class);
                uint interfaceImplRid = (uint) (i + 1);
                methodImplementations.Add(ownerToken, interfaceImplRid);
            }

            return methodImplementations;
        }

        internal OneToManyRelation<MetadataToken, uint>.ValueSet GetMethodImplementationRids(MetadataToken ownerToken)
        {
            EnsureMethodImplementationsInitialized();
            return _methodImplementations.GetValues(ownerToken);
        }

        [MemberNotNull(nameof(_classLayouts))]
        private void EnsureClassLayoutsInitialized()
        {
            if (_classLayouts is null)
                Interlocked.CompareExchange(ref _classLayouts, InitializeClassLayouts(), null);
        }

        private OneToOneRelation<MetadataToken, uint> InitializeClassLayouts()
        {
            var tablesStream = ReaderContext.TablesStream;
            var layoutTable = tablesStream.GetTable<ClassLayoutRow>(TableIndex.ClassLayout);

            var layouts = new OneToOneRelation<MetadataToken, uint>(layoutTable.Count);
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

        [MemberNotNull(nameof(_implementationMaps))]
        private void EnsureImplementationMapsInitialized()
        {
            if (_implementationMaps is null)
                Interlocked.CompareExchange(ref _implementationMaps, InitializeImplementationMaps(), null);
        }

        private OneToOneRelation<MetadataToken, uint> InitializeImplementationMaps()
        {
            var tablesStream = ReaderContext.TablesStream;
            var mapTable = tablesStream.GetTable<ImplementationMapRow>(TableIndex.ImplMap);
            var encoder = tablesStream.GetIndexEncoder(CodedIndex.TypeOrMethodDef);

            var maps = new OneToOneRelation<MetadataToken, uint>(mapTable.Count);
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

        [MemberNotNull(nameof(_fieldRvas))]
        private void EnsureFieldRvasInitialized()
        {
            if (_fieldRvas is null)
                Interlocked.CompareExchange(ref _fieldRvas, InitializeFieldRvas(), null);
        }

        private OneToOneRelation<MetadataToken, uint> InitializeFieldRvas()
        {
            var tablesStream = ReaderContext.TablesStream;
            var rvaTable = tablesStream.GetTable<FieldRvaRow>(TableIndex.FieldRva);

            var rvas = new OneToOneRelation<MetadataToken, uint>(rvaTable.Count);
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

        [MemberNotNull(nameof(_fieldMarshals))]
        private void EnsureFieldMarshalsInitialized()
        {
            if (_fieldMarshals is null)
                Interlocked.CompareExchange(ref _fieldMarshals, InitializeFieldMarshals(), null);
        }

        private OneToOneRelation<MetadataToken, uint> InitializeFieldMarshals()
        {
            var tablesStream = ReaderContext.TablesStream;
            var marshalTable = tablesStream.GetTable<FieldMarshalRow>(TableIndex.FieldMarshal);
            var encoder = tablesStream.GetIndexEncoder(CodedIndex.HasFieldMarshal);

            var marshals = new OneToOneRelation<MetadataToken, uint>(marshalTable.Count);
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

        internal MarshalDescriptor? GetFieldMarshal(MetadataToken ownerToken)
        {
            var table = ReaderContext.TablesStream.GetTable<FieldMarshalRow>(TableIndex.FieldMarshal);

            if (ReaderContext.BlobStream is not { } blobStream
                || !table.TryGetByRid(GetFieldMarshalRid(ownerToken), out var row)
                || !blobStream.TryGetBlobReaderByIndex(row.NativeType, out var reader))
            {
                return null;
            }

            return MarshalDescriptor.FromReader(this, ref reader);
        }

        [MemberNotNull(nameof(_fieldLayouts))]
        private void EnsureFieldLayoutsInitialized()
        {
            if (_fieldLayouts is null)
                Interlocked.CompareExchange(ref _fieldLayouts, InitializeFieldLayouts(), null);
        }

        private OneToOneRelation<MetadataToken, uint> InitializeFieldLayouts()
        {
            var tablesStream = ReaderContext.TablesStream;
            var layoutTable = tablesStream.GetTable<FieldLayoutRow>(TableIndex.FieldLayout);

            var fieldLayouts = new OneToOneRelation<MetadataToken, uint>(layoutTable.Count);
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

        [MemberNotNull(nameof(_exportInfos))]
        private void EnsureExportInfosInitialized()
        {
            if (_exportInfos is null)
                Interlocked.CompareExchange(ref _exportInfos, InitializeExportInfos(), null);
        }

        private Dictionary<MetadataToken, UnmanagedExportInfo> InitializeExportInfos()
        {
            var result = new Dictionary<MetadataToken, UnmanagedExportInfo>();

            // Get relevant data directories.
            var exportDirectory = ReaderContext.Image.Exports;
            if (exportDirectory is null)
                return result;

            var vtableDirectory = ReaderContext.Image.DotNetDirectory?.VTableFixups;
            if (vtableDirectory is null || vtableDirectory.Count == 0)
                return result;

            // Check if PE machine type is supported.
            if (!Platform.TryGet(ReaderContext.Image.MachineType, out var platform))
                return result;

            // Try to extract from all thunks the VA that was referenced in the stub code.
            // This is not guaranteed to work since an unmanaged export does not necessarily have to be
            // using the exact same code every time. However, every .NET compiler or assembler always
            // emits a thunk stub that is very similar, which we can pattern match.
            var exportedThunks = new Dictionary<uint, ExportedSymbol>();
            for (int i = 0; i < exportDirectory.Entries.Count; i++)
            {
                var export = exportDirectory.Entries[i];
                if (export.Address.CanRead)
                {
                    var reader = export.Address.CreateReader();
                    if (platform.TryExtractThunkAddress(ReaderContext.Image, reader, out uint thunkRva))
                        exportedThunks.Add(thunkRva, export);
                }
            }

            // Map the extracted addresses to vtable slots, and assign export infos.
            for (int i = 0; i < vtableDirectory.Count; i++)
            {
                var fixup = vtableDirectory[i];
                var tokens = fixup.Tokens;

                for (int j = 0; j < tokens.Count; j++)
                {
                    uint rva = tokens.Rva + tokens.GetOffsetToIndex(j);
                    if (exportedThunks.TryGetValue(rva, out var symbol))
                    {
                        result[tokens[j]] = symbol.IsByName
                            ? new UnmanagedExportInfo(symbol.Name, tokens.Type)
                            : new UnmanagedExportInfo((ushort) symbol.Ordinal, tokens.Type);
                    }
                }
            }

            return result;
        }

        internal UnmanagedExportInfo? GetExportInfo(MetadataToken methodToken)
        {
            EnsureExportInfosInitialized();
            return _exportInfos.TryGetValue(methodToken, out var result)
                ? result
                : null;
        }
    }
}
