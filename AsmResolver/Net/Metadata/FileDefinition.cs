using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AsmResolver.Net.Builder;
using AsmResolver.Net.Signatures;

namespace AsmResolver.Net.Metadata
{
    public class FileDefinitionTable : MetadataTable<FileDefinition>
    {
        public override MetadataTokenType TokenType
        {
            get { return MetadataTokenType.File; }
        }

        public override uint GetElementByteCount()
        {
            return sizeof (uint) +
                   (uint)TableStream.StringIndexSize +
                   (uint)TableStream.BlobIndexSize;
        }

        protected override FileDefinition ReadMember(MetadataToken token, ReadingContext context)
        {
            var reader = context.Reader;
            return new FileDefinition(Header, token, new MetadataRow<uint, uint, uint>()
            {
                Column1 = reader.ReadUInt32(),
                Column2 = reader.ReadIndex(TableStream.StringIndexSize),
                Column3 = reader.ReadIndex(TableStream.BlobIndexSize)
            });
        }

        protected override void UpdateMember(NetBuildingContext context, FileDefinition member)
        {
            var row = member.MetadataRow;
            row.Column1 = (uint)member.Attributes;
            row.Column2 = context.GetStreamBuffer<StringStreamBuffer>().GetStringOffset(member.Name);
            row.Column3 = context.GetStreamBuffer<BlobStreamBuffer>().GetBlobOffset(member.HashValue);
        }

        protected override void WriteMember(WritingContext context, FileDefinition member)
        {
            var writer = context.Writer;
            var row = member.MetadataRow;

            writer.WriteUInt32(row.Column1);
            writer.WriteIndex(TableStream.StringIndexSize, row.Column2);
            writer.WriteIndex(TableStream.BlobIndexSize, row.Column3);
        }
    }

    public class FileDefinition : MetadataMember<MetadataRow<uint, uint, uint>>, IImplementation, IHasCustomAttribute
    {
        private readonly LazyValue<string> _name;
        private readonly LazyValue<DataBlobSignature> _hashValue;
        private CustomAttributeCollection _customAttributes;

        internal FileDefinition(MetadataHeader header, MetadataToken token, MetadataRow<uint, uint, uint> row)
            : base(header, token, row)
        {
            Attributes = (FileAttributes)row.Column1;
            _name = new LazyValue<string>(() => header.GetStream<StringStream>().GetStringByOffset(row.Column1));
            _hashValue = new LazyValue<DataBlobSignature>(() => 
                DataBlobSignature.FromReader(header.GetStream<BlobStream>().CreateBlobReader(row.Column3)));
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
            get
            {
                if (_customAttributes != null)
                    return _customAttributes;
                return _customAttributes = new CustomAttributeCollection(this);
            }
        }
    }
}
