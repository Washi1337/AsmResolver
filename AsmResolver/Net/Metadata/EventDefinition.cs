using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AsmResolver.Net.Builder;

namespace AsmResolver.Net.Metadata
{
    public class EventDefinitionTable : MetadataTable<EventDefinition>
    {
        public override MetadataTokenType TokenType
        {
            get { return MetadataTokenType.Event; }
        }

        public override uint GetElementByteCount()
        {
            return sizeof (ushort) +
                   (uint)TableStream.StringIndexSize +
                   (uint)TableStream.GetIndexEncoder(CodedIndex.TypeDefOrRef).IndexSize;
        }

        protected override EventDefinition ReadMember(MetadataToken token, ReadingContext context)
        {
            var reader = context.Reader;
            return new EventDefinition(Header, token, new MetadataRow<ushort, uint, uint>()
            {
                Column1 = reader.ReadUInt16(),
                Column2 = reader.ReadIndex(TableStream.StringIndexSize),
                Column3 = reader.ReadIndex(TableStream.GetIndexEncoder(CodedIndex.TypeDefOrRef).IndexSize)
            });
        }

        protected override void UpdateMember(NetBuildingContext context, EventDefinition member)
        {
            var row = member.MetadataRow;
            row.Column1 = (ushort)member.Attributes;
            row.Column2 = context.GetStreamBuffer<StringStreamBuffer>().GetStringOffset(member.Name);
            row.Column3 = TableStream.GetIndexEncoder(CodedIndex.TypeDefOrRef)
                .EncodeToken(member.EventType.MetadataToken);
        }

        protected override void WriteMember(WritingContext context, EventDefinition member)
        {
            var writer = context.Writer;
            var row = member.MetadataRow;

            writer.WriteUInt16(row.Column1);
            writer.WriteIndex(TableStream.StringIndexSize, row.Column2);
            writer.WriteIndex(TableStream.GetIndexEncoder(CodedIndex.TypeDefOrRef).IndexSize, row.Column3);
        }
    }

    public class EventDefinition : MetadataMember<MetadataRow<ushort, uint, uint>>, IHasSemantics, IResolvable, ICollectionItem
    {
        private readonly LazyValue<string> _name;
        private readonly LazyValue<ITypeDefOrRef> _eventType;
        private CustomAttributeCollection _customAttributes;
        private EventMap _eventMap;
        private MethodSemanticsCollection _semantics;

        internal EventDefinition(MetadataHeader header, MetadataToken token, MetadataRow<ushort, uint, uint> row)
            : base(header, token, row)
        {
            Attributes = (EventAttributes)row.Column1;
            _name = new LazyValue<string>(() => header.GetStream<StringStream>().GetStringByOffset(row.Column2));

            _eventType = new LazyValue<ITypeDefOrRef>(() =>
            {
                var tableStream = header.GetStream<TableStream>();
                var eventTypeToken = tableStream.GetIndexEncoder(CodedIndex.TypeDefOrRef).DecodeIndex(row.Column3);
                return eventTypeToken.Rid != 0 ? (ITypeDefOrRef)tableStream.ResolveMember(eventTypeToken) : null;
            });
        }

        public EventAttributes Attributes
        {
            get;
            set;
        }

        public string Name
        {
            get { return _name.Value; }
            set { _name.Value = value; }
        }

        public string FullName
        {
            get
            {
                if (DeclaringType == null)
                    return Name;
                return DeclaringType.FullName + "::" + Name;
            }
        }

        public EventMap EventMap
        {
            get
            {
                if (_eventMap != null)
                    return _eventMap;

                return _eventMap =
                    Header.GetStream<TableStream>()
                        .GetTable<EventMap>()
                        .FirstOrDefault(x => x.Events.Contains(this));
            }
        }

        public TypeDefinition DeclaringType
        {
            get { return EventMap.Parent; }
        }

        ITypeDefOrRef IMemberReference.DeclaringType
        {
            get { return DeclaringType; }
        }

        public ITypeDefOrRef EventType
        {
            get { return _eventType.Value; }
            set { _eventType.Value = value; }
        }

        public MethodSemanticsCollection Semantics
        {
            get
            {
                if (_semantics != null)
                    return _semantics;
                return _semantics = new MethodSemanticsCollection(this);
            }
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

        public MethodDefinition AddMethod
        {
            get
            {
                var semantic = Semantics.FirstOrDefault(x => x.Attributes.HasFlag(MethodSemanticsAttributes.AddOn));
                return semantic != null ? semantic.Method : null;
            }
        }

        public MethodDefinition RemoveMethod
        {
            get
            {
                var semantic = Semantics.FirstOrDefault(x => x.Attributes.HasFlag(MethodSemanticsAttributes.RemoveOn));
                return semantic != null ? semantic.Method : null;
            }
        }

        object ICollectionItem.Owner
        {
            get { return EventMap; }
            set { _eventMap = value as EventMap; }
        }

        public IMetadataMember Resolve()
        {
            return this;
        }
    }
}
