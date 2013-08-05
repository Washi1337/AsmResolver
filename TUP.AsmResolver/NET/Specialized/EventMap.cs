using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TUP.AsmResolver.NET.Specialized
{
    public class EventMap : MetaDataMember 
    {
        MemberRange<EventDefinition> _eventRange = null;
        TypeDefinition _parent = null;

        public EventMap(MetaDataRow row)
            : base(row)
        {
        }

        public EventMap(TypeDefinition parent, uint startingIndex)
            :base(new MetaDataRow(parent.TableIndex, startingIndex))
        {
            this._parent = parent;
        }

        public TypeDefinition Parent
        {
            get
            {
                if (_parent == null)
                {
                    _netheader.TablesHeap.GetTable(MetaDataTableType.TypeDef).TryGetMember(Convert.ToInt32(_metadatarow._parts[0]), out _parent);
                }
                return _parent;
            }
        }

        public EventDefinition[] Events
        {
            get
            {
                if (_eventRange == null)
                {
                    _eventRange = MemberRange.CreateRange<EventDefinition>(this, 1, _netheader.TablesHeap.GetTable(MetaDataTableType.Event, false));
                }
                return _eventRange.Members;

                //return Convert.ToUInt32(metadatarow.parts[1]); 
            }
        }

        public bool HasEvents
        {
            get { return Events != null && Events.Length > 0; }
        }

        public override void ClearCache()
        {   
            _eventRange = null;
            _parent = null;
        }

        public override void LoadCache()
        {
            _eventRange = MemberRange.CreateRange<EventDefinition>(this, 1, _netheader.TablesHeap.GetTable(MetaDataTableType.Event, false));
            _eventRange.LoadCache();
            _parent = Parent;
        }
    }
}
