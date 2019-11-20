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

        private IDictionary<uint, IList<uint>> _typeDefTree;
        private IDictionary<uint, uint> _parentTypeRids;
        private TypeReference[] _typeReferences;
        private TypeDefinition[] _typeDefinitions;

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
        public override IMetadataMember LookupMember(MetadataToken token) =>
            !TryLookupMember(token, out var member)
                ? throw new ArgumentException($"Cannot resolve metadata token {token}.")
                : member;

        /// <inheritdoc />
        public override bool TryLookupMember(MetadataToken token, out IMetadataMember member)
        {
            member = token.Table switch
            {
                TableIndex.TypeRef => LookupTypeReference(token),
                TableIndex.TypeDef => LookupTypeDefinition(token),
                TableIndex.AssemblyRef => LookupAssemblyReference(token),
                _ => null
            };

            return member != null;
        }

        private TypeReference LookupTypeReference(MetadataToken token) =>
            LookupOrCreateMemberFromCache<TypeReference, TypeReferenceRow>(ref _typeReferences, token,
                (m, t, r) => new SerializedTypeReference(m, this, t, r));

        private TypeDefinition LookupTypeDefinition(MetadataToken token) => 
            LookupOrCreateMemberFromCache<TypeDefinition, TypeDefinitionRow>(ref _typeDefinitions, token, 
                (m, t, r) => new SerializedTypeDefinition(m, this, t, r));
        
        private IMetadataMember LookupAssemblyReference(MetadataToken token) =>
            token.Rid != 0 && token.Rid <= AssemblyReferences.Count
                ? AssemblyReferences[(int) (token.Rid - 1)]
                : null;

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

            var typeDefTable = _metadata.GetStream<TablesStream>().GetTable<TypeDefinitionRow>();
            for (int i = 0; i < typeDefTable.Count; i++)
            {
                uint rid = (uint) i + 1;
                if (!_parentTypeRids.ContainsKey(rid))
                {
                    var token = new MetadataToken(TableIndex.TypeDef, rid);
                    types.Add(LookupTypeDefinition(token));
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
            var nestedClassTable = tablesStream.GetTable<NestedClassRow>();
            
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
        
    }
}