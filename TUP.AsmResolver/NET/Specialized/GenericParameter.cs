using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TUP.AsmResolver.NET.Specialized
{
    public class GenericParameter : TypeReference
    {

        MetaDataMember owner = null;
        string name = string.Empty;

        public ushort Index
        {
            get { return Convert.ToUInt16(metadatarow.parts[0]); }
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
                if (name == string.Empty)
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
