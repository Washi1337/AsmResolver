using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
using AsmResolver.Net.Builder;
using AsmResolver.Net.Signatures;

namespace AsmResolver.Net.Metadata
{
    public class SecurityDeclarationTable : MetadataTable<SecurityDeclaration>
    {
        public override MetadataTokenType TokenType
        {
            get { return MetadataTokenType.DeclSecurity; }
        }

        public override uint GetElementByteCount()
        {
            return sizeof (ushort) +
                   (uint)TableStream.GetIndexEncoder(CodedIndex.HasDeclSecurity).IndexSize +
                   (uint)TableStream.BlobIndexSize;
        }

        protected override SecurityDeclaration ReadMember(MetadataToken token, ReadingContext context)
        {
            var reader = context.Reader;
            return new SecurityDeclaration(Header, token, new MetadataRow<ushort, uint, uint>()
            {
                Column1 = reader.ReadUInt16(),
                Column2 = reader.ReadIndex(TableStream.GetIndexEncoder(CodedIndex.HasDeclSecurity).IndexSize),
                Column3 = reader.ReadIndex(TableStream.BlobIndexSize),
            });
        }

        protected override void UpdateMember(NetBuildingContext context, SecurityDeclaration member)
        {
            var row = member.MetadataRow;
            row.Column1 = (ushort)member.Action;
            row.Column2 = TableStream.GetIndexEncoder(CodedIndex.HasDeclSecurity)
                .EncodeToken(member.Parent.MetadataToken);
            row.Column3 = context.GetStreamBuffer<BlobStreamBuffer>().GetBlobOffset(member.PermissionSet);
        }

        protected override void WriteMember(WritingContext context, SecurityDeclaration member)
        {
            var writer = context.Writer;
            var row = member.MetadataRow;
            writer.WriteUInt16(row.Column1);
            writer.WriteIndex(TableStream.GetIndexEncoder(CodedIndex.HasDeclSecurity).IndexSize, row.Column2);
            writer.WriteIndex(TableStream.BlobIndexSize, row.Column3);
        }
    }

    public class SecurityDeclaration : MetadataMember<MetadataRow<ushort,uint,uint>>, IHasCustomAttribute
    {
        private CustomAttributeCollection _customAttributes;

        public SecurityDeclaration(MetadataHeader header, MetadataToken token, MetadataRow<ushort, uint, uint> row)
            : base(header, token, row)
        {
            Action = (SecurityAction)row.Column1;

            var tableStream = header.GetStream<TableStream>();
            var parentToken = tableStream.GetIndexEncoder(CodedIndex.HasDeclSecurity).DecodeIndex(row.Column2);
            if (parentToken.Rid != 0)
                Parent = (IHasSecurityAttribute)tableStream.ResolveMember(parentToken);

            PermissionSet = DataBlobSignature.FromReader(header.GetStream<BlobStream>().CreateBlobReader(row.Column3));
        }

        public IHasSecurityAttribute Parent
        {
            get;
            set;
        }

        public SecurityAction Action
        {
            get;
            set;
        }

        public DataBlobSignature PermissionSet
        {
            get;
            set;
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
