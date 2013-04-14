using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TUP.AsmResolver.NET.Specialized
{
    public class ExportedType : MetaDataMember
    {
        MetaDataMember implementation;

        public ExportedType(MetaDataRow row) 
            : base(row)
        {
        }

        public TypeAttributes Attributes { get { return (TypeAttributes)Convert.ToUInt32(metadatarow.parts[0]); } }
        public uint TypeID { get { return Convert.ToUInt32(metadatarow.parts[1]); } }
        public string TypeName { get { return netheader.StringsHeap.GetStringByOffset(Convert.ToUInt32(metadatarow.parts[2])); } }
        public string TypeNamespace { get { return netheader.StringsHeap.GetStringByOffset(Convert.ToUInt32(metadatarow.parts[3])); } }
        public MetaDataMember Implementation
        {
            get
            {
                if (implementation == null)
                    implementation = netheader.TablesHeap.Implementation.GetMember(Convert.ToInt32(metadatarow.parts[4]));
                return implementation;
            }
        }
        public override void ClearCache()
        {
            implementation = null;
        }

    }
}
