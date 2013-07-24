using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TUP.AsmResolver.NET.Specialized
{
    public class FieldMarshal : MetaDataMember
    {
        MetaDataMember parent = null;

        public FieldMarshal(MetaDataRow row)
            : base(row)
        {
        }

        public FieldMarshal(MetaDataMember parent, uint nativeType)
            :base(new MetaDataRow((uint)0, nativeType))
        {
            this.parent = parent;
        }

        public MetaDataMember Parent
        {
            get
            {
                if (parent == null)
                {
                    netheader.TablesHeap.HasFieldMarshall.TryGetMember(Convert.ToInt32(metadatarow._parts[0]), out parent);
                }
                return parent;
                
            }
        }

        public uint NativeType
        {
            get { return Convert.ToUInt32(metadatarow._parts[1]); }
        }

        public override void ClearCache()
        {
            parent = null;
        }

        public override void LoadCache()
        {
            parent = Parent;
        }
    }
}
