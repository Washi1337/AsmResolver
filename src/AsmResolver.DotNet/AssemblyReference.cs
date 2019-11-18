using System;
using AsmResolver.DotNet.Collections;
using AsmResolver.Lazy;
using AsmResolver.PE.DotNet.Metadata.Tables;

namespace AsmResolver.DotNet
{
    /// <summary>
    /// Represents a reference to an external .NET assembly, hosted by a common language runtime (CLR). 
    /// </summary>
    public class AssemblyReference : AssemblyDescriptor, IResolutionScope, IOwnedCollectionElement<ModuleDefinition>
    {
        private readonly LazyVariable<byte[]> _publicKeyOrToken;

        /// <summary>
        /// Initializes a new assembly reference.
        /// </summary>
        /// <param name="token">The token of the assembly reference.</param>
        protected AssemblyReference(MetadataToken token) 
            : base(token)
        {
            _publicKeyOrToken = new LazyVariable<byte[]>(GetPublicKeyOrToken);
        }

        /// <summary>
        /// Creates a new assembly reference.
        /// </summary>
        /// <param name="name">The name of the assembly.</param>
        /// <param name="version">The version of the assembly.</param>
        public AssemblyReference(string name, Version version)
            : base(new MetadataToken(TableIndex.AssemblyRef, 0))
        {
            Name = name;
            Version = version;
        }

        /// <inheritdoc />
        public ModuleDefinition Module
        {
            get;
            private set;
        }

        /// <inheritdoc />
        ModuleDefinition IOwnedCollectionElement<ModuleDefinition>.Owner
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
        public byte[] PublicKeyOrToken
        {
            get => _publicKeyOrToken.Value;
            set => _publicKeyOrToken.Value = value;
        }

        /// <inheritdoc />
        public override byte[] GetPublicKeyToken()
        {
            if (HasPublicKey)
                throw new NotImplementedException();
            return PublicKeyOrToken;
        }

        /// <summary>
        /// Obtains the public key or token of the assembly reference.
        /// </summary>
        /// <returns>The public key or token.</returns>
        /// <remarks>
        /// This method is called upon initializing the <see cref="PublicKeyOrToken"/> property.
        /// </remarks>
        protected virtual byte[] GetPublicKeyOrToken() => null;
    }
}