using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TUP.AsmResolver.NET.Specialized
{
    public class ExportedType : MetaDataMember
    {
        MetaDataMember _implementation;

        public ExportedType(MetaDataRow row) 
            : base(row)
        {
        }

        public TypeAttributes Attributes { get { return (TypeAttributes)Convert.ToUInt32(_metadatarow._parts[0]); } }
        public uint TypeID { get { return Convert.ToUInt32(_metadatarow._parts[1]); } }
        public string TypeName { get { return _netheader.StringsHeap.GetStringByOffset(Convert.ToUInt32(_metadatarow._parts[2])); } }
        public string TypeNamespace { get { return _netheader.StringsHeap.GetStringByOffset(Convert.ToUInt32(_metadatarow._parts[3])); } }
        public MetaDataMember Implementation
        {
            get
            {
                if (_implementation == null)
                    _netheader.TablesHeap.Implementation.TryGetMember(Convert.ToInt32(_metadatarow._parts[4]), out _implementation);
                return _implementation;
            }
        }

        public override void ClearCache()
        {
            _implementation = null;
        }

        public override void LoadCache()
        {
            _implementation = Implementation;
        }

    }
}
