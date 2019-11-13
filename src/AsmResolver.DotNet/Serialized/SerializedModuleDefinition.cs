using System;
using System.Collections.Generic;
using System.Threading;
using AsmResolver.DotNet.Collections;
using AsmResolver.PE.DotNet.Metadata;
using AsmResolver.PE.DotNet.Metadata.Guid;
using AsmResolver.PE.DotNet.Metadata.Strings;
using AsmResolver.PE.DotNet.Metadata.Tables;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;

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

        private TypeReference[] _typeReferences;

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
        }

        /// <inheritdoc />
        public override IMetadataMember LookupMember(MetadataToken token)
        {
            switch (token.Table)
            {
                case TableIndex.TypeRef:
                    return LookupTypeReference(token);
                
                default:
                    throw new NotSupportedException();
            }
        }

        private TypeReference LookupTypeReference(MetadataToken token) =>
            LookupOrCreateMemberFromCache<TypeReference, TypeReferenceRow>(
                ref _typeReferences, token, (m, t, r) => new SerializedTypeReference(m, t, r));

        private TMember LookupOrCreateMemberFromCache<TMember, TRow>(ref TMember[] cache, MetadataToken token,
            Func<IMetadata, MetadataToken, TRow, TMember> createMember)
            where TRow : struct, IMetadataRow
            where TMember : class, IMetadataMember
        {
            // Obtain table.
            var table = (MetadataTable<TRow>) _metadata
                .GetStream<TablesStream>()
                .GetTable(token.Table);
            
            // Check if within bounds.
            if (token.Rid >= table.Count) 
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
            var types = new OwnedCollection<ModuleDefinition, TypeDefinition>(this);

            // TODO: exclude nested types.
            
            var typeDefTable = _metadata.GetStream<TablesStream>().GetTable<TypeDefinitionRow>();
            for (int i = 0; i < typeDefTable.Count; i++)
            {
                var token = new MetadataToken(TableIndex.TypeDef, (uint) i + 1);
                types.Add(new SerializedTypeDefinition(_metadata, token, typeDefTable[i]));
            }

            return types;
        }
    }
}