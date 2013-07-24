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

        public TypeAttributes Attributes { get { return (TypeAttributes)Convert.ToUInt32(metadatarow._parts[0]); } }
        public uint TypeID { get { return Convert.ToUInt32(metadatarow._parts[1]); } }
        public string TypeName { get { return netheader.StringsHeap.GetStringByOffset(Convert.ToUInt32(metadatarow._parts[2])); } }
        public string TypeNamespace { get { return netheader.StringsHeap.GetStringByOffset(Convert.ToUInt32(metadatarow._parts[3])); } }
        public MetaDataMember Implementation
        {
            get
            {
                if (implementation == null)
                    netheader.TablesHeap.Implementation.TryGetMember(Convert.ToInt32(metadatarow._parts[4]), out implementation);
                return implementation;
            }
        }

        public override void ClearCache()
        {
            implementation = null;
        }

        public override void LoadCache()
        {
            implementation = Implementation;
        }

    }
}
