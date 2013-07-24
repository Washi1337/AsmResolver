using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TUP.AsmResolver.NET.Specialized
{
    public class EventDefinition : MemberReference
    {
        MethodDefinition _addmethod = null;
        MethodDefinition _removemethod = null;
        string _name = null;
        TypeReference _eventType = null;
        TypeDefinition _declaringType = null;

        public EventDefinition(MetaDataRow row)
            : base(row)
        {
        }

        public EventDefinition(string name, EventAttributes attributes, TypeReference eventType)
            : base(new MetaDataRow((ushort)attributes, 0U, 0U))
        {
            this._name = name;
            this._eventType = eventType;
        }

        public EventAttributes Attributes
        {
            get { return (EventAttributes)Convert.ToUInt16(_metadatarow._parts[0]); }
            set { _metadatarow._parts[0] = (ushort)value; }
        }

        public override string Name
        {
            get
            {
                if (string.IsNullOrEmpty(_name))
                    _netheader.StringsHeap.TryGetStringByOffset(Convert.ToUInt32(_metadatarow._parts[1]), out _name);
                return _name;
            }
        }

        public TypeReference EventType
        {
            get
            {
                if (_eventType == null)
                    _netheader.TablesHeap.TypeDefOrRef.TryGetMember(Convert.ToInt32(_metadatarow._parts[2]), out _eventType);
                return _eventType;
            }
        }

        public MethodDefinition AddMethod
        {
            get
            {
                if (_addmethod == null)
                {
                    foreach (MetaDataMember member in _netheader.TablesHeap.GetTable(MetaDataTableType.MethodSemantics).Members)
                    {
                        MethodSemantics semantics = (MethodSemantics)member;
                        if (semantics.Association._metadatatoken == this._metadatatoken && (semantics.Attributes & MethodSemanticsAttributes.AddOn) == MethodSemanticsAttributes.AddOn)
                        {
                            _addmethod = semantics.Method;
                            break;
                        }
                    }
                }
                return _addmethod;
            }
        }

        public MethodDefinition RemoveMethod
        {
            get
            {
                if (_removemethod  == null)
                {
                    foreach (MetaDataMember member in _netheader.TablesHeap.GetTable(MetaDataTableType.MethodSemantics).Members)
                    {
                        MethodSemantics semantics = (MethodSemantics)member;
                        if (semantics.Association._metadatatoken == this._metadatatoken && (semantics.Attributes & MethodSemanticsAttributes.RemoveOn) == MethodSemanticsAttributes.RemoveOn)
                        {
                            _removemethod = semantics.Method;
                            break;
                        }
                    }
                }
                return _removemethod;
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
                if (_declaringType == null)
                {
                    MetaDataTable eventMapTable = _netheader.TablesHeap.GetTable(MetaDataTableType.EventMap);
                    foreach (var member in eventMapTable.Members)
                    {
                        EventMap eventMap = member as EventMap;
                        if (eventMap.Events.Contains(this))
                        {
                            _declaringType = eventMap.Parent;
                            break;
                        }
                    }
                }
                return _declaringType;
            }
        }

        public override string ToString()
        {
            return Name;
        }

        public override void ClearCache()
        {
            _addmethod = null;
            _removemethod = null;
            _name = null;
            _eventType = null;
        }

        public override void LoadCache()
        {
            _addmethod = AddMethod;
            _removemethod = RemoveMethod;
            _name = Name;
            _eventType = EventType;
        }

    }
}
