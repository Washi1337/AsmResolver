using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AsmResolver.Net.Builder;
using AsmResolver.Net.Signatures;

namespace AsmResolver.Net.Metadata
{
    public class ConstantTable : MetadataTable<Constant>
    {
        public override MetadataTokenType TokenType
        {
            get { return MetadataTokenType.Constant; }
        }

        public override uint GetElementByteCount()
        {
            return sizeof(byte) + 
                   sizeof(byte) +
                   (uint)TableStream.GetIndexEncoder(CodedIndex.HasConstant).IndexSize +
                   (uint)TableStream.BlobIndexSize;
        }

        protected override Constant ReadMember(MetadataToken token, ReadingContext context)
        {
            var reader = context.Reader;
            return new Constant(Header, token, new MetadataRow<byte, byte, uint, uint>()
            {
                Column1 = reader.ReadByte(),
                Column2 = reader.ReadByte(),
                Column3 = reader.ReadIndex(TableStream.GetIndexEncoder(CodedIndex.HasConstant).IndexSize),
                Column4 = reader.ReadIndex(TableStream.BlobIndexSize)
            });
        }

        protected override void UpdateMember(NetBuildingContext context, Constant member)
        {
            var row = member.MetadataRow;
            row.Column1 = (byte)member.ConstantType;
            row.Column3 = TableStream.GetIndexEncoder(CodedIndex.HasConstant).EncodeToken(member.Parent.MetadataToken);
            row.Column4 = context.GetStreamBuffer<BlobStreamBuffer>().GetBlobOffset(member.Value);
        }

        protected override void WriteMember(WritingContext context, Constant member)
        {
            var writer = context.Writer;
            var row = member.MetadataRow;

            writer.WriteByte(row.Column1);
            writer.WriteByte(row.Column2);
            writer.WriteIndex(TableStream.GetIndexEncoder(CodedIndex.HasConstant).IndexSize, row.Column3);
            writer.WriteIndex(TableStream.BlobIndexSize, row.Column4);
        }
    }

    public class Constant : MetadataMember<MetadataRow<byte, byte, uint, uint>>
    {
        private readonly LazyValue<IHasConstant> _parent;
        private readonly LazyValue<DataBlobSignature> _value;

        internal Constant(MetadataHeader header, MetadataToken token, MetadataRow<byte, byte, uint, uint> row)
            : base(header, token, row)
        {
            ConstantType = (ElementType)row.Column1;

            _parent = new LazyValue<IHasConstant>(() =>
            {
                var tableStream = header.GetStream<TableStream>();
                var hasConstantToken = tableStream.GetIndexEncoder(CodedIndex.HasConstant).DecodeIndex(row.Column3);
                return hasConstantToken.Rid != 0 ? (IHasConstant)tableStream.ResolveMember(hasConstantToken) : null;
            });

            _value = new LazyValue<DataBlobSignature>(() => 
                DataBlobSignature.FromReader(header.GetStream<BlobStream>().CreateBlobReader(row.Column4)));
        }

        public ElementType ConstantType
        {
            get;
            set;
        }

        public IHasConstant Parent
        {
            get { return _parent.Value; }
            set { _parent.Value = value; }
        }

        public DataBlobSignature Value
        {
            get { return _value.Value; }
            set { _value.Value = value; }
        }
    }
}
