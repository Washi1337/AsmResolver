using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AsmResolver.Net.Builder;

namespace AsmResolver.Net.Metadata
{
    public class EventPtrTable : MetadataTable<EventPtr>
    {
        public override MetadataTokenType TokenType
        {
            get { return MetadataTokenType.EventPtr; }
        }

        public override uint GetElementByteCount()
        {
            return (uint)TableStream.GetTable<EventDefinition>().IndexSize;
        }

        protected override EventPtr ReadMember(MetadataToken token, ReadingContext context)
        {
            return new EventPtr(Header, token, new MetadataRow<uint>()
            {
                Column1 = context.Reader.ReadIndex(TableStream.GetTable<EventDefinition>().IndexSize)
            });
        }

        protected override void UpdateMember(NetBuildingContext context, EventPtr member)
        {
            member.MetadataRow.Column1 = member.Event.MetadataToken.Rid;
        }

        protected override void WriteMember(WritingContext context, EventPtr member)
        {
            context.Writer.WriteIndex(TableStream.GetTable<EventDefinition>().IndexSize, member.MetadataRow.Column1);
        }
    }

    public class EventPtr : MetadataMember<MetadataRow<uint>>
    {
        internal EventPtr(MetadataHeader header, MetadataToken token, MetadataRow<uint> row)
            : base(header, token, row)
        {
            Event = header.GetStream<TableStream>().GetTable<EventDefinition>()[(int)(row.Column1 - 1)];
        }

        public EventDefinition Event
        {
            get;
            set;
        }
    }
}
