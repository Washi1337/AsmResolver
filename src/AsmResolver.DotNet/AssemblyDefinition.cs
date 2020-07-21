using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using AsmResolver.Collections;
using AsmResolver.DotNet.Builder;
using AsmResolver.DotNet.Collections;
using AsmResolver.DotNet.Serialized;
using AsmResolver.DotNet.Signatures;
using AsmResolver.PE;
using AsmResolver.PE.Builder;
using AsmResolver.PE.DotNet;
using AsmResolver.PE.DotNet.Builder;
using AsmResolver.PE.DotNet.Metadata.Tables;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;
using AsmResolver.PE.File;

namespace AsmResolver.DotNet
{
    /// <summary>
    /// Represents an assembly of self-describing modules of an executable file hosted by a common language runtime (CLR).
    /// </summary>
    public class AssemblyDefinition : AssemblyDescriptor, IHasSecurityDeclaration
    {
        /// <summary>
        /// Reads a .NET assembly from the provided input buffer.
        /// </summary>
        /// <param name="buffer">The raw contents of the executable file to load.</param>
        /// <returns>The module.</returns>
        /// <exception cref="BadImageFormatException">Occurs when the image does not contain a valid .NET metadata directory.</exception>
        public static AssemblyDefinition FromBytes(byte[] buffer) => FromImage(PEImage.FromBytes(buffer));

        /// <summary>
        /// Reads a .NET assembly from the provided input file.
        /// </summary>
        /// <param name="filePath">The file path to the input executable to load.</param>
        /// <returns>The module.</returns>
        /// <exception cref="BadImageFormatException">Occurs when the image does not contain a valid .NET metadata directory.</exception>
        public static AssemblyDefinition FromFile(string filePath) => FromImage(
            PEImage.FromFile(filePath),
            new ModuleReadParameters(Path.GetDirectoryName(filePath)));

        /// <summary>
        /// Reads a .NET assembly from the provided input file.
        /// </summary>
        /// <param name="file">The portable executable file to load.</param>
        /// <returns>The module.</returns>
        /// <exception cref="BadImageFormatException">Occurs when the image does not contain a valid .NET metadata directory.</exception>
        public static AssemblyDefinition FromFile(PEFile file) => FromImage(PEImage.FromFile(file));

        /// <summary>
        /// Reads a .NET assembly from an input stream.
        /// </summary>
        /// <param name="reader">The input stream pointing at the beginning of the executable to load.</param>
        /// <returns>The module.</returns>
        /// <exception cref="BadImageFormatException">Occurs when the image does not contain a valid .NET metadata directory.</exception>
        public static AssemblyDefinition FromReader(IBinaryStreamReader reader) => FromImage(PEImage.FromReader(reader));

        /// <summary>
        /// Initializes a .NET assembly from a PE image.
        /// </summary>
        /// <param name="peImage">The image containing the .NET metadata.</param>
        /// <returns>The module.</returns>
        /// <exception cref="BadImageFormatException">Occurs when the image does not contain a valid .NET metadata directory.</exception>
        public static AssemblyDefinition FromImage(IPEImage peImage) => FromImage(peImage, new ModuleReadParameters());

        /// <summary>
        /// Initializes a .NET assembly from a PE image.
        /// </summary>
        /// <param name="peImage">The image containing the .NET metadata.</param>
        /// <param name="readParameters">The parameters to use while reading the assembly.</param>
        /// <returns>The module.</returns>
        /// <exception cref="BadImageFormatException">Occurs when the image does not contain a valid .NET metadata directory.</exception>
        public static AssemblyDefinition FromImage(IPEImage peImage, ModuleReadParameters readParameters) =>
            ModuleDefinition.FromImage(peImage, readParameters).Assembly
            ?? throw new BadImageFormatException("The provided PE image does not contain an assembly manifest.");

        private IList<ModuleDefinition> _modules;
        private IList<SecurityDeclaration> _securityDeclarations;
        private readonly LazyVariable<byte[]> _publicKey;
        private byte[] _publicKeyToken;

        /// <summary>
        /// Initializes a new assembly definition.
        /// </summary>
        /// <param name="token">The token of the assembly definition.</param>
        protected AssemblyDefinition(MetadataToken token)
            : base(token)
        {
            _publicKey = new LazyVariable<byte[]>(GetPublicKey);
        }

        /// <summary>
        /// Creates a new assembly definition.
        /// </summary>
        /// <param name="name">The name of the assembly.</param>
        /// <param name="version">The version of the assembly.</param>
        public AssemblyDefinition(string name, Version version)
            : this(new MetadataToken(TableIndex.Assembly, 0))
        {
            Name = name;
            Version = version;
        }

        /// <summary>
        /// Gets or sets the hashing algorithm that is used to sign the assembly.
        /// </summary>
        public AssemblyHashAlgorithm HashAlgorithm
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the main module of the .NET assembly containing the assembly's manifest. 
        /// </summary>
        public ModuleDefinition ManifestModule => Modules.Count > 0 ? Modules[0] : null;

        /// <summary>
        /// Gets a collection of modules that this .NET assembly defines.
        /// </summary>
        public IList<ModuleDefinition> Modules
        {
            get
            {
                if (_modules == null)
                    Interlocked.CompareExchange(ref _modules, GetModules(), null);
                return _modules;
            }
        }

        /// <inheritdoc />
        public IList<SecurityDeclaration> SecurityDeclarations
        {
            get
            {
                if (_securityDeclarations is null)
                    Interlocked.CompareExchange(ref _securityDeclarations, GetSecurityDeclarations(), null);
                return _securityDeclarations;
            }
        }

        /// <summary>
        /// Gets or sets the public key of the assembly to use for verification of a signature.
        /// </summary>
        /// <remarks>
        /// <para>If this value is set to <c>null</c>, no public key will be assigned.</para>
        /// <para>This property does not automatically update the <see cref="AssemblyDescriptor.HasPublicKey"/> property.</para>
        /// <para>This property corresponds to the Culture column in the assembly definition table.</para> 
        /// </remarks>
        public byte[] PublicKey
        {
            get => _publicKey.Value;
            set
            {
                _publicKey.Value = value;
                _publicKeyToken = null;
            }
        }

        /// <summary>
        /// Obtains the list of defined modules in the .NET assembly. 
        /// </summary>
        /// <returns>The modules.</returns>
        /// <remarks>
        /// This method is called upon initializing the <see cref="Modules"/> and/or <see cref="ManifestModule"/> property.
        /// </remarks>
        protected virtual IList<ModuleDefinition> GetModules()
            => new OwnedCollection<AssemblyDefinition, ModuleDefinition>(this);

        /// <summary>
        /// Obtains the list of security declarations assigned to the member.
        /// </summary>
        /// <returns>The security declarations</returns>
        /// <remarks>
        /// This method is called upon initialization of the <see cref="SecurityDeclarations"/> property.
        /// </remarks>
        protected virtual IList<SecurityDeclaration> GetSecurityDeclarations() =>
            new OwnedCollection<IHasSecurityDeclaration, SecurityDeclaration>(this);

        /// <summary>
        /// Obtains the public key of the assembly definition.
        /// </summary>
        /// <returns>The public key.</returns>
        /// <remarks>
        /// This method is called upon initializing the <see cref="PublicKey"/> property.
        /// </remarks>
        protected virtual byte[] GetPublicKey() => null;

        /// <inheritdoc />
        public override byte[] GetPublicKeyToken()
        {
            if (!HasPublicKey)
                return PublicKey;

            _publicKeyToken ??= PublicKey != null
                ? ComputePublicKeyToken(PublicKey, HashAlgorithm)
                : null;

            return _publicKeyToken;
        }

        /// <inheritdoc />
        public override AssemblyDefinition Resolve() => this;

        /// <summary>
        /// Rebuilds the .NET assembly to a portable executable file and writes it to the file system. 
        /// </summary>
        /// <param name="filePath">The output path of the manifest module file.</param>
        public void Write(string filePath)
        {
            Write(filePath, new ManagedPEImageBuilder(), new ManagedPEFileBuilder());
        }

        /// <summary>
        /// Rebuilds the .NET assembly to a portable executable file and writes it to the file system. 
        /// </summary>
        /// <param name="filePath">The output path of the manifest module file.</param>
        /// <param name="imageBuilder">The engine to use for reconstructing a PE image.</param>
        public void Write(string filePath, IPEImageBuilder imageBuilder)
        {
            Write(filePath, imageBuilder, new ManagedPEFileBuilder());
        }

        /// <summary>
        /// Rebuilds the .NET assembly to a portable executable file and writes it to the file system. 
        /// </summary>
        /// <param name="filePath">The output path of the manifest module file.</param>
        /// <param name="imageBuilder">The engine to use for reconstructing a PE image.</param>
        /// <param name="fileBuilder">The engine to use for reconstructing a PE file.</param>
        public void Write(string filePath, IPEImageBuilder imageBuilder, IPEFileBuilder fileBuilder)
        {
            string directory = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(directory))
                throw new DirectoryNotFoundException();

            foreach (var module in Modules)
            {
                string modulePath = module == ManifestModule
                    ? filePath
                    : Path.Combine(directory, module.Name);

                module.Write(modulePath, imageBuilder, fileBuilder);
            }
        }

        /// <summary>
        /// Rebuilds the manifest module and writes it to the stream specified. 
        /// </summary>
        /// <param name="stream">The output stream of the manifest module file.</param>
        public void WriteManifest(Stream stream)
        {
            WriteManifest(stream, new ManagedPEImageBuilder(), new ManagedPEFileBuilder());
        }

        /// <summary>
        /// Rebuilds the manifest module and writes it to the stream specified. 
        /// </summary>
        /// <param name="stream">The output stream of the manifest module file.</param>
        /// <param name="imageBuilder">The engine to use for reconstructing a PE image.</param>
        public void WriteManifest(Stream stream, IPEImageBuilder imageBuilder)
        {
            WriteManifest(stream, imageBuilder, new ManagedPEFileBuilder());
        }

        /// <summary>
        /// Rebuilds the manifest module and writes it to the stream specified. 
        /// </summary>
        /// <param name="stream">The output stream of the manifest module file.</param>
        /// <param name="imageBuilder">The engine to use for reconstructing a PE image.</param>
        /// <param name="fileBuilder">The engine to use for reconstructing a PE file.</param>
        public void WriteManifest(Stream stream, IPEImageBuilder imageBuilder, IPEFileBuilder fileBuilder)
        {
            ManifestModule.Write(stream, imageBuilder, fileBuilder);
        }
    }
}