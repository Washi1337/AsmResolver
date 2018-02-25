using System.Linq;
using AsmResolver.Net.Builder;
using AsmResolver.Net.Cts.Collections;
using AsmResolver.Net.Metadata;

namespace AsmResolver.Net.Cts
{
  public class EventDefinition : MetadataMember<MetadataRow<ushort, uint, uint>>, IHasSemantics, IResolvable
    {
        private readonly LazyValue<string> _name;
        private readonly LazyValue<ITypeDefOrRef> _eventType;
        private readonly LazyValue<EventMap> _eventMap;

        public EventDefinition(string name, ITypeDefOrRef eventType)
            : base(null, new MetadataToken(MetadataTokenType.Event))
        {
            _name = new LazyValue<string>(name);
            _eventType = new LazyValue<ITypeDefOrRef>(eventType);

            _eventMap = new LazyValue<EventMap>(default(EventMap));
            
            CustomAttributes = new CustomAttributeCollection(this);
            Semantics = new MethodSemanticsCollection(this);
        }

        internal EventDefinition(MetadataImage image, MetadataRow<EventAttributes, uint, uint> row)
            : base(image, row.MetadataToken)
        {
            Attributes = (EventAttributes)row.Column1;
            _name = new LazyValue<string>(() => image.Header.GetStream<StringStream>().GetStringByOffset(row.Column2));

            _eventType = new LazyValue<ITypeDefOrRef>(() =>
            {
                var enoder = image.Header.GetStream<TableStream>().GetIndexEncoder(CodedIndex.TypeDefOrRef);
                var eventTypeToken = enoder.DecodeIndex(row.Column3);
                IMetadataMember member;
                return image.TryResolveMember(eventTypeToken, out member) ? (ITypeDefOrRef) member : null;
            });
            
            _eventMap = new LazyValue<EventMap>(() =>
            {
                var table = image.Header.GetStream<TableStream>().GetTable(MetadataTokenType.EventMap);
                var mapRow = table.GetRowClosestToKey(1, row.MetadataToken.Rid);
                return mapRow != null ? (EventMap) table.GetMemberFromRow(image, mapRow) : null;
            });
            
            CustomAttributes = new CustomAttributeCollection(this);
            Semantics = new MethodSemanticsCollection(this);
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
            get { return _eventMap.Value; }
            set { _eventMap.Value = value; }
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
            get;
            private set;
        }

        public CustomAttributeCollection CustomAttributes
        {
            get;
            private set;
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

        public IMetadataMember Resolve()
        {
            return this;
        }

        public override string ToString()
        {
            return FullName;
        }
    }
}