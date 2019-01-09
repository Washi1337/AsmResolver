using System;
using System.Collections.Generic;
using AsmResolver.Net.Metadata;

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
