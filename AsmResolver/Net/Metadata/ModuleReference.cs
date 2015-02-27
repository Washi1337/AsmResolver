using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AsmResolver.Net.Builder;

namespace AsmResolver.Net.Metadata
{
    public class ModuleReferenceTable : MetadataTable<ModuleReference>
    {
        public override MetadataTokenType TokenType
        {
            get { return MetadataTokenType.ModuleRef; }
        }

        public override uint GetElementByteCount()
        {
            return (uint)TableStream.StringIndexSize;
        }

        protected override ModuleReference ReadMember(MetadataToken token, ReadingContext context)
        {
            return new ModuleReference(Header, token, new MetadataRow<uint>()
            {
                Column1 = context.Reader.ReadIndex(TableStream.StringIndexSize)
            });
        }

        protected override void UpdateMember(NetBuildingContext context, ModuleReference member)
        {
            member.MetadataRow.Column1 = context.GetStreamBuffer<StringStreamBuffer>().GetStringOffset(member.Name);
        }

        protected override void WriteMember(WritingContext context, ModuleReference member)
        {
            context.Writer.WriteIndex(TableStream.StringIndexSize, member.MetadataRow.Column1);
        }
    }

    public class ModuleReference : MetadataMember<MetadataRow<uint>>, IHasCustomAttribute, IMemberRefParent, IResolutionScope
    {
        private CustomAttributeCollection _customAttributes;

        public ModuleReference(string name)
            : base(null, new MetadataToken(MetadataTokenType.ModuleRef), new MetadataRow<uint>())
        {
            Name = name;
        }

        internal ModuleReference(MetadataHeader header, MetadataToken token, MetadataRow<uint> row)
            : base(header, token, row)
        {
            Name = header.GetStream<StringStream>().GetStringByOffset(row.Column1);
        }

        public string Name
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
