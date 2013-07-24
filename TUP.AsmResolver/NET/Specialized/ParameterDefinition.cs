using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TUP.AsmResolver.NET.Specialized
{
    public class ParameterDefinition : MetaDataMember
    {
        private string _name = null;

        public ParameterDefinition(MetaDataRow row)
            : base(row)
        {
        }

        public ParameterDefinition(string name, ElementType parameterType, ParameterAttributes attributes, ushort sequence)
            : base(new MetaDataRow((uint)attributes, sequence, 0U, (uint)parameterType))
        {
        }

        public ParameterAttributes Attributes
        {
            get{return (ParameterAttributes)_metadatarow._parts[0];}
        }

        public ushort Sequence
        {
            get { return Convert.ToUInt16(_metadatarow._parts[1]); }
        }

        public string Name
        {
            get
            {
                if (string.IsNullOrEmpty(_name))
                    _netheader.StringsHeap.TryGetStringByOffset(Convert.ToUInt32(_metadatarow._parts[2]), out _name);
                return _name;
            }
        }

        public ElementType ParameterType
        {
            get;
            internal set;
        }

        public override string ToString()
        {
            return Name;
        }

        public override void ClearCache()
        {
            _name = null;
        }

        public override void LoadCache()
        {
            _name = Name;
        }
    }
}
