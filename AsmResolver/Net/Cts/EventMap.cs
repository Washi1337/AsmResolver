using System;
using System.Runtime.InteropServices;
using AsmResolver.Collections.Generic;
using AsmResolver.Net.Builder;
using AsmResolver.Net.Cts.Collections;
using AsmResolver.Net.Metadata;

namespace AsmResolver.Net.Cts
{
    public class EventMap : MetadataMember<MetadataRow<uint, uint>>
    {
        private readonly LazyValue<TypeDefinition> _parent;

        public EventMap(TypeDefinition parent)
            : base(null, new MetadataToken(MetadataTokenType.EventMap))
        {
            _parent = new LazyValue<TypeDefinition>(parent);
            Events = new DelegatedMemberCollection<EventMap, EventDefinition>(this, GetEventOwner, SetEventOwner);
        }


        internal EventMap(MetadataImage image, MetadataRow<uint, uint> row)
            : base(image, row.MetadataToken)
        {
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

        public TypeDefinition Parent
        {
            get { return _parent.Value; }
            set { _parent.Value = value; }
        }

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

        public override void AddToBuffer(MetadataBuffer buffer)
        {
            var tableStream = buffer.TableStreamBuffer;

            foreach (var @event in Events)
                @event.AddToBuffer(buffer);
            
            tableStream.GetTable<EventMapTable>().Add(new MetadataRow<uint, uint>
            {
                Column1 = Parent.MetadataToken.Rid,
                Column2 = Events.Count == 0 
                    ? (uint) Math.Max(1, tableStream.GetTable(MetadataTokenType.Event).Count) 
                    : Events[0].MetadataToken.Rid,
            });
        }
    }
}