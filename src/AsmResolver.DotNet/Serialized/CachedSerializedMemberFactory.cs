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
                TableIndex.AssemblyRef => LookupAssemblyReference(token),
                TableIndex.Field => LookupFieldReference(token),
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

        internal IMetadataMember LookupAssemblyReference(MetadataToken token)
        {
            return token.Rid != 0 && token.Rid <= _parentModule.AssemblyReferences.Count
                ? _parentModule.AssemblyReferences[(int) (token.Rid - 1)]
                : null;
        }

        private FieldDefinition LookupFieldReference(MetadataToken token)
        {
            return LookupOrCreateMember<FieldDefinition, FieldDefinitionRow>(ref _fieldDefinitions, token,
                (m, t, r) => new SerializedFieldDefinition(m, _parentModule, t, r));
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
            if (token.Rid > table.Count) 
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