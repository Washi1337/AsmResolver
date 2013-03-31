using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TUP.AsmResolver.NET.Specialized
{
    public class EventMap : MetaDataMember 
    {
        MemberRange<EventDefinition> eventRange = null;
        TypeDefinition parent = null;

        public TypeDefinition Parent
        {
            get
            {
                if (parent == null)
                {
                    MetaDataTable table = netheader.TablesHeap.GetTable(MetaDataTableType.TypeDef);
                    int index = Convert.ToInt32(metadatarow.parts[0]) - 1;
                    if (index > 0 && index < table.members.Length)
                        parent = table.members[index] as TypeDefinition;
                }
                return parent;
            }
        }
        public EventDefinition[] Events
        {
            get
            {
                if (eventRange == null)
                {
                    eventRange = MemberRange.CreateRange<EventDefinition>(this, 1, netheader.TablesHeap.GetTable(MetaDataTableType.Event, false));
                }
                return eventRange.Members;

                //return Convert.ToUInt32(metadatarow.parts[1]); 
            }
        }
        public override void ClearCache()
        {
            eventRange = null;
            parent = null;
        }
        

    }
}
