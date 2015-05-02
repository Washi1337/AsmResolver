using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AsmResolver.Net.Builder;

namespace AsmResolver.Net.Metadata
{
    public class EventMapTable : MetadataTable<EventMap>
    {
        public override MetadataTokenType TokenType
        {
            get { return MetadataTokenType.EventMap; }
        }

        public override uint GetElementByteCount()
        {
            return (uint)TableStream.GetTable<TypeDefinition>().IndexSize + 
                   (uint)TableStream.GetTable<EventDefinition>().IndexSize;
        }

        protected override EventMap ReadMember(MetadataToken token, ReadingContext context)
        {
            var reader = context.Reader;
            return new EventMap(Header, token, new MetadataRow<uint, uint>()
            {
                Column1 = reader.ReadIndex(TableStream.GetTable<TypeDefinition>().IndexSize),
                Column2 = reader.ReadIndex(TableStream.GetTable<EventDefinition>().IndexSize)
            });
        }

        protected override void UpdateMember(NetBuildingContext context, EventMap member)
        {
            var row = member.MetadataRow;
            row.Column1 = member.Parent.MetadataToken.Rid;
            // TODO: update event list.
        }

        protected override void WriteMember(WritingContext context, EventMap member)
        {
            var writer = context.Writer;
            var row = member.MetadataRow;

            writer.WriteUInt32(row.Column1);
            writer.WriteIndex(TableStream.GetTable<EventDefinition>().IndexSize, row.Column2);
        }
    }

    public class EventMap : MetadataMember<MetadataRow<uint, uint>>
    {
        private readonly LazyValue<TypeDefinition> _parent;
        private RangedDefinitionCollection<EventDefinition> _events;

        public EventMap(TypeDefinition parent)
            : base(null, new MetadataToken(MetadataTokenType.EventMap), new MetadataRow<uint, uint>())
        {
            _parent = new LazyValue<TypeDefinition>(parent);
        }

        internal EventMap(MetadataHeader header, MetadataToken token, MetadataRow<uint, uint> row)
            : base(header, token, row)
        {
            var tableStream = header.GetStream<TableStream>();
            _parent = new LazyValue<TypeDefinition>(() => tableStream.GetTable<TypeDefinition>()[(int)row.Column1 - 1]);
        }

        public TypeDefinition Parent
        {
            get { return _parent.Value; }
            set { _parent.Value = value; }
        }

        public RangedDefinitionCollection<EventDefinition> Events
        {
            get
            {
                return _events ??
                       (_events = RangedDefinitionCollection<EventDefinition>.Create(Header, this,
                           x => (int)x.MetadataRow.Column2));
            }
        }
    }
}
