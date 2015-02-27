using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AsmResolver.Net.Builder;

namespace AsmResolver.Net.Metadata
{
    public class GenericParameterConstraintTable : MetadataTable<GenericParameterConstraint>
    {
        public override MetadataTokenType TokenType
        {
            get { return MetadataTokenType.GenericParamConstraint; }
        }

        public override uint GetElementByteCount()
        {
            return (uint)TableStream.GetTable<GenericParameter>().IndexSize +
                   (uint)TableStream.GetIndexEncoder(CodedIndex.TypeDefOrRef).IndexSize;
        }

        protected override GenericParameterConstraint ReadMember(MetadataToken token, ReadingContext context)
        {
            var reader = context.Reader;
            return new GenericParameterConstraint(Header, token, new MetadataRow<uint, uint>()
            {
                Column1 = reader.ReadIndex(TableStream.GetTable<GenericParameter>().IndexSize),
                Column2 = reader.ReadIndex(TableStream.GetIndexEncoder(CodedIndex.TypeDefOrRef).IndexSize)
            });
        }

        protected override void UpdateMember(NetBuildingContext context, GenericParameterConstraint member)
        {
            var row = member.MetadataRow;
            row.Column1 = member.Owner.MetadataToken.Rid;
            row.Column2 = TableStream.GetIndexEncoder(CodedIndex.TypeDefOrRef)
                .EncodeToken(member.Constraint.MetadataToken);
        }

        protected override void WriteMember(WritingContext context, GenericParameterConstraint member)
        {
            var writer = context.Writer;
            var row = member.MetadataRow;

            writer.WriteIndex(TableStream.GetTable<GenericParameter>().IndexSize, row.Column1);
            writer.WriteIndex(TableStream.GetIndexEncoder(CodedIndex.TypeDefOrRef).IndexSize, row.Column2);
        }
    }

    public class GenericParameterConstraint : MetadataMember<MetadataRow<uint, uint>>
    {
        internal GenericParameterConstraint(MetadataHeader header, MetadataToken token, MetadataRow<uint, uint> row)
            : base(header, token, row)
        {
            var tableStream = header.GetStream<TableStream>();
            Owner = tableStream.GetTable<GenericParameter>()[(int)(row.Column1 - 1)];

            var constraintToken = tableStream.GetIndexEncoder(CodedIndex.TypeDefOrRef).DecodeIndex(row.Column2);
            if (constraintToken.Rid != 0)
                Constraint = (ITypeDefOrRef)tableStream.ResolveMember(constraintToken);
        }

        public GenericParameter Owner
        {
            get;
            set;
        }

        public ITypeDefOrRef Constraint
        {
            get;
            set;
        }
    }
}
