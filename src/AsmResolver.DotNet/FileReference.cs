using AsmResolver.DotNet.Collections;
using AsmResolver.Lazy;
using AsmResolver.PE.DotNet.Metadata.Tables;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;

namespace AsmResolver.DotNet
{
    /// <summary>
    /// Represents a reference to an external file that a .NET module depends on.
    /// </summary>
    public class FileReference : IImplementation, IOwnedCollectionElement<ModuleDefinition>
    {
        private readonly LazyVariable<string> _name;

        /// <summary>
        /// Initializes the file reference with a metadata token.
        /// </summary>
        /// <param name="token">The metadata token.</param>
        protected FileReference(MetadataToken token)
        {
            MetadataToken = token;
            _name = new LazyVariable<string>(GetName);
        }

        /// <summary>
        /// Creates a new reference to an external file. 
        /// </summary>
        /// <param name="name">The name of the file.</param>
        /// <param name="attributes">The attributes associated to the reference.</param>
        public FileReference(string name, FileAttributes attributes)
        {
            Name = name;
            Attributes = attributes;
        }

        /// <inheritdoc />
        public MetadataToken MetadataToken
        {
            get;
            protected set;
        }

        /// <summary>
        /// Gets or sets the attributes associated to the file reference.
        /// </summary>
        public FileAttributes Attributes
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating the referenced file contains .NET metadata.
        /// </summary>
        public bool ContainsMetadata
        {
            get => !ContainsNoMetadata;
            set => ContainsNoMetadata = !value;
        }

        /// <summary>
        /// Gets or sets a value indicating the referenced file does not contain .NET metadat.
        /// </summary>
        public bool ContainsNoMetadata
        {
            get => Attributes == FileAttributes.ContainsNoMetadata;
            set => Attributes = (Attributes & ~FileAttributes.ContainsNoMetadata)
                                | (value ? FileAttributes.ContainsNoMetadata : 0);
        }

        /// <inheritdoc />
        public string Name
        {
            get => _name.Value;
            set => _name.Value = value;
        }

        /// <inheritdoc />
        public string FullName => Name;

        /// <inheritdoc />
        public ModuleDefinition Module
        {
            get;
            private set;
        }

        ModuleDefinition IOwnedCollectionElement<ModuleDefinition>.Owner
        {
            get => Module;
            set => Module = value;
        }
        
        /// <summary>
        /// Obtains the name of the referenced file.
        /// </summary>
        /// <returns>The name.</returns>
        /// <remarks>
        /// This method is called upon initializing the <see cref="Name"/> property.
        /// </remarks>
        protected virtual string GetName() => null;
    }
}