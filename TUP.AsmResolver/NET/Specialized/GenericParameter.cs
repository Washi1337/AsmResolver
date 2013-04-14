using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TUP.AsmResolver.NET.Specialized
{
    public class GenericParameter : TypeReference
    {
        internal MetaDataMember owner = null;
        
        public GenericParameter(MetaDataRow row)
            : base(row)
        {
        }

        public GenericParameter(string name, ushort index, GenericParameterAttributes attributes, MetaDataMember owner)
            : base(new MetaDataRow(index, (ushort)attributes, 0U, 0U))
        {
            this.name = name;
            this.owner = owner;
        }

        public ushort Index
        {
            get { return Convert.ToUInt16(metadatarow.parts[0]); }
            set
            {
                metadatarow.parts[0] = value;
            }
        }

        public GenericParameterAttributes GenericAttributes
        {
            get { return (GenericParameterAttributes)Convert.ToUInt16(metadatarow.parts[1]); }
        }

        public MetaDataMember Owner
        {
            get 
            {
                if (owner == null && Convert.ToUInt16(metadatarow.parts[2]) != 0)
                    netheader.TablesHeap.TypeOrMethod.TryGetMember(Convert.ToUInt16(metadatarow.parts[2]), out owner);
                return owner;
            }
        }

        public override string Name
        {
            get {
                if (string.IsNullOrEmpty(name))
                    name = netheader.StringsHeap.GetStringByOffset(Convert.ToUInt32(metadatarow.parts[3]));
                return name;
            }
        }

        public override string Namespace
        {
            get
            {
                return "";
            }
        }

        public override string FullName
        {
            get
            {
                return Name;
            }
        }

    }
}
