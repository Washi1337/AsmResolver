using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using AsmResolver.PE.DotNet.Metadata.Tables;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;

namespace AsmResolver.DotNet.Serialized
{
    internal class CachedSerializedMemberFactory
    {
        private readonly ModuleReaderContext _context;
        private readonly TablesStream _tablesStream;

        private TypeReference?[]? _typeReferences;
        private TypeDefinition?[]? _typeDefinitions;
        private FieldDefinition?[]? _fieldDefinitions;
        private MethodDefinition?[]? _methodDefinitions;
        private ParameterDefinition?[]? _parameterDefinitions;
        private MemberReference?[]? _memberReferences;
        private StandAloneSignature?[]? _standAloneSignatures;
        private PropertyDefinition?[]? _propertyDefinitions;
        private EventDefinition?[]? _eventDefinition;
        private MethodSemantics?[]? _methodSemantics;
        private TypeSpecification?[]? _typeSpecifications;
        private CustomAttribute?[]? _customAttributes;
        private MethodSpecification?[]? _methodSpecifications;
        private GenericParameter?[]? _genericParameters;
        private GenericParameterConstraint?[]? _genericParameterConstraints;
        private ModuleReference?[]? _moduleReferences;
        private FileReference?[]? _fileReferences;
        private ManifestResource?[]? _resources;
        private ExportedType?[]? _exportedTypes;
        private Constant?[]? _constants;
        private ClassLayout?[]? _classLayouts;
        private ImplementationMap?[]? _implementationMaps;
        private InterfaceImplementation?[]? _interfaceImplementations;
        private SecurityDeclaration?[]? _securityDeclarations;

        internal CachedSerializedMemberFactory(ModuleReaderContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _tablesStream = _context.Image.DotNetDirectory!.Metadata!.GetStream<TablesStream>();
        }

        internal bool TryLookupMember(MetadataToken token, [NotNullWhen(true)] out IMetadataMember? member)
        {
            member = token.Table switch
            {
                TableIndex.Module => LookupModuleDefinition(token),
                TableIndex.TypeRef => LookupTypeReference(token),
                TableIndex.TypeDef => LookupTypeDefinition(token),
                TableIndex.TypeSpec => LookupTypeSpecification(token),
                TableIndex.Assembly => LookupAssemblyDefinition(token),
                TableIndex.AssemblyRef => LookupAssemblyReference(token),
                TableIndex.Field => LookupFieldDefinition(token),
                TableIndex.Method => LookupMethodDefinition(token),
                TableIndex.Param => LookupParameterDefinition(token),
                TableIndex.MemberRef => LookupMemberReference(token),
                TableIndex.StandAloneSig => LookupStandAloneSignature(token),
                TableIndex.Property => LookupPropertyDefinition(token),
                TableIndex.Event => LookupEventDefinition(token),
                TableIndex.MethodSemantics => LookupMethodSemantics(token),
                TableIndex.CustomAttribute => LookupCustomAttribute(token),
                TableIndex.MethodSpec => LookupMethodSpecification(token),
                TableIndex.GenericParam => LookupGenericParameter(token),
                TableIndex.GenericParamConstraint => LookupGenericParameterConstraint(token),
                TableIndex.ModuleRef => LookupModuleReference(token),
                TableIndex.File => LookupFileReference(token),
                TableIndex.ManifestResource => LookupManifestResource(token),
                TableIndex.ExportedType => LookupExportedType(token),
                TableIndex.Constant => LookupConstant(token),
                TableIndex.ClassLayout => LookupClassLayout(token),
                TableIndex.ImplMap => LookupImplementationMap(token),
                TableIndex.InterfaceImpl => LookupInterfaceImplementation(token),
                TableIndex.DeclSecurity => LookupSecurityDeclaration(token),
                _ => null
            };

            return member is not null;
        }

        private ModuleDefinition? LookupModuleDefinition(in MetadataToken token)
        {
            return token.Rid == 1
                ? _context.ParentModule
                : null; // TODO: handle spurious assembly definition rows.
        }

        internal TypeReference? LookupTypeReference(MetadataToken token)
        {
            return LookupOrCreateMember<TypeReference, TypeReferenceRow>(ref _typeReferences, token,
                (c, t, r) => new SerializedTypeReference(c, t, r));
        }

        internal TypeDefinition? LookupTypeDefinition(MetadataToken token)
        {
            return LookupOrCreateMember<TypeDefinition, TypeDefinitionRow>(ref _typeDefinitions, token,
                (c, t, r) => new SerializedTypeDefinition(c, t, r));
        }

        internal TypeSpecification? LookupTypeSpecification(MetadataToken token)
        {
            return LookupOrCreateMember<TypeSpecification, TypeSpecificationRow>(ref _typeSpecifications, token,
                (c, t, r) => new SerializedTypeSpecification(c, t, r));
        }

        private AssemblyDefinition? LookupAssemblyDefinition(MetadataToken token)
        {
            return token.Rid == 1
                ? _context.ParentModule.Assembly
                : null; // TODO: handle spurious assembly definition rows.
        }

        internal IMetadataMember? LookupAssemblyReference(MetadataToken token)
        {
            return token.Rid != 0 && token.Rid <= _context.ParentModule.AssemblyReferences.Count
                ? _context.ParentModule.AssemblyReferences[(int) (token.Rid - 1)]                : null;
        }

        private FieldDefinition? LookupFieldDefinition(MetadataToken token)
        {
            return LookupOrCreateMember<FieldDefinition, FieldDefinitionRow>(ref _fieldDefinitions, token,
                (c, t, r) => new SerializedFieldDefinition(c, t, r));
        }

        private MethodDefinition? LookupMethodDefinition(MetadataToken token)
        {
            return LookupOrCreateMember<MethodDefinition, MethodDefinitionRow>(ref _methodDefinitions, token,
                (c, t, r) => new SerializedMethodDefinition(c, t, r));
        }

        private ParameterDefinition? LookupParameterDefinition(MetadataToken token)
        {
            return LookupOrCreateMember<ParameterDefinition, ParameterDefinitionRow>(ref _parameterDefinitions, token,
                (c, t, r) => new SerializedParameterDefinition(c, t, r));
        }

        private MemberReference? LookupMemberReference(MetadataToken token)
        {
            return LookupOrCreateMember<MemberReference, MemberReferenceRow>(ref _memberReferences, token,
                (c, t, r) => new SerializedMemberReference(c, t, r));
        }

        private StandAloneSignature? LookupStandAloneSignature(MetadataToken token)
        {
            return LookupOrCreateMember<StandAloneSignature, StandAloneSignatureRow>(ref _standAloneSignatures, token,
                (c, t, r) => new SerializedStandAloneSignature(c, t, r));
        }

        private PropertyDefinition? LookupPropertyDefinition(MetadataToken token)
        {
            return LookupOrCreateMember<PropertyDefinition, PropertyDefinitionRow>(ref _propertyDefinitions, token,
                (c, t, r) => new SerializedPropertyDefinition(c, t, r));
        }

        private EventDefinition? LookupEventDefinition(MetadataToken token)
        {
            return LookupOrCreateMember<EventDefinition, EventDefinitionRow>(ref _eventDefinition, token,
                (c, t, r) => new SerializedEventDefinition(c, t, r));
        }

        private MethodSemantics? LookupMethodSemantics(MetadataToken token)
        {
            return LookupOrCreateMember<MethodSemantics, MethodSemanticsRow>(ref _methodSemantics, token,
                (c, t, r) => new SerializedMethodSemantics(c, t, r));
        }

        private CustomAttribute? LookupCustomAttribute(MetadataToken token)
        {
            return LookupOrCreateMember<CustomAttribute, CustomAttributeRow>(ref _customAttributes, token,
                (c, t, r) => new SerializedCustomAttribute(c, t, r));
        }

        private IMetadataMember? LookupMethodSpecification(MetadataToken token)
        {
            return LookupOrCreateMember<MethodSpecification, MethodSpecificationRow>(ref _methodSpecifications, token,
                (c, t, r) => new SerializedMethodSpecification(c, t, r));
        }

        private GenericParameter? LookupGenericParameter(MetadataToken token)
        {
            return LookupOrCreateMember<GenericParameter, GenericParameterRow>(ref _genericParameters, token,
                (c, t, r) => new SerializedGenericParameter(c, t, r));
        }

        private GenericParameterConstraint? LookupGenericParameterConstraint(in MetadataToken token)
        {
            return LookupOrCreateMember<GenericParameterConstraint, GenericParameterConstraintRow>(ref _genericParameterConstraints, token,
                (c, t, r) => new SerializedGenericParameterConstraint(c, t, r));
        }

        private ModuleReference? LookupModuleReference(MetadataToken token)
        {
            return LookupOrCreateMember<ModuleReference, ModuleReferenceRow>(ref _moduleReferences, token,
                (c, t, r) => new SerializedModuleReference(c, t, r));
        }

        private FileReference? LookupFileReference(MetadataToken token)
        {
            return LookupOrCreateMember<FileReference, FileReferenceRow>(ref _fileReferences, token,
                (c, t, r) => new SerializedFileReference(c, t, r));
        }

        private ManifestResource? LookupManifestResource(MetadataToken token)
        {
            return LookupOrCreateMember<ManifestResource, ManifestResourceRow>(ref _resources, token,
                (c, t, r) => new SerializedManifestResource(c, t, r));
        }

        private ExportedType? LookupExportedType(MetadataToken token)
        {
            return LookupOrCreateMember<ExportedType, ExportedTypeRow>(ref _exportedTypes, token,
                (c, t, r) => new SerializedExportedType(c, t, r));
        }

        private Constant? LookupConstant(MetadataToken token)
        {
            return LookupOrCreateMember<Constant, ConstantRow>(ref _constants, token,
                (c, t, r) => new SerializedConstant(c, t, r));
        }

        private ClassLayout? LookupClassLayout(MetadataToken token)
        {
            return LookupOrCreateMember<ClassLayout, ClassLayoutRow>(ref _classLayouts, token,
                (c, t, r) => new SerializedClassLayout(c, t, r));
        }

        internal ImplementationMap? LookupImplementationMap(MetadataToken token)
        {
            return LookupOrCreateMember<ImplementationMap, ImplementationMapRow>(ref _implementationMaps, token,
                (c, t, r) => new SerializedImplementationMap(c, t, r));
        }

        private InterfaceImplementation? LookupInterfaceImplementation(MetadataToken token)
        {
            return LookupOrCreateMember<InterfaceImplementation, InterfaceImplementationRow>(ref _interfaceImplementations, token,
                (c, t, r) => new SerializedInterfaceImplementation(c, t, r));
        }

        private SecurityDeclaration? LookupSecurityDeclaration(MetadataToken token)
        {
            return LookupOrCreateMember<SecurityDeclaration, SecurityDeclarationRow>(ref _securityDeclarations, token,
                (c, t, r) => new SerializedSecurityDeclaration(c, t, r));
        }

        internal TMember? LookupOrCreateMember<TMember, TRow>(ref TMember?[]? cache, MetadataToken token,
            Func<ModuleReaderContext, MetadataToken, TRow, TMember?> createMember)
            where TRow : struct, IMetadataRow
            where TMember : class, IMetadataMember
        {
            // Obtain table.
            var table = (MetadataTable<TRow>) _tablesStream.GetTable(token.Table);

            // Check if within bounds.
            if (token.Rid == 0 || token.Rid > table.Count)
                return null;

            // Allocate cache if necessary.
            if (cache is null)
                Interlocked.CompareExchange(ref cache, new TMember[table.Count], null);

            // Get or create cached member.
            int index = (int) token.Rid - 1;
            var member = cache[index];
            if (member is null)
            {
                member = createMember(_context, token, table[index]);
                member = Interlocked.CompareExchange(ref cache[index], member, null)
                         ?? member;
            }

            return member;
        }
    }
}
