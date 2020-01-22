using System;
using System.Threading;
using AsmResolver.PE.DotNet.Metadata;
using AsmResolver.PE.DotNet.Metadata.Tables;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;

namespace AsmResolver.DotNet.Serialized
{
    internal class CachedSerializedMemberFactory
    {
        private readonly IMetadata _metadata;
        private readonly SerializedModuleDefinition _parentModule;
        
        private TypeReference[] _typeReferences;
        private TypeDefinition[] _typeDefinitions;
        private FieldDefinition[] _fieldDefinitions;
        private MethodDefinition[] _methodDefinitions;
        private ParameterDefinition[] _parameterDefinitions;
        private MemberReference[] _memberReferences;
        private StandAloneSignature[] _standAloneSignatures;
        private PropertyDefinition[] _propertyDefinitions;
        private EventDefinition[] _eventDefinition;
        private MethodSemantics[] _methodSemantics;
        private TypeSpecification[] _typeSpecifications;
        private CustomAttribute[] _customAttributes;
        private MethodSpecification[] _methodSpecifications;

        internal CachedSerializedMemberFactory(IMetadata metadata, SerializedModuleDefinition parentModule)
        {
            _metadata = metadata;
            _parentModule = parentModule;
        }

        internal bool TryLookupMember(MetadataToken token, out IMetadataMember member)
        {
            member = token.Table switch
            {
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
                _ => null
            };

            return member != null;
        }

        internal TypeReference LookupTypeReference(MetadataToken token)
        {
            return LookupOrCreateMember<TypeReference, TypeReferenceRow>(ref _typeReferences, token,
                (m, t, r) => new SerializedTypeReference(m, _parentModule, t, r));
        }

        internal TypeDefinition LookupTypeDefinition(MetadataToken token)
        {
            return LookupOrCreateMember<TypeDefinition, TypeDefinitionRow>(ref _typeDefinitions, token,
                (m, t, r) => new SerializedTypeDefinition(m, _parentModule, t, r));
        }

        internal TypeSpecification LookupTypeSpecification(MetadataToken token)
        {
            return LookupOrCreateMember<TypeSpecification, TypeSpecificationRow>(ref _typeSpecifications, token,
                (m, t, r) => new SerializedTypeSpecification(m, _parentModule, t, r));
        }

        private AssemblyDefinition LookupAssemblyDefinition(in MetadataToken token)
        {
            return token.Rid == 1
                ? _parentModule.Assembly
                : null; // TODO: handle spurious assembly definition rows.
        }

        internal IMetadataMember LookupAssemblyReference(MetadataToken token)
        {
            return token.Rid != 0 && token.Rid <= _parentModule.AssemblyReferences.Count
                ? _parentModule.AssemblyReferences[(int) (token.Rid - 1)]
                : null;
        }

        private FieldDefinition LookupFieldDefinition(MetadataToken token)
        {
            return LookupOrCreateMember<FieldDefinition, FieldDefinitionRow>(ref _fieldDefinitions, token,
                (m, t, r) => new SerializedFieldDefinition(m, _parentModule, t, r));
        }

        private MethodDefinition LookupMethodDefinition(MetadataToken token)
        {
            return LookupOrCreateMember<MethodDefinition, MethodDefinitionRow>(ref _methodDefinitions, token,
                (m, t, r) => new SerializedMethodDefinition(m, _parentModule, t, r));
        }

        private ParameterDefinition LookupParameterDefinition(MetadataToken token)
        {   
            return LookupOrCreateMember<ParameterDefinition, ParameterDefinitionRow>(ref _parameterDefinitions, token,
                (m, t, r) => new SerializedParameterDefinition(m, _parentModule, t, r));
        }

        private MemberReference LookupMemberReference(MetadataToken token)
        {
            return LookupOrCreateMember<MemberReference, MemberReferenceRow>(ref _memberReferences, token,
                (m, t, r) => new SerializedMemberReference(m, _parentModule, t, r));
        }

        private StandAloneSignature LookupStandAloneSignature(in MetadataToken token)
        {
            return LookupOrCreateMember<StandAloneSignature, StandAloneSignatureRow>(ref _standAloneSignatures, token,
                (m, t, r) => new SerializedStandAloneSignature(m, _parentModule, t, r));
        }

        private PropertyDefinition LookupPropertyDefinition(in MetadataToken token)
        {
            return LookupOrCreateMember<PropertyDefinition, PropertyDefinitionRow>(ref _propertyDefinitions, token,
                (m, t, r) => new SerializedPropertyDefinition(m, _parentModule, t, r));
        }

        private EventDefinition LookupEventDefinition(in MetadataToken token)
        {
            return LookupOrCreateMember<EventDefinition, EventDefinitionRow>(ref _eventDefinition, token,
                (m, t, r) => new SerializedEventDefinition(m, _parentModule, t, r));
        }

        private MethodSemantics LookupMethodSemantics(in MetadataToken token)
        {
            return LookupOrCreateMember<MethodSemantics, MethodSemanticsRow>(ref _methodSemantics, token,
                (m, t, r) => new SerializedMethodSemantics(m, _parentModule, t, r));
        }

        private CustomAttribute LookupCustomAttribute(in MetadataToken token)
        {
            return LookupOrCreateMember<CustomAttribute, CustomAttributeRow>(ref _customAttributes, token,
                (m, t, r) => new SerializedCustomAttribute(m, _parentModule, t, r));
        }

        private IMetadataMember LookupMethodSpecification(in MetadataToken token)
        {
            return LookupOrCreateMember<MethodSpecification, MethodSpecificationRow>(ref _methodSpecifications, token,
                (m, t, r) => new SerializedMethodSpecification(m, _parentModule, t, r));
        }

        internal TMember LookupOrCreateMember<TMember, TRow>(ref TMember[] cache, MetadataToken token,
            Func<IMetadata, MetadataToken, TRow, TMember> createMember)
            where TRow : struct, IMetadataRow
            where TMember : class, IMetadataMember
        {
            // Obtain table.
            var table = (MetadataTable<TRow>) _metadata
                .GetStream<TablesStream>()
                .GetTable(token.Table);
            
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
                member = createMember(_metadata, token, table[index]);
                member = Interlocked.CompareExchange(ref cache[index], member, null)
                         ?? member;
            }

            return member;
        }
    }
}