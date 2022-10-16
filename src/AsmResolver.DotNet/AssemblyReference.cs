using System;
using AsmResolver.Collections;
using AsmResolver.PE.DotNet.Metadata.Tables;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;

namespace AsmResolver.DotNet
{
    /// <summary>
    /// Represents a reference to an external .NET assembly, hosted by a common language runtime (CLR).
    /// </summary>
    public class AssemblyReference :
        AssemblyDescriptor,
        IResolutionScope,
        IOwnedCollectionElement<ModuleDefinition>,
        IImplementation
    {
        private readonly LazyVariable<byte[]?> _publicKeyOrToken;
        private readonly LazyVariable<byte[]?> _hashValue;
        private byte[]? _publicKeyToken;

        /// <summary>
        /// Initializes a new assembly reference.
        /// </summary>
        /// <param name="token">The token of the assembly reference.</param>
        protected AssemblyReference(MetadataToken token)
            : base(token)
        {
            _publicKeyOrToken = new LazyVariable<byte[]?>(GetPublicKeyOrToken);
            _hashValue = new LazyVariable<byte[]?>(GetHashValue);
        }

        /// <summary>
        /// Creates a new assembly reference.
        /// </summary>
        /// <param name="name">The name of the assembly.</param>
        /// <param name="version">The version of the assembly.</param>
        public AssemblyReference(string? name, Version version)
            : this(new MetadataToken(TableIndex.AssemblyRef, 0))
        {
            Name = name;
            Version = version;
        }

        /// <summary>
        /// Creates a new assembly reference.
        /// </summary>
        /// <param name="name">The name of the assembly.</param>
        /// <param name="version">The version of the assembly.</param>
        /// <param name="publicKey">Indicates the key provided by <paramref name="publicKeyOrToken"/> is the full,
        /// unhashed public key used to verify the authenticity of the assembly.</param>
        /// <param name="publicKeyOrToken">Indicates the public key or token (depending on <paramref name="publicKey"/>),
        /// used to verify the authenticity of the assembly.</param>
        public AssemblyReference(string? name, Version version, bool publicKey, byte[]? publicKeyOrToken)
            : this(new MetadataToken(TableIndex.AssemblyRef, 0))
        {
            Name = name;
            Version = version;
            HasPublicKey = publicKey;
            PublicKeyOrToken = publicKeyOrToken;
        }

        /// <summary>
        /// Creates a new assembly reference, and copies over all properties of another assembly descriptor.
        /// </summary>
        /// <param name="descriptor">The assembly to base the reference on.</param>
        public AssemblyReference(AssemblyDescriptor descriptor)
            : this(new MetadataToken(TableIndex.AssemblyRef, 0))
        {
            Name = descriptor.Name;
            Version = descriptor.Version;
            Attributes = descriptor.Attributes;
            HasPublicKey = false;

            PublicKeyOrToken = descriptor.GetPublicKeyToken();
            if (PublicKeyOrToken?.Length == 0)
                PublicKeyOrToken = null;

            Culture = descriptor.Culture;
            if (Utf8String.IsNullOrEmpty(Culture))
                Culture = null;
        }

        /// <inheritdoc />
        public ModuleDefinition? Module
        {
            get;
            private set;
        }

        /// <inheritdoc />
        ModuleDefinition? IOwnedCollectionElement<ModuleDefinition>.Owner
        {
            get => Module;
            set => Module = value;
        }

        /// <summary>
        /// Gets or sets the (token of the) public key of the assembly to use for verification of a signature.
        /// </summary>
        /// <remarks>
        /// <para>If this value is set to <c>null</c>, no public key will be assigned.</para>
        /// <para>When <see cref="AssemblyDescriptor.HasPublicKey"/> is set to <c>true</c>, this value contains the full
        /// unhashed public key that was used to sign the assembly. This property does not automatically update the
        /// <see cref="AssemblyDescriptor.HasPublicKey"/> property.</para>
        /// <para>This property corresponds to the Culture column in the assembly definition table.</para>
        /// </remarks>
        public byte[]? PublicKeyOrToken
        {
            get => _publicKeyOrToken.Value;
            set => _publicKeyOrToken.Value = value;
        }

        /// <summary>
        /// Gets or sets the hash value of the assembly reference.
        /// </summary>
        public byte[]? HashValue
        {
            get => _hashValue.Value;
            set => _hashValue.Value = value;
        }

        /// <inheritdoc />
        public override bool IsCorLib => KnownCorLibs.KnownCorLibReferences.Contains(this);

        /// <inheritdoc />
        public override byte[]? GetPublicKeyToken()
        {
            if (!HasPublicKey)
                return PublicKeyOrToken;

            _publicKeyToken ??= PublicKeyOrToken != null
                ? ComputePublicKeyToken(PublicKeyOrToken, Resolve()?.HashAlgorithm ?? AssemblyHashAlgorithm.Sha1)
                : null;

            return _publicKeyToken;
        }

        /// <summary>
        /// Obtains the public key or token of the assembly reference.
        /// </summary>
        /// <returns>The public key or token.</returns>
        /// <remarks>
        /// This method is called upon initializing the <see cref="PublicKeyOrToken"/> property.
        /// </remarks>
        protected virtual byte[]? GetPublicKeyOrToken() => null;

        /// <summary>
        /// Obtains the hash value of the assembly reference.
        /// </summary>
        /// <returns>The hash value.</returns>
        /// <remarks>
        /// This method is called upon initializing the <see cref="HashValue"/> property.
        /// </remarks>
        protected virtual byte[]? GetHashValue() => null;

        /// <inheritdoc />
        public override bool IsImportedInModule(ModuleDefinition module) => Module == module;

        /// <inheritdoc />
        public override AssemblyReference ImportWith(ReferenceImporter importer) =>
            (AssemblyReference) importer.ImportScope(this);

        /// <inheritdoc />
        public override AssemblyDefinition? Resolve() => Module?.MetadataResolver.AssemblyResolver.Resolve(this);

        AssemblyDescriptor IResolutionScope.GetAssembly() => this;
    }
}
