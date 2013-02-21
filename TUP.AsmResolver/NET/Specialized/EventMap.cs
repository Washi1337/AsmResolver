using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TUP.AsmResolver.NET.Specialized
{
    public class EventMap : MetaDataMember 
    {
        EventDefinition[] events = null;
        TypeDefinition parent = null;

        public TypeDefinition Parent
        {
            get
            {
                if (parent == null)
                {
                    MetaDataTable table = netheader.tableheap.GetTable(MetaDataTableType.TypeDef);
                    int index = Convert.ToInt32(metadatarow.parts[0]) - 1;
                    if (index > 0 || index < table.members.Count)
                        parent = table.members[index] as TypeDefinition;
                }
                return parent;
            }
        }
        public EventDefinition[] Events
        {
            get
            {
                if (events == null)
                {
                    int eventlist = Convert.ToInt32(metadatarow.parts[1]);

                    int nexteventlist = -1;

                    if (netheader.TablesHeap.GetTable(MetaDataTableType.EventMap).members.Last().metadatatoken != this.metadatatoken)
                        nexteventlist = Convert.ToInt32(netheader.TokenResolver.ResolveMember(this.MetaDataToken + 1).metadatarow.parts[1]);


                    MetaDataTable eventTable = netheader.tableheap.GetTable(MetaDataTableType.Event);
                    int length = -1;
                    if (nexteventlist != -1)
                        length = nexteventlist - eventlist;
                    else
                        length = eventTable.members.Count - eventlist + 1;


                    if (length > -1)
                    {
                        EventDefinition[] eventdefs = new EventDefinition[length];
                        for (int i = 0; i < eventdefs.Length; i++)
                        {
                            int index = eventlist + i - 1;
                            if (index >= 0 && index < eventTable.members.Count)
                                eventdefs[i] = (EventDefinition)eventTable.members[eventlist + i - 1];
                        }
                        // int actualsize = 0;
                        // for (int i = length-1; i > 0; i--)
                        // {
                        //     if (propertydefs[i] != null)
                        //     {
                        //         actualsize = i;
                        //         break;
                        //     }
                        // }
                        // Array.Resize<PropertyDefinition>(ref propertydefs, actualsize);
                        this.events = eventdefs;
                    }

                }
                return events;

                //return Convert.ToUInt32(metadatarow.parts[1]); 
            }
        }
        public override void ClearCache()
        {
            events = null;
            parent = null;
        }
        

    }
}
