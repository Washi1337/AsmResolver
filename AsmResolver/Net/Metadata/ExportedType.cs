using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AsmResolver.Net.Builder;

namespace AsmResolver.Net.Metadata
{
    public class ExportedTypeTable : MetadataTable<ExportedType>
    {
        public override MetadataTokenType TokenType
        {
            get { return MetadataTokenType.ExportedType; }
        }

        public override uint GetElementByteCount()
        {
            return sizeof (uint) +
                   sizeof (uint) +
                   (uint)TableStream.StringIndexSize +
                   (uint)TableStream.StringIndexSize +
                   (uint)TableStream.GetIndexEncoder(CodedIndex.Implementation).IndexSize;
        }

        protected override ExportedType ReadMember(MetadataToken token, ReadingContext context)
        {
            var reader = context.Reader;
            return new ExportedType(Header, token, new MetadataRow<uint, uint, uint, uint, uint>()
            {
                Column1 = reader.ReadUInt32(),
                Column2 = reader.ReadUInt32(),
                Column3 = reader.ReadIndex(TableStream.StringIndexSize),
                Column4 = reader.ReadIndex(TableStream.StringIndexSize),
                Column5 = reader.ReadIndex(TableStream.GetIndexEncoder(CodedIndex.Implementation).IndexSize),
            });
        }

        protected override void UpdateMember(NetBuildingContext context, ExportedType member)
        {
            var stringStream = context.GetStreamBuffer<StringStreamBuffer>();

            var row = member.MetadataRow;
            row.Column1 = (uint)member.Attributes;
            row.Column2 = member.TypeDefId;
            row.Column3 = stringStream.GetStringOffset(member.TypeName);
            row.Column4 = stringStream.GetStringOffset(member.TypeNamespace);
            row.Column5 = TableStream.GetIndexEncoder(CodedIndex.Implementation)
                .EncodeToken(member.Implementation.MetadataToken);
        }

        protected override void WriteMember(WritingContext context, ExportedType member)
        {
            throw new NotImplementedException();
        }
    }

    public class ExportedType : MetadataMember<MetadataRow<uint, uint, uint, uint, uint>>, IImplementation, IHasCustomAttribute
    {
        private readonly LazyValue<string> _typeName;
        private readonly LazyValue<string> _typeNamespace;
        private readonly LazyValue<IImplementation> _implementation;
        private CustomAttributeCollection _customAttributes;

        internal ExportedType(MetadataHeader header, MetadataToken token, MetadataRow<uint, uint, uint, uint, uint> row)
            : base(header, token, row)
        {
            var tableStream = header.GetStream<TableStream>();
            var stringStream = header.GetStream<StringStream>();

            Attributes = (TypeAttributes)row.Column1;
            TypeDefId = row.Column2;
            _typeName = new LazyValue<string>(() => stringStream.GetStringByOffset(row.Column3));
            _typeNamespace = new LazyValue<string>(() => stringStream.GetStringByOffset(row.Column4));
            _implementation = new LazyValue<IImplementation>(() =>
            {
                var implementationToken = tableStream.GetIndexEncoder(CodedIndex.Implementation)
                    .DecodeIndex(row.Column5);
                return implementationToken.Rid != 0
                    ? Implementation = (IImplementation)tableStream.ResolveMember(implementationToken)
                    : null;
            });
        }

        public TypeAttributes Attributes
        {
            get;
            set;
        }

        public uint TypeDefId
        {
            get;
            set;
        }

        public string TypeName
        {
            get { return _typeName.Value; }
            set { _typeName.Value = value; }
        }

        public string TypeNamespace
        {
            get { return _typeNamespace.Value; }
            set { _typeNamespace.Value = value; }
        }

        public IImplementation Implementation
        {
            get { return _implementation.Value; }
            set { _implementation.Value = value; }
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

        string IImplementation.Name
        {
            get { return TypeName; }
        }
    }
}
