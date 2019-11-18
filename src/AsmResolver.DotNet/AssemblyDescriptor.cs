using System;
using System.Diagnostics;
using AsmResolver.Lazy;
using AsmResolver.PE.DotNet.Metadata.Tables;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;

namespace AsmResolver.DotNet
{
    /// <summary>
    /// Provides a base implementation for describing a self-describing .NET assembly hosted by a common language runtime (CLR).
    /// </summary>
    public abstract class AssemblyDescriptor : IMetadataMember
    {
        private readonly LazyVariable<string> _name;
        private readonly LazyVariable<string> _culture;
        private readonly LazyVariable<byte[]> _publicKey;

        /// <summary>
        /// Initializes a new empty assembly descriptor.
        /// </summary>
        /// <param name="token">The token of the assembly descriptor.</param>
        protected AssemblyDescriptor(MetadataToken token)
        {
            MetadataToken = token;
            _name = new LazyVariable<string>(GetName);
            _culture = new LazyVariable<string>(GetCulture);
            _publicKey = new LazyVariable<byte[]>(GetPublicKey);
        }

        /// <inheritdoc />
        public MetadataToken MetadataToken
        {
            get;
            protected set;
        }

        /// <summary>
        /// Gets or sets the name of the assembly.
        /// </summary>
        /// <remarks>
        /// This property corresponds to the Name column in the assembly table. 
        /// </remarks>
        public string Name
        {
            get => _name.Value;
            set => _name.Value = value;
        }
        
        /// <summary>
        /// Gets or sets the version of the assembly.
        /// </summary>
        /// <remarks>
        /// This property corresponds to the MajorVersion, MinorVersion, BuildNumber and RevisionNumber columns in
        /// the assembly table. 
        /// </remarks>
        public Version Version
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the attributes associated to the assembly.
        /// </summary>
        /// <remarks>
        /// This property corresponds to the Attributes column in the assembly table. 
        /// </remarks>
        public AssemblyAttributes Attributes
        {
            get;
            set;
        }       
        
        /// <summary>
        /// Gets or sets a value indicating whether the assembly holds the full (unhashed) public key.
        /// </summary>
        /// <remarks>
        /// This property does not automatically update after <see cref="PublicKey"/> was updated.
        /// </remarks>
        public bool HasPublicKey
        {
            get => (Attributes & AssemblyAttributes.PublicKey) != 0;
            set => Attributes = (Attributes & ~AssemblyAttributes.PublicKey)
                                | (value ? AssemblyAttributes.PublicKey : 0);
        }
        
        /// <summary>
        /// Gets or sets a value indicating just-in-time (JIT) compiler tracking is enabled for the assembly.
        /// </summary>
        /// <remarks>
        /// This attribute originates from the <see cref="DebuggableAttribute"/> attribute.
        /// </remarks>
        public bool EnableJitCompileTracking 
        {
            get => (Attributes & AssemblyAttributes.EnableJitCompileTracking) != 0;
            set => Attributes = (Attributes & ~AssemblyAttributes.EnableJitCompileTracking)
                                | (value ? AssemblyAttributes.EnableJitCompileTracking : 0);
        }

        /// <summary>
        /// Gets or sets a value indicating any just-in-time (JIT) compiler optimization is disabled for the assembly.
        /// </summary>
        /// <remarks>
        /// This attribute originates from the <see cref="DebuggableAttribute"/> attribute.
        /// </remarks>
        public bool DisableJitCompileOptimizer
        {
            get => (Attributes & AssemblyAttributes.DisableJitCompileOptimizer) != 0;
            set => Attributes = (Attributes & ~AssemblyAttributes.DisableJitCompileOptimizer)
                                | (value ? AssemblyAttributes.DisableJitCompileOptimizer : 0);
        }
        
        /// <summary>
        /// Gets or sets a value indicating whether the assembly contains Windows Runtime (WinRT) code or not.
        /// </summary>
        public bool IsWindowsRuntime
        {
            get => (Attributes & AssemblyAttributes.ContentMask) == AssemblyAttributes.ContentWindowsRuntime;
            set => Attributes = (Attributes & ~AssemblyAttributes.ContentMask)
                                | (value ? AssemblyAttributes.ContentWindowsRuntime : 0);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the assembly can be retargeted (at runtime) to an assembly from
        /// a different publisher.
        /// </summary>
        public bool IsRetargetable
        {
            get => (Attributes & AssemblyAttributes.Retargetable) != 0;
            set => Attributes = (Attributes & ~AssemblyAttributes.Retargetable)
                                | (value ? AssemblyAttributes.Retargetable : 0);
        }
        
        /// <summary>
        /// Gets or sets the public key of the assembly to use for verification of a signature.
        /// </summary>
        /// <remarks>
        /// <para>If this value is set to <c>null</c>, no public key will be assigned.</para>
        /// <para>This property does not automatically update the <see cref="HasPublicKey"/> property.</para>
        /// <para>This property corresponds to the Culture column in the assembly definition table.</para> 
        /// </remarks>
        public byte[] PublicKey
        {
            get => _publicKey.Value;
            set => _publicKey.Value = value;
        }

        /// <summary>
        /// Gets or sets the locale string of the assembly (if available).
        /// </summary>
        /// <remarks>
        /// <para>If this value is set to <c>null</c>, the default locale will be used</para>
        /// <para>This property corresponds to the Culture column in the assembly table.</para> 
        /// </remarks>
        public string Culture
        {
            get => _culture.Value;
            set => _culture.Value = value;
        }

        /// <summary>
        /// When the application is signed with a strong name, obtains the public key token of the assembly 
        /// </summary>
        /// <returns>The token.</returns>
        public byte[] GetPublicKeyToken()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Obtains the name of the assembly definition.
        /// </summary>
        /// <returns>The name.</returns>
        /// <remarks>
        /// This method is called upon initializing the <see cref="Name"/> property.
        /// </remarks>
        protected virtual string GetName() => null;

        /// <summary>
        /// Obtains the locale string of the assembly definition.
        /// </summary>
        /// <returns>The locale string.</returns>
        /// <remarks>
        /// This method is called upon initializing the <see cref="Culture"/> property.
        /// </remarks>
        protected virtual string GetCulture() => null;

        /// <summary>
        /// Obtains the public key of the assembly definition.
        /// </summary>
        /// <returns>The public key.</returns>
        /// <remarks>
        /// This method is called upon initializing the <see cref="PublicKey"/> property.
        /// </remarks>
        protected virtual byte[] GetPublicKey() => null;

        /// <inheritdoc />
        public override string ToString() => $"{Name}, Version={Version}";
    }
}