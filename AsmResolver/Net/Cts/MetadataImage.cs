using System;
using System.Collections.Generic;
using AsmResolver.Net.Cil;
using AsmResolver.Net.Cts.Collections;
using AsmResolver.Net.Metadata;
using AsmResolver.Net.Signatures;

namespace AsmResolver.Net.Cts
{
    /// <summary>
    /// Provides a high-level view on the .NET metadata tables that somewhat resembles the hierarchical structure found
    /// in the common type system (CTS). It contains the root assembly definition, as well as various ways to resolve
    /// members by their token.
    ///
    /// When a metadata image is instantiated from a <see cref="MetadataHeader"/>, the metadata header is automatically
    /// locked and cannot be changed until the image has been committed to the .NET streams. This is done by
    /// <see cref="MetadataHeader.UnlockMetadata"/>.
    /// </summary>
    public class MetadataImage
    {
        private readonly IDictionary<MetadataToken, IMetadataMember> _cachedMembers = new Dictionary<MetadataToken, IMetadataMember>();
        private readonly LazyValue<MethodDefinition> _entrypoint;

        private IDictionary<uint, ICollection<uint>> _nestedClasses = null;
        private ICollection<uint> _topLevelTypes = null;
        
        internal MetadataImage(MetadataHeader header)
        {
            Header = header ?? throw new ArgumentNullException(nameof(header));
            var tableStream = header.GetStream<TableStream>();

            var table = tableStream.GetTable(MetadataTokenType.Assembly);

            if (table.TryGetRow(0, out var assemblyRow))
                Assembly = (AssemblyDefinition) table.GetMemberFromRow(this, assemblyRow);
            else
                Assembly = new AssemblyDefinition(null, new Version());

            TypeSystem = new TypeSystem(this, Assembly.Name == "mscorlib");
            MetadataResolver = new DefaultMetadataResolver(new DefaultNetAssemblyResolver());

            _entrypoint = new LazyValue<MethodDefinition>(() =>
                TryResolveMember(new MetadataToken(header.NetDirectory.EntryPointToken), out var member)
                    ? member as MethodDefinition
                    : null);
        }

        /// <summary>
        /// Gets the metadata header this image was based on. 
        /// </summary>
        public MetadataHeader Header
        {
            get;
        }

        /// <summary>
        /// Gets a collection of the basic type signatures that are used throughout the image.
        /// </summary>
        public TypeSystem TypeSystem
        {
            get;
        }

        /// <summary>
        /// Gets or sets the metadata resolver that will be used when <see cref="IResolvable.Resolve"/> is called on a
        /// specific member reference.
        /// </summary>
        public IMetadataResolver MetadataResolver
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the root assembly definition that is defined in the assembly.
        /// </summary>
        public AssemblyDefinition Assembly
        {
            get;
        }

        /// <summary>
        /// Gets or sets the managed entrypoint of this metadata image.
        /// </summary>
        public MethodDefinition ManagedEntrypoint
        {
            get => _entrypoint.Value;
            set => _entrypoint.Value = value;
        }

        /// <summary>
        /// Gets the module static constructor of this metadata image. That is, the first method that is executed
        /// upon loading the .NET module. 
        /// </summary>
        /// <returns>The module constructor, or <c>null</c> if none is present.</returns>
        public MethodDefinition GetModuleConstructor()
        {
            if (TryResolveMember(new MetadataToken(MetadataTokenType.Method, 1), out var member))
            {
                var cctor = (MethodDefinition) member;
                if (cctor.IsConstructor && cctor.IsStatic)
                    return cctor;
            }

            return null;
        }
        
        /// <summary>
        /// Gets or creates the module static constructor of this metadata image. That is, the first method that is
        /// executed upon loading the .NET module. 
        /// </summary>
        /// <returns>The module constructor.</returns>
        /// <remarks>
        /// If the static constructor was not present in the image, the new one is automatically added.
        /// </remarks>
        public MethodDefinition GetOrCreateModuleConstructor()
        {
            var cctor = GetModuleConstructor();
            if (cctor == null)
            {
                cctor = new MethodDefinition(".cctor",
                    MethodAttributes.Private
                    | MethodAttributes.Static
                    | MethodAttributes.SpecialName
                    | MethodAttributes.RuntimeSpecialName,
                    new MethodSignature(TypeSystem.Void));

                cctor.CilMethodBody = new CilMethodBody(cctor);
                cctor.CilMethodBody.Instructions.Add(CilInstruction.Create(CilOpCodes.Ret));
                GetModuleType().Methods.Insert(0, cctor);
            }

            return cctor;
        }

        public TypeDefinition GetModuleType()
        {
            return TryResolveMember(new MetadataToken(MetadataTokenType.TypeDef, 1), out var member)
                ? (TypeDefinition) member
                : null;
        }

        private void EnsureTypeTreeIsInitialized()
        {
            if (_nestedClasses == null)
                InitializeTypeTree();
        }

        private void InitializeTypeTree()
        {
            _nestedClasses = new Dictionary<uint, ICollection<uint>>();

            var tableStream = Header.GetStream<TableStream>();
            var typeTable = (TypeDefinitionTable) tableStream.GetTable(MetadataTokenType.TypeDef);
            var nestedClassTable = (NestedClassTable) tableStream.GetTable(MetadataTokenType.NestedClass);

            var allNestedTypes = new HashSet<uint>();
            foreach (var row in nestedClassTable)
            {
                uint type = row.Column1;
                uint enclosingType = row.Column2;

                if (!_nestedClasses.TryGetValue(enclosingType, out var nestedTypes))
                {
                    nestedTypes = new List<uint>();
                    _nestedClasses.Add(enclosingType, nestedTypes);
                }

                nestedTypes.Add(row.MetadataToken.Rid);
                allNestedTypes.Add(type);
            }

            _topLevelTypes = new List<uint>();
            foreach (var row in typeTable)
            {
                if (!allNestedTypes.Contains(row.MetadataToken.Rid)) 
                    _topLevelTypes.Add(row.MetadataToken.Rid);
            }
        }

        internal ICollection<uint> GetTopLevelTypes()
        {
            EnsureTypeTreeIsInitialized();
            return _topLevelTypes;
        }

        internal ICollection<uint> GetNestedClasses(uint typeRid)
        {
            EnsureTypeTreeIsInitialized();
            if (!_nestedClasses.TryGetValue(typeRid, out var collection))
            {
                collection = new List<uint>();
                _nestedClasses.Add(typeRid, collection);
            }
             
            return collection;
        }
       
        internal bool TryGetCachedMember(MetadataToken token, out IMetadataMember member)
        {
            return _cachedMembers.TryGetValue(token, out member);
        }

        internal void CacheMember(IMetadataMember member)
        {
            if (member.MetadataToken.Rid == 0)
                throw new ArgumentException("Cannot cache metadata members that do not have a metadata token yet.");
            _cachedMembers[member.MetadataToken] = member;
        }

        /// <summary>
        /// Resolves a member by its metadata token.
        /// </summary>
        /// <param name="token">The token to resolve.</param>
        /// <returns>The resolved metadata member.</returns>
        /// <exception cref="MemberResolutionException">Occurs when the metadata token is invalid for this image.</exception>
        public IMetadataMember ResolveMember(MetadataToken token)
        {
            if (!TryResolveMember(token, out var member))
                throw new MemberResolutionException($"Invalid metadata token {token}.");
            return member;
        }

        /// <summary>
        /// Attempts to resolves a member by its metadata token.
        /// </summary>
        /// <param name="token">The token to resolve.</param>
        /// <param name="member">The resolved metadata member.</param>
        /// <returns>True if the resolution succeeded, false otherwise.</returns>
        public bool TryResolveMember(MetadataToken token, out IMetadataMember member)
        {
            if (!TryGetCachedMember(token, out member))
            {
                var tableStream = Header.GetStream<TableStream>();

                if (!tableStream.TryResolveRow(token, out var row))
                    return false;

                member = tableStream.GetTable(token.TokenType).GetMemberFromRow(this, row);
                CacheMember(member);
            }

            return true;
        }
    }
}
