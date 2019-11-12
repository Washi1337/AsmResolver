using System;
using AsmResolver.Lazy;
using AsmResolver.PE;
using AsmResolver.PE.DotNet.Metadata;
using AsmResolver.PE.DotNet.Metadata.Tables;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;
using AsmResolver.PE.File;

namespace AsmResolver.DotNet
{
    /// <summary>
    /// Represents an assembly of self-describing modules of an executable file hosted by a common language runtime (CLR).
    /// </summary>
    public class AssemblyDefinition : IMetadataMember, IAssemblyName
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
        public static AssemblyDefinition FromFile(string filePath) => FromImage(PEImage.FromFile(filePath));

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
        public static AssemblyDefinition FromImage(IPEImage peImage)
        {
            if (peImage.DotNetDirectory == null)
                throw new BadImageFormatException("Input PE image does not contain a .NET directory.");
            if (peImage.DotNetDirectory.Metadata == null)
                throw new BadImageFormatException("Input PE image does not contain a .NET metadata directory.");
            return FromMetadata(peImage.DotNetDirectory.Metadata);
        }
        
        /// <summary>
        /// Initializes a .NET module from a .NET metadata directory.
        /// </summary>
        /// <param name="metadata">The object providing access to the underlying metadata streams.</param>
        /// <returns>The module.</returns>
        public static AssemblyDefinition FromMetadata(IMetadata metadata)
        {
            var stream = metadata.GetStream<TablesStream>();
            var table = stream.GetTable<AssemblyDefinitionRow>();
            return new SerializedAssemblyDefinition(metadata, new MetadataToken(TableIndex.Assembly, 1), table[0],
                ModuleDefinition.FromMetadata(metadata));
        }
        
        private readonly LazyVariable<string> _name;
        private readonly LazyVariable<string> _culture;
        private readonly LazyVariable<byte[]> _publicKey;

        /// <summary>
        /// Initializes a new assembly definition.
        /// </summary>
        /// <param name="token">The token of the assembly definition.</param>
        protected AssemblyDefinition(MetadataToken token)
        {
            MetadataToken = token;
            _name = new LazyVariable<string>(GetName);
            _culture = new LazyVariable<string>(GetCulture);
            _publicKey = new LazyVariable<byte[]>(GetPublicKey);
        }

        /// <summary>
        /// Creates a new assembly definition.
        /// </summary>
        /// <param name="name">The name of the assembly.</param>
        /// <param name="version">The version of the assembly.</param>
        public AssemblyDefinition(string name, Version version)
            : this(new MetadataToken(TableIndex.Assembly, 1))
        {
            Name = name;
            Version = version;
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
        /// This property corresponds to the Name column in the assembly definition table. 
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
        /// This property corresponds to the MajorVersion, MinorVersion, BuildNumber and RevisionNumber columns in the assembly definition table. 
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
        /// This property corresponds to the Attributes column in the assembly definition table. 
        /// </remarks>
        public AssemblyAttributes Attributes
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the locale string of the assembly (if available).
        /// </summary>
        /// <remarks>
        /// <para>If this value is set to <c>null</c>, the default locale will be used</para>
        /// <para>This property corresponds to the Culture column in the assembly definition table.</para> 
        /// </remarks>
        public string Culture
        {
            get => _culture.Value;
            set => _culture.Value = value;
        }

        /// <summary>
        /// Gets or sets the public key of the assembly to use for verification of a signature.
        /// </summary>
        /// <remarks>
        /// <para>If this value is set to <c>null</c>, no public key will be assigned.</para>
        /// <para>This property corresponds to the Culture column in the assembly definition table.</para> 
        /// </remarks>
        public byte[] PublicKey
        {
            get => _publicKey.Value;
            set => _publicKey.Value = value;
        }

        /// <inheritdoc />
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
    }
}