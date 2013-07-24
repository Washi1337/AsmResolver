using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TUP.AsmResolver.NET.Specialized
{
    public class Constant : MetaDataMember
    {
        object value;
        MetaDataMember parent;

        public Constant(MetaDataRow row)
            : base(row)
        {
        }

        public Constant(MetaDataMember parent, ElementType type, uint signature)
            : base(new MetaDataRow((byte)type, 0U, signature))
        {
        }

        public ElementType ConstantType
        {
            get { return (Specialized.ElementType)Convert.ToByte(metadatarow._parts[0]); }
        }

        public MetaDataMember Parent
        {
            get 
            {
                if (parent != null || netheader.TablesHeap.HasConstant.TryGetMember(Convert.ToInt32(metadatarow._parts[2]), out parent))
                    return parent;
                
                return null;
            }
        }

        public uint Signature
        {
            get { return Convert.ToUInt32(metadatarow._parts[3]); }
        }

        public object Value
        {
            get
            {
                if (value == null)
                    value = netheader.BlobHeap.ReadConstantValue(ConstantType, Signature);
                return value;

            }
        }

        public override void ClearCache()
        {
            value = null;
            parent = null;
        }

        public override void LoadCache()
        {
            value = Value;
            parent = Parent;
        }
    }
}
