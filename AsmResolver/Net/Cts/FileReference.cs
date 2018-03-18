using AsmResolver.Net.Cts.Collections;
using AsmResolver.Net.Metadata;
using AsmResolver.Net.Signatures;

namespace AsmResolver.Net.Cts
{
    public class FileReference : MetadataMember<MetadataRow<FileAttributes, uint, uint>>, IImplementation, IHasCustomAttribute
    {
        private readonly LazyValue<string> _name;
        private readonly LazyValue<DataBlobSignature> _hashValue;
        private MetadataImage _image;

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
            _image = image;
            Attributes = row.Column1;
            
            _name = new LazyValue<string>(() 
                => image.Header.GetStream<StringStream>().GetStringByOffset(row.Column2));
            
            _hashValue = new LazyValue<DataBlobSignature>(() => 
                DataBlobSignature.FromReader(image.Header.GetStream<BlobStream>().CreateBlobReader(row.Column3)));
            
            CustomAttributes = new CustomAttributeCollection(this);
        }

        public override MetadataImage Image
        {
            get { return _image; }
        }

        public FileAttributes Attributes
        {
            get;
            set;
        }

        public string Name
        {
            get { return _name.Value; }
            set { _name.Value = value; }
        }

        public DataBlobSignature HashValue
        {
            get { return _hashValue.Value; }
            set { _hashValue.Value = value; }
        }

        public CustomAttributeCollection CustomAttributes
        {
            get;
            private set;
        }

        public AssemblyDefinition Referrer
        {
            get;
            set;
        }
    }
}
