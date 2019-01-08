using AsmResolver.Net.Cts.Collections;
using AsmResolver.Net.Metadata;
using AsmResolver.Net.Signatures;

namespace AsmResolver.Net.Cts
{
    /// <summary>
    /// Represents a reference to an external file containing metadata part of the assembly.
    /// </summary>
    public class FileReference : MetadataMember<MetadataRow<FileAttributes, uint, uint>>, IImplementation, IHasCustomAttribute
    {
        private readonly LazyValue<string> _name;
        private readonly LazyValue<DataBlobSignature> _hashValue;

        public FileReference(string name, FileAttributes attributes, DataBlobSignature hashValue)
            : base(new MetadataToken(MetadataTokenType.File))
        {
            _name = new LazyValue<string>(name);
            _hashValue = new LazyValue<DataBlobSignature>(hashValue);
            Attributes = attributes;
            
            CustomAttributes = new CustomAttributeCollection(this);
        }
            
        internal FileReference(MetadataImage image, MetadataRow<FileAttributes, uint, uint> row)
            : base(row.MetadataToken)
        {
            Image = image;
            Attributes = row.Column1;
            
            _name = new LazyValue<string>(() 
                => image.Header.GetStream<StringStream>().GetStringByOffset(row.Column2));
            
            _hashValue = new LazyValue<DataBlobSignature>(() => 
                DataBlobSignature.FromReader(image.Header.GetStream<BlobStream>().CreateBlobReader(row.Column3)));
            
            CustomAttributes = new CustomAttributeCollection(this);
        }

        /// <inheritdoc />
        public override MetadataImage Image
        {
            get;
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
        /// Gets or sets the name of the file to reference.
        /// </summary>
        public string Name
        {
            get => _name.Value;
            set => _name.Value = value;
        }

        /// <summary>
        /// Gets or sets the hash value of the file to reference.
        /// </summary>
        public DataBlobSignature HashValue
        {
            get => _hashValue.Value;
            set => _hashValue.Value = value;
        }

        /// <inheritdoc />
        public CustomAttributeCollection CustomAttributes
        {
            get;
        }

        /// <summary>
        /// Gets the assembly that refers to the file.
        /// </summary>
        public AssemblyDefinition Referrer
        {
            get;
            internal set;
        }
    }
}
