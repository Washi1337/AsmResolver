using System.Security.Policy;
using AsmResolver.Net.Builder;
using AsmResolver.Net.Cts.Collections;
using AsmResolver.Net.Metadata;
using AsmResolver.Net.Signatures;

namespace AsmResolver.Net.Cts
{
    public class FileReference : MetadataMember<MetadataRow<FileAttributes, uint, uint>>, IImplementation, IHasCustomAttribute
    {
        private readonly LazyValue<string> _name;
        private readonly LazyValue<DataBlobSignature> _hashValue;
        
        public FileReference(string name, FileAttributes attributes, DataBlobSignature hashValue)
            : base(null, new MetadataToken(MetadataTokenType.File))
        {
            _name = new LazyValue<string>(name);
            _hashValue = new LazyValue<DataBlobSignature>(hashValue);
            Attributes = attributes;
            
            CustomAttributes = new CustomAttributeCollection(this);
        }
            
        internal FileReference(MetadataImage image, MetadataRow<FileAttributes, uint, uint> row)
            : base(image, row.MetadataToken)
        {
            Attributes = row.Column1;
            
            _name = new LazyValue<string>(() 
                => image.Header.GetStream<StringStream>().GetStringByOffset(row.Column2));
            
            _hashValue = new LazyValue<DataBlobSignature>(() => 
                DataBlobSignature.FromReader(image.Header.GetStream<BlobStream>().CreateBlobReader(row.Column3)));
            
            CustomAttributes = new CustomAttributeCollection(this);
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

        public override void AddToBuffer(MetadataBuffer buffer)
        {
            buffer.TableStreamBuffer.GetTable<FileReferenceTable>().Add(new MetadataRow<FileAttributes, uint, uint>
            {
                Column1 = Attributes,
                Column2 = buffer.StringStreamBuffer.GetStringOffset(Name),
                Column3 = buffer.BlobStreamBuffer.GetBlobOffset(HashValue)
            });

            foreach (var attribute in CustomAttributes)
                attribute.AddToBuffer(buffer);
        }
    }
}
