using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AsmResolver.Net.Builder;

namespace AsmResolver.Net.Metadata
{
    public class ModuleDefinitionTable : MetadataTable<ModuleDefinition>
    {

        public override MetadataTokenType TokenType
        {
            get { return MetadataTokenType.Module; }
        }

        public override uint GetElementByteCount()
        {
            return sizeof (ushort) +                    // Generation
                   (uint)TableStream.StringIndexSize +  // Name
                   (uint)TableStream.GuidIndexSize +    // Mvid
                   (uint)TableStream.GuidIndexSize +    // EncId
                   (uint)TableStream.GuidIndexSize;     // EncBaseId
        }

        protected override ModuleDefinition ReadMember(MetadataToken token, ReadingContext context)
        {
            var reader = context.Reader;
            var row = new MetadataRow<ushort, uint, uint, uint, uint>
            {
                Column1 = reader.ReadUInt16(),                           // Generation
                Column2 = reader.ReadIndex(TableStream.StringIndexSize), // Name
                Column3 = reader.ReadIndex(TableStream.GuidIndexSize),   // Mvid
                Column4 = reader.ReadIndex(TableStream.GuidIndexSize),   // EncId
                Column5 = reader.ReadIndex(TableStream.GuidIndexSize)    // EncBaseId
            };
            return new ModuleDefinition(Header, token, row);
        }

        protected override void UpdateMember(NetBuildingContext context, ModuleDefinition member)
        {
            var guidStream = context.GetStreamBuffer<GuidStreamBuffer>();

            var row = member.MetadataRow;
            row.Column1 = member.Generation;
            row.Column2 = context.GetStreamBuffer<StringStreamBuffer>().GetStringOffset(member.Name);
            row.Column3 = guidStream.GetGuidOffset(member.Mvid);
            row.Column4 = guidStream.GetGuidOffset(member.EncId);
            row.Column5 = guidStream.GetGuidOffset(member.EncBaseId);
        }

        protected override void WriteMember(WritingContext context, ModuleDefinition member)
        {
            var writer = context.Writer;
            var row = member.MetadataRow;

            writer.WriteUInt16(row.Column1);
            writer.WriteIndex(TableStream.StringIndexSize, row.Column2);
            writer.WriteIndex(TableStream.GuidIndexSize, row.Column3);
            writer.WriteIndex(TableStream.GuidIndexSize, row.Column4);
            writer.WriteIndex(TableStream.GuidIndexSize, row.Column5);
        }
    }

    public class ModuleDefinition : MetadataMember<MetadataRow<ushort, uint, uint, uint, uint>>, IHasCustomAttribute, IResolutionScope
    {
        private CustomAttributeCollection _customAttributes;

        public ModuleDefinition(string name)
            : base(null, new MetadataToken(MetadataTokenType.Module), new MetadataRow<ushort, uint, uint, uint, uint>())
        {
            Name = name;
        }

        public ModuleDefinition(MetadataHeader header, MetadataToken token, MetadataRow<ushort, uint, uint, uint, uint> row)
            : base(header, token, row)
        {
            var stringStream = header.GetStream<StringStream>();
            var guidStream = header.GetStream<GuidStream>();

            Generation = row.Column1;
            Name = stringStream.GetStringByOffset(row.Column2);
            Mvid = guidStream.GetGuidByOffset(row.Column3);
            EncId = guidStream.GetGuidByOffset(row.Column4);
            EncBaseId = guidStream.GetGuidByOffset(row.Column5);
        }

        public ushort Generation
        {
            get;
            set;
        }

        public string Name
        {
            get;
            set;
        }

        public Guid Mvid
        {
            get;
            set;
        }

        public Guid EncId
        {
            get;
            set;
        }

        public Guid EncBaseId
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
                _customAttributes = new CustomAttributeCollection(this);
                return _customAttributes;
            }
        }
    }
}
