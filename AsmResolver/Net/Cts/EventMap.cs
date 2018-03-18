using AsmResolver.Collections.Generic;
using AsmResolver.Net.Cts.Collections;
using AsmResolver.Net.Metadata;

namespace AsmResolver.Net.Cts
{
    /// <summary>
    /// Provides a map that binds a <see cref="TypeDefinition"/> to a collection of <see cref="EventDefinition"/>s.
    /// </summary>
    public class EventMap : MetadataMember<MetadataRow<uint, uint>>
    {
        private readonly LazyValue<TypeDefinition> _parent;
        private MetadataImage _image;

        public EventMap()
            : base(new MetadataToken(MetadataTokenType.EventMap))
        {
            _parent = new LazyValue<TypeDefinition>();
            Events = new DelegatedMemberCollection<EventMap, EventDefinition>(this, GetEventOwner, SetEventOwner);
        }


        internal EventMap(MetadataImage image, MetadataRow<uint, uint> row)
            : base(row.MetadataToken)
        {
            _image = image;
            _parent = new LazyValue<TypeDefinition>(() =>
            {
                var typeTable = image.Header.GetStream<TableStream>().GetTable(MetadataTokenType.TypeDef);
                MetadataRow typeRow;
                return typeTable.TryGetRow((int) row.Column1 - 1, out typeRow)
                    ? (TypeDefinition) typeTable.GetMemberFromRow(image, typeRow)
                    : null;
            });
            
            Events = new RangedMemberCollection<EventMap,EventDefinition>(this, MetadataTokenType.Event, 1, GetEventOwner, SetEventOwner);
        }

        /// <inheritdoc />
        public override MetadataImage Image
        {
            get { return _parent.IsInitialized && _parent.Value != null ? _parent.Value.Image : _image; }
        }

        /// <summary>
        /// Gets the type the event map was assigned to.
        /// </summary>
        public TypeDefinition Parent
        {
            get { return _parent.Value; }
            internal set
            {
                _parent.Value = value;
                _image = null;
            }
        }

        /// <summary>
        /// Gets a collection of events the <see cref="Parent"/> type declares.
        /// </summary>
        public Collection<EventDefinition> Events
        {
            get;
            private set;
        }

        private static EventMap GetEventOwner(EventDefinition @event)
        {
            return @event.EventMap;
        }

        private static void SetEventOwner(EventDefinition @event, EventMap owner)
        {
            @event.EventMap = owner;
        }
    }
}