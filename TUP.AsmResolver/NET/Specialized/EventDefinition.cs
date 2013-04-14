using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TUP.AsmResolver.NET.Specialized
{
    public class EventDefinition : MemberReference
    {
        MethodDefinition addmethod = null;
        MethodDefinition removemethod = null;
        string name = null;
        TypeReference eventType = null;
        TypeDefinition declaringType = null;

        public EventDefinition(MetaDataRow row)
            : base(row)
        {
        }

        public EventDefinition(string name, EventAttributes attributes, TypeReference eventType)
            : base(new MetaDataRow((ushort)attributes, 0U, 0U))
        {
            this.name = name;
            this.eventType = eventType;
        }

        public EventAttributes Attributes
        {
            get { return (EventAttributes)Convert.ToUInt16(metadatarow.parts[0]); }
            set { metadatarow.parts[0] = (ushort)value; }
        }

        public override string Name
        {
            get
            {
                if (name == null)
                    name = netheader.StringsHeap.GetStringByOffset(Convert.ToUInt32(metadatarow.parts[1]));
                return name;
            }
        }

        public TypeReference EventType
        {
            get
            {
                if (eventType == null)
                    eventType = (TypeReference)netheader.TablesHeap.TypeDefOrRef.GetMember(Convert.ToInt32(metadatarow.parts[2]));
                return eventType;
            }
        }

        public MethodDefinition AddMethod
        {
            get
            {
                if (addmethod == null)
                {
                    foreach (MetaDataMember member in netheader.TablesHeap.GetTable(MetaDataTableType.MethodSemantics).members)
                    {
                        MethodSemantics semantics = (MethodSemantics)member;
                        if (semantics.Association.metadatatoken == this.metadatatoken && (semantics.Attributes & MethodSemanticsAttributes.AddOn) == MethodSemanticsAttributes.AddOn)
                        {
                            addmethod = semantics.Method;
                            break;
                        }
                    }
                }
                return addmethod;
            }
        }

        public MethodDefinition RemoveMethod
        {
            get
            {
                if (removemethod  == null)
                {
                    foreach (MetaDataMember member in netheader.TablesHeap.GetTable(MetaDataTableType.MethodSemantics).members)
                    {
                        MethodSemantics semantics = (MethodSemantics)member;
                        if (semantics.Association.metadatatoken == this.metadatatoken && (semantics.Attributes & MethodSemanticsAttributes.RemoveOn) == MethodSemanticsAttributes.RemoveOn)
                        {
                            removemethod = semantics.Method;
                            break;
                        }
                    }
                }
                return removemethod;
            }
        }

        public override string FullName
        {
            get
            {
                try
                {
                    if (DeclaringType is TypeReference)
                        return DeclaringType.FullName + "::" + Name;

                    return Name;
                }
                catch { return Name; }
            }
        }

        public override TypeReference DeclaringType
        {
            get
            {
                if (declaringType == null)
                {
                    MetaDataTable eventMapTable = netheader.TablesHeap.GetTable(MetaDataTableType.EventMap);
                    foreach (var member in eventMapTable.members)
                    {
                        EventMap eventMap = member as EventMap;
                        if (eventMap.Events.Contains(this))
                        {
                            declaringType = eventMap.Parent;
                            break;
                        }
                    }
                }
                return declaringType;
            }
        }

        public override string ToString()
        {
            return Name;
        }

        public override void ClearCache()
        {
            addmethod = null;
            removemethod = null;
            name = null;
            eventType = null;
        }

    }
}
