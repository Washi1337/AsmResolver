using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using AsmResolver.Collections;
using AsmResolver.PE.DotNet.Metadata.Tables;
using AsmResolver.Shims;

namespace AsmResolver.DotNet
{
    /// <summary>
    /// Provides a base implementation for describing a self-describing .NET assembly hosted by a common language runtime (CLR).
    /// </summary>
    public abstract partial class AssemblyDescriptor : MetadataMember, IHasCustomAttribute, IFullNameProvider, IImportable
    {
        private const int PublicKeyTokenLength = 8;

        private readonly object _lock = new();

        /// <summary> The internal custom attribute list. </summary>
        /// <remarks> This value may not be initialized. Use <see cref="CustomAttributes"/> instead.</remarks>
        protected IList<CustomAttribute>? CustomAttributesInternal;

        /// <summary>
        /// Initializes a new empty assembly descriptor.
        /// </summary>
        /// <param name="token">The token of the assembly descriptor.</param>
        protected AssemblyDescriptor(MetadataToken token)
            : base(token)
        {
            Version = new Version(0, 0, 0, 0);
        }

        /// <summary>
        /// Gets or sets the name of the assembly.
        /// </summary>
        /// <remarks>
        /// This property corresponds to the Name column in the assembly table.
        /// </remarks>
        [LazyProperty]
        public partial Utf8String? Name
        {
            get;
            set;
        }

        string? INameProvider.Name => Name;

        /// <inheritdoc />
        public string FullName
        {
            get
            {
                string cultureString = !Utf8String.IsNullOrEmpty(Culture)
                    ? Culture
                    : "neutral";

                byte[]? publicKeyToken = GetPublicKeyToken();
                string publicKeyTokenString = publicKeyToken is not null
                    ? StringShim.Join(string.Empty, publicKeyToken.Select(x => x.ToString("x2")))
                    : "null";

                return $"{Name}, Version={Version}, Culture={cultureString}, PublicKeyToken={publicKeyTokenString}";
            }
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

        /// <inheritdoc />
        public virtual bool HasCustomAttributes => CustomAttributesInternal is { Count: > 0 };

        /// <inheritdoc />
        public IList<CustomAttribute> CustomAttributes
        {
            get
            {
                if (CustomAttributesInternal is null)
                    Interlocked.CompareExchange(ref CustomAttributesInternal, GetCustomAttributes(), null);
                return CustomAttributesInternal;
            }
        }

        /// <summary>
        /// Obtains the list of custom attributes assigned to the member.
        /// </summary>
        /// <returns>The attributes</returns>
        /// <remarks>
        /// This method is called upon initialization of the <see cref="CustomAttributes"/> property.
        /// </remarks>
        protected virtual IList<CustomAttribute> GetCustomAttributes() =>
            new OwnedCollection<IHasCustomAttribute, CustomAttribute>(this);

        /// <summary>
        /// Gets or sets the locale string of the assembly (if available).
        /// </summary>
        /// <remarks>
        /// <para>If this value is set to <c>null</c>, the default locale will be used</para>
        /// <para>This property corresponds to the Culture column in the assembly table.</para>
        /// </remarks>
        [LazyProperty]
        public partial Utf8String? Culture
        {
            get;
            set;
        }

        /// <summary>
        /// Gets a value indicating whether the assembly descriptor references a Common Object Runtime standard library.
        /// </summary>
        public abstract bool IsCorLib
        {
            get;
        }

        /// <summary>
        /// When the application is signed with a strong name, obtains the public key token of the assembly
        /// </summary>
        /// <returns>The token.</returns>
        public abstract byte[]? GetPublicKeyToken();

        /// <summary>
        /// Obtains the name of the assembly definition.
        /// </summary>
        /// <returns>The name.</returns>
        /// <remarks>
        /// This method is called upon initializing the <see cref="Name"/> property.
        /// </remarks>
        protected virtual Utf8String? GetName() => null;

        /// <summary>
        /// Obtains the locale string of the assembly definition.
        /// </summary>
        /// <returns>The locale string.</returns>
        /// <remarks>
        /// This method is called upon initializing the <see cref="Culture"/> property.
        /// </remarks>
        protected virtual Utf8String? GetCulture() => null;

        /// <inheritdoc />
        public override string ToString() => FullName;

        /// <inheritdoc />
        public abstract bool IsImportedInModule(ModuleDefinition module);

        /// <summary>
        /// Imports the assembly descriptor using the provided reference importer.
        /// </summary>
        /// <param name="importer">The importer object to use.</param>
        /// <returns>The imported assembly reference.</returns>
        public abstract AssemblyReference ImportWith(ReferenceImporter importer);

        /// <inheritdoc />
        IImportable IImportable.ImportWith(ReferenceImporter importer) => ImportWith(importer);

        /// <summary>
        /// Computes the token of a public key using the provided hashing algorithm.
        /// </summary>
        /// <param name="publicKey">The public key to obtain the token from.</param>
        /// <param name="algorithm">The algorithm to use.</param>
        /// <returns>The public key token.</returns>
        protected static byte[] ComputePublicKeyToken(byte[] publicKey, AssemblyHashAlgorithm algorithm)
        {
            using HashAlgorithm implementation = algorithm switch
            {
                AssemblyHashAlgorithm.None => SHA1.Create(), // Default algo is SHA-1.
                AssemblyHashAlgorithm.Md5 => MD5.Create(),
                AssemblyHashAlgorithm.Sha1 => SHA1.Create(),
                AssemblyHashAlgorithm.Sha256 => SHA256.Create(),
                AssemblyHashAlgorithm.Sha384 => SHA384.Create(),
                AssemblyHashAlgorithm.Sha512 => SHA512.Create(),
                _ => throw new NotSupportedException($"Unsupported hashing algorithm {algorithm}.")
            };

            byte[] hash = implementation.ComputeHash(publicKey);
            byte[] token = new byte[PublicKeyTokenLength];
            for (int i = 0; i < PublicKeyTokenLength; i++)
                token[i] = hash[hash.Length - 1 - i];
            return token;
        }

        /// <summary>
        /// Resolves the reference to the assembly to an assembly definition.
        /// </summary>
        /// <returns>The assembly definition, or <c>null</c> if the resolution failed.</returns>
        public abstract AssemblyDefinition? Resolve();
    }
}
